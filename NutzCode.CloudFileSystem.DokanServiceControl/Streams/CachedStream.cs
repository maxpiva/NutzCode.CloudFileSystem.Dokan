using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceControl.Cache;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Streams
{
    public class CacheStream : Stream
    {
        private BackedCacheFile _file;
        private long _position;

        public delegate void CloseHandler();

        public event CloseHandler OnClose;


        public CacheStream(BackedCacheFile file)
        {
            _file = file;
        }

        public override void Flush()
        {
            _file.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos=0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset;
                    break;
                case SeekOrigin.Current:
                    pos = _position+offset;
                    break;
                case SeekOrigin.End:
                    pos = _file.Length + offset;
                    break;
            }
            Position = pos;
            return Position;
        }

        public override void SetLength(long value)
        {
            _file.SetLength(value);
            if (value > _position)
                _position = value;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            Tuple<int, long> ret = await _file.ReadAsync(buffer, offset, count, _position, token);
            _position = ret.Item2;
            return ret.Item1;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            Tuple<int, long> ret = await _file.WriteAsync(buffer, offset, count, _position, token);
            _position = ret.Item2;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Task.Run<int>(() => ReadAsync(buffer, offset, count, new CancellationToken())).Result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Task.Run(() => WriteAsync(buffer, offset, count, new CancellationToken()));
        }

        public override bool CanRead  => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _file.Length;

        public override long Position
        {
            get { return _position; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > _file.Length)
                    value = _file.Length;
                _position = value;
            }
        }

        public override void Close()
        {
            base.Close();
            OnClose?.Invoke();
        }
    }
}
