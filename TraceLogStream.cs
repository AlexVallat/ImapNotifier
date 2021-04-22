using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ImapNotifier
{
#if DEBUG
	/// <summary>
	/// Write streamed data line-at-a-time to trace output
	/// </summary>
	internal class TraceLogStream : Stream
	{
		private readonly Encoding _encoding;
		private readonly StringBuilder _buffer = new();

		public TraceLogStream() : this(Encoding.UTF8) {}
		public TraceLogStream(Encoding encoding)
		{
			_encoding = encoding;
		}

		public override bool CanWrite => true;

		public override void Write(byte[] buffer, int offset, int count)
		{
			var value = _encoding.GetString(buffer, offset, count).Split('\n');

			if (value.Length > 1)
			{
				Trace.Write(_buffer.ToString());
				_buffer.Clear();
				for (var i = 0; i < value.Length - 1; i++)
				{
					Trace.Write(value[0]);
				}
			}
			_buffer.Append(value.Last());
		}

		public override void Flush()
		{
			if (_buffer.Length > 0)
			{
				Trace.WriteLine(_buffer.ToString());
				_buffer.Clear();
			}
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override long Length => throw new System.NotSupportedException();
		public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }
		public override int Read(byte[] buffer, int offset, int count) => throw new System.NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new System.NotSupportedException();
		public override void SetLength(long value) => throw new System.NotSupportedException();
	}
#endif
}