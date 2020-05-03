
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace GrowingData.Utilities {
	public class StreamingIO {

		public static void Stream(Action<Stream> writer, Action<Stream> reader) {
			var producerConsumer = new ProducerConsumerStream(65535);
			var writeFinished = false;
			Task.Run(() => {
				try {
					reader(producerConsumer.Reader);
				} finally {
					writeFinished = true;
				}
			});

			writer(producerConsumer.Writer);

			while (!writeFinished) {
				Thread.Sleep(100);
			}

		}
		public static void StreamText(Action<StreamWriter> writer, Action<Stream> reader) {
			var producerConsumer = new ProducerConsumerStream(65535);
			var writeFinished = false;
			Task.Run(() => {
				try {
					reader(producerConsumer.Reader);
				} finally {
					writeFinished = true;
				}
			});

			using (var streamWriter = new StreamWriter(producerConsumer.Writer)) {
				writer(streamWriter);
			}
			while (!writeFinished) {
				Thread.Sleep(100);
			}

		}
	}
}
