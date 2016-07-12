using System.IO;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Streams
{
    public class ReportOnCloseStream : Stream
    {
        public Stream _baseStream;

        public delegate void CloseHandler();

        public event CloseHandler OnClose;


        public ReportOnCloseStream(Stream original)
        {
            _baseStream = original;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer,offset, count);
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;

        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        public override void Close()
        {
            base.Close();
            OnClose?.Invoke();
        }
    }
}
