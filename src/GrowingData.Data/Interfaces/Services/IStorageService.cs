
namespace GrowingData.Data {
	using System.Collections.Generic;
	using System.IO;
	using GrowingData.Data;

	/// <summary>
	/// Defines the <see cref="IStorageService" />
	/// </summary>
	public interface IStorageService {
		/// <summary>
		/// The DeleteObjects
		/// </summary>
		/// <param name="bucket">The <see cref="StorageBucket"/></param>
		/// <param name="searchPath">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		int DeleteObjects(StorageBucket bucket, string searchPath);

		/// <summary>
		/// Read from a storage blob into the stream provided
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="storagePath"></param>
		/// <param name="stream"></param>
		void Read(StorageBucket bucket, string storagePath, Stream destination);
		/// <summary>
		/// Read from a storage blob into the stream provided
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="storagePath"></param>
		/// <param name="stream"></param>
		void Read(StorageBucket bucket, string storagePath, int maxBytes, Stream destination);

		/// <summary>
		/// The GetTableBlobPath
		/// </summary>
		/// <param name="storagePath">The <see cref="string"/></param>
		/// <param name="table">The <see cref="SqlTable"/></param>
		/// <param name="partition">The <see cref="int"/></param>
		/// <param name="isGzip">The <see cref="bool"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetTableStoragePath(StorageBucket bucket, string storagePath, SqlTable table, int partition, bool isGzip);

		/// <summary>
		/// The ListObjectNames
		/// </summary>
		/// <param name="bucket">The <see cref="StorageBucket"/></param>
		/// <param name="searchPath">The <see cref="string"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		IEnumerable<StorageObject> ListObjects(StorageBucket bucket, string searchPath);


		/// <summary>
		/// The WriteToCloudStorageBlob
		/// </summary>
		/// <param name="bucket">The <see cref="StorageBucket"/></param>
		/// <param name="localPath">The <see cref="string"/></param>
		/// <param name="storagePath">The <see cref="string"/></param>
		/// <param name="mimeType">The <see cref="string"/></param>
		/// <returns>The <see cref="StorageWriteResult"/></returns>
		StorageObject Write(StorageBucket bucket, string storagePath, string mimeType, Stream stream);


		/// <summary>
		/// Copy an object from one location to another
		/// </summary>
		/// <param name="sourceBucket"></param>
		/// <param name="sourcePath"></param>
		/// <param name="destinationBucket"></param>
		/// <param name="destinationPath"></param>
		/// <returns></returns>
		StorageObject Copy(StorageBucket sourceBucket, string sourcePath, StorageBucket destinationBucket, string destinationPath);
	}
}
