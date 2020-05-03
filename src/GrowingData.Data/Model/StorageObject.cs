using System;

namespace GrowingData.Data {
	public class StorageObject {
		public string Bucket;
		public string Uri;
		public string Name;
		public string BucketPath;
		public long Bytes;
		public string Md5Hash;
		public bool IsFolder;
		public DateTime? CreatedDate;
	}
}
