using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NutzCode.CloudFileSystem.DokanServiceControl.Streams;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Cache
{
    public class BackedCacheFile : CacheFile
    {
        public const int BUFFER_SIZE = 104857560;


        private AsyncReaderWriterLock outputlock =new AsyncReaderWriterLock();
        private AsyncReaderWriterLock inputlock =new AsyncReaderWriterLock();

        internal Stream _inputStream;
        internal Stream _outputStream;



        public override Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            using (countLock.WriterLock())
            {
                UseCount++;
                CacheStream s = new CacheStream(this);
                s.OnClose += DecrementUsage;
                return s;
            }
        }

        public override void Close()
        {
            _inputStream?.Close();
            _outputStream?.Close();
            _inputStream?.Dispose();
            _outputStream?.Dispose();
            _inputStream = _outputStream = null;
        }


        public void Flush() //???///
        {
            using (outputlock.ReaderLock())
            {
                _outputStream.Flush();
            }
        }

        public void SetLength(long length)
        {
            using (outputlock.ReaderLock())
            {
                _outputStream.SetLength(length);
                Length = length;
            }
        }

        private async Task<int> ReadFromInput(byte[] buffer, int offset, int count, long position, CancellationToken token)
        {
            using (await inputlock.WriterLockAsync())
            {
                if (_inputStream.Position != position)
                    _inputStream.Position = position;
                return await _inputStream.ReadAsync(buffer, offset, count, token);
            }
        }
        private async Task<int> ReadFromOutput(byte[] buffer, int offset, int count, long position, CancellationToken token)
        {
            using (await outputlock.WriterLockAsync())
            {
                if (_outputStream.Position != position)
                    _outputStream.Position = position;
                return await _outputStream.ReadAsync(buffer, offset, count, token);
            }
        }
        private async Task WriteToOutput(byte[] buffer, int offset, int count, long position, CancellationToken token)
        {
            using (await outputlock.WriterLockAsync())
            {
                if (_outputStream.Position != position)
                    _outputStream.Position = position;
                await _outputStream.WriteAsync(buffer, offset, count, token);
            }
        }

        internal async Task FinishDownloadTask(CancellationToken token)
        {
            try
            {
                ChangeState(() =>
                {
                    State = DesiredState;
                    DesiredState = CacheFileState.UploadAndOverwrite;
                });
                SaveMetadata();
                await FillAsync(token);
            }
            catch (OperationCanceledException e)
            {
                ChangeState(() =>
                {
                    DesiredState = State;
                    State = CacheFileState.Canceled;
                });
            }
            catch (Exception e)
            {
                ChangeState(() =>
                {
                    DesiredState = State;
                    ErrorType = "Error downloading file: " + e;
                    Retries++;
                    State = Retries == Properties.Settings.Default.MaxRetries
                        ? CacheFileState.Error
                        : CacheFileState.Waiting;
                });
            }
        }


        public async Task FillAsync(CancellationToken token)
        {
            using (await rangelock.WriterLockAsync(token))
            {
                long total = CachedRanges.Sum(a => a.EndPosition - a.StartPosition);
                foreach (Range ran in CachedRanges.InCache(0, Length).Where(a=>!a.Found))
                {
                    int read = 0;
                    long len = ran.EndPosition - ran.StartPosition;
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long position = ran.StartPosition;
                    while (len > 0)
                    {
                        int count = len > BUFFER_SIZE ? BUFFER_SIZE : (int)len;
                        int cnt = await ReadFromInput(buffer, 0, count, position, new CancellationToken());
                        if (cnt == 0)
                            break;
                        await WriteToOutput(buffer, 0, cnt, position, new CancellationToken());
                        WasWritten = true;
                        CachedRanges.AddSize(position, read);
                        SaveMetadata();
                        token.ThrowIfCancellationRequested();
                        long upload = CachedRanges.Where(a => a.Found).Sum(a => a.EndPosition - a.StartPosition);
                        FileProgress p=new FileProgress();
                        p.TotalSize = total;
                        p.TransferSize = upload;
                        p.Percentage = (float)upload*100F/(float)total;
                        DoProgress(p);
                        len -= cnt;
                        position += cnt;
                        read += cnt;                        
                    }
                }                
            }
        }

        public async Task<Tuple<int, long>> WriteAsync(byte[] buffer, int offset, int count, long position, CancellationToken token)
        {
            using (await rangelock.WriterLockAsync(token))
            {
                long originalposition = position;
                await WriteToOutput(buffer, offset, count, position, token);
                WasWritten = true;
                position += count;
                CachedRanges.AddSize(originalposition, count);
                SaveMetadata();
                if (position > Length)
                    Length = position;
            }
            return new Tuple<int, long>(count, position);
        }

        public async Task<Tuple<int,long>> ReadAsync(byte[] buffer, int offset, int count, long position, CancellationToken token)
        {
            int cnt = 0;
            int totalread = 0;
            do
            {
                Range range;
                int len = 0;
                using (AsyncReaderWriterLock.UpgradeableReaderKey key=await rangelock.UpgradeableReaderLockAsync(token))
                {
                    range = CachedRanges.FirstBlock(position, count);
                    len = (int) (range.EndPosition - position);
                    if (len > count)
                        len = count;

                    if (range.Found)
                    {
                        while (len > 0)
                        {
                            cnt = await ReadFromOutput(buffer, offset, len, position, token);
                            if (cnt == 0)
                                break;
                            len -= cnt;
                            offset += cnt;
                            count -= cnt;
                            totalread += cnt;
                            position += cnt;
                        }
                    }
                    else
                    {
                        using (await key.UpgradeAsync(token))
                        {
                            range = CachedRanges.FirstBlock(position, count);
                            if (!range.Found)
                            {
                                int read = 0;
                                while (len > 0)
                                {
                                    cnt = await ReadFromInput(buffer, offset, len,position, new CancellationToken());
                                    if (cnt == 0)
                                        break;
                                    await WriteToOutput(buffer, offset, cnt, position, new CancellationToken());
                                    WasWritten = true;
                                    CachedRanges.AddSize(position, read);
                                    SaveMetadata();
                                    token.ThrowIfCancellationRequested();
                                    len -= cnt;
                                    offset += cnt;
                                    count -= cnt;
                                    totalread += cnt;
                                    position += cnt;
                                    read += cnt;
                                }
                            }
                        }
                    }
                }
            } while (count > 0 && cnt > 0);
            return new Tuple<int, long>(totalread,position);
        }
    }
}
