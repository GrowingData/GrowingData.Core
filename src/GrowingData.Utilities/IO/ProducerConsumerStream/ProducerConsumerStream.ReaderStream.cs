/*
MIT License

Copyright(c) 2018 Stephen Cleary

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

https://github.com/StephenCleary/ProducerConsumerStream

*/



using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GrowingData.Utilities {
	public partial class ProducerConsumerStream {
		private sealed class ReaderStream : AsyncStreamBase {
			private readonly ProducerConsumerStream _producerConsumerStream;

			public ReaderStream(ProducerConsumerStream producerConsumerStream) {
				_producerConsumerStream = producerConsumerStream;
			}

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

			public override void SetLength(long value) => throw new NotSupportedException();

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position {
				get => _producerConsumerStream.ReaderPosition;
				set => throw new NotSupportedException();
			}

			protected override Task DoFlushAsync(CancellationToken cancellationToken, bool sync) => Task.FromException(new NotSupportedException());

			protected override Task<int> DoReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync) =>
				_producerConsumerStream.ReadAsync(buffer, offset, count, cancellationToken, sync);

			protected override Task DoWriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync) =>
				Task.FromException(new NotSupportedException());
		}
	}
}
