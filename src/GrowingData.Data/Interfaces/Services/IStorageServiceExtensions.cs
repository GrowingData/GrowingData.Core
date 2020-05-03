using System.IO;
using System.IO.Compression;
using System.Text;
using GrowingData.Data;
using Newtonsoft.Json;

namespace GrowingData.Data {
	public static class IStorageServiceExtensions {

		/// <summary>
		/// Download from the Storage Service into a Local File
		/// </summary>
		/// <param name="storage"></param>
		/// <param name="bucket"></param>
		/// <param name="storagePath"></param>
		/// <param name="localFilePath"></param>
		public static void Download(this IStorageService storage, StorageBucket bucket, string storagePath, string localFilePath) {
			using (var localFile = File.OpenWrite(localFilePath)) {
				storage.Read(bucket, storagePath, localFile);
			}
		}


		public static string PeekString(this IStorageService storage, StorageBucket bucket, string storagePath, int maxBytes) {
			using (var memoryStream = new MemoryStream()) {
				storage.Read(bucket, storagePath, maxBytes, memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				if (storagePath.EndsWith(".gz")) {
					var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
					using (var reader = new StreamReader(gzipStream)) {
						return reader.ReadToEnd();
					}
				}
				using (var reader = new StreamReader(memoryStream)) {
					return reader.ReadToEnd();
				}
			}
		}

		public static string ReadString(this IStorageService storage, StorageBucket bucket, string storagePath) {
			using (var memoryStream = new MemoryStream()) {
				storage.Read(bucket, storagePath, memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				if (storagePath.EndsWith(".gz")) {
					var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
					using (var reader = new StreamReader(gzipStream)) {
						return reader.ReadToEnd();
					}
				}
				using (var reader = new StreamReader(memoryStream)) {
					return reader.ReadToEnd();
				}
			}
		}
		public static T ReadJson<T>(this IStorageService storage, StorageBucket bucket, string storagePath) {
			using (var memoryStream = new MemoryStream()) {
				storage.Read(bucket, storagePath, memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				if (storagePath.EndsWith(".gz")) {
					var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
					using (var reader = new StreamReader(gzipStream)) {
						var json = reader.ReadToEnd();
						return JsonConvert.DeserializeObject<T>(json);
					}
				}
				using (var reader = new StreamReader(memoryStream)) {
					var json = reader.ReadToEnd();
					return JsonConvert.DeserializeObject<T>(json);
				}
			}
		}

		public static StorageObject Write(this IStorageService storage, StorageBucket bucket, string localFilePath, string storagePath, string mimeType) {
			using (var localFile = File.OpenRead(localFilePath)) {
				return storage.Write(bucket, storagePath, mimeType, localFile);
			}
		}


		public static StorageObject WriteString(this IStorageService storage, StorageBucket bucket, string storagePath, string mimeType, string stringContent) {
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringContent))) {
				return storage.Write(bucket, storagePath, mimeType, stream);
			}
		}

		public static StorageObject WriteTable(this IStorageService storage, StorageBucket bucket, string rootFolder, CsvResult csvResult) {
			//var bucketPath = BucketPath(bucket);
			var model = csvResult.TableSchema;
			var storagePath = storage.GetTableStoragePath(bucket, rootFolder, model, csvResult.PartitionKey, csvResult.IsGZip);

			using (var localFile = File.OpenRead(csvResult.FilePath)) {
				return storage.Write(bucket, storagePath, "text/csv", localFile);
			}

		}
	}
}
