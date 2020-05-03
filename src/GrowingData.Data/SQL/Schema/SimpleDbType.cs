// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Newtonsoft.Json.Linq;


	/// <summary>
	/// Defines the <see cref="SimpleDbType" />
	/// </summary>
	public class SimpleDbType : IEqualityComparer<SimpleDbType>, IComparable<SimpleDbType>, IComparable {
		/// <summary>
		/// Defines the DotNetType
		/// </summary>
		public readonly Type DotNetType;

		/// <summary>
		/// Defines the DatabaseType
		/// </summary>
		public readonly DbType DatabaseType;

		/// <summary>
		/// The ToString
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public override string ToString() {
			return DatabaseType.ToString();
		}

		public string Code => DatabaseType.ToString();

		public readonly bool IsNullable;

		/// <summary>
		/// The Equals
		/// </summary>
		/// <param name="x">The <see cref="SimpleDbType"/></param>
		/// <param name="y">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool Equals(SimpleDbType x, SimpleDbType y) {
			return x.DatabaseType == y.DatabaseType;
		}

		/// <summary>
		/// The GetHashCode
		/// </summary>
		/// <param name="obj">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="int"/></returns>
		public int GetHashCode(SimpleDbType obj) {
			return obj.DatabaseType.GetHashCode();
		}

		/// <summary>
		/// The CompareTo
		/// </summary>
		/// <param name="obj">The <see cref="object"/></param>
		/// <returns>The <see cref="int"/></returns>
		public int CompareTo(object obj) {
			return DatabaseType.CompareTo((obj as SimpleDbType).DatabaseType);
		}

		/// <summary>
		/// The CompareTo
		/// </summary>
		/// <param name="other">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="int"/></returns>
		public int CompareTo(SimpleDbType other) {
			return DatabaseType.CompareTo(other.DatabaseType);
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="SimpleDbType"/> class from being created.
		/// </summary>
		/// <param name="code">The <see cref="SimpleDbTypeCode"/></param>
		/// <param name="dotNetType">The <see cref="Type"/></param>
		/// <param name="dbType">The <see cref="DbType"/></param>
		private SimpleDbType(Type dotNetType, DbType dbType, bool isNullable) {
			DotNetType = dotNetType;
			DatabaseType = dbType;
			IsNullable = isNullable;
		}

		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="simpleCode">The <see cref="SimpleDbTypeCode"/></param>
		/// <returns>The <see cref="SimpleDbType"/></returns>
		public static SimpleDbType Get(DbType dbType) {
			var mt = ValidTypes.FirstOrDefault(t => t.DatabaseType == dbType);
			if (mt == null) {
				throw new KeyNotFoundException($"The DbType '{dbType}' is unsupported (no mapping).");
			}
			return mt;
		}

		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="dotNetType">The <see cref="Type"/></param>
		/// <returns>The <see cref="SimpleDbType"/></returns>
		public static SimpleDbType Get(Type dotNetType) {
			var mt = ValidTypes.FirstOrDefault(t => t.DotNetType == dotNetType);
			if (mt == null) {
				throw new KeyNotFoundException($"Unable to convert dotNetType from '{dotNetType}' to DbType (type is unknown).");
			}
			return mt;
		}

		public static SimpleDbType Get(string typeCode) {
			DbType type;
			if (Enum.TryParse<DbType>(typeCode, out type)) {
				var mt = ValidTypes.FirstOrDefault(t => t.DatabaseType == type);

				if (mt == null) {
					throw new KeyNotFoundException($"SimpleDbType Get: '{typeCode}' is unknown.");
				}
				return mt;
			}
			throw new KeyNotFoundException($"SimpleDbType Get: Unable to parse: '{typeCode}' as a DbType.");
		}

		/// <summary>
		/// Defines the ValidTypes
		/// </summary>
		public static List<SimpleDbType> ValidTypes = new List<SimpleDbType>() {
			new SimpleDbType(typeof(string), DbType.String, true),

			new SimpleDbType(typeof(bool), DbType.Boolean , false),
			new SimpleDbType(typeof(DateTime), DbType.DateTime, false ),
			new SimpleDbType(typeof(double), DbType.Double, false ),
			new SimpleDbType(typeof(decimal), DbType.Decimal, false ),
			new SimpleDbType(typeof(float), DbType.Single , false),
			new SimpleDbType(typeof(long), DbType.Int64 , false),
			new SimpleDbType(typeof(byte), DbType.Byte, false ),
			new SimpleDbType(typeof(short), DbType.Int16, false ),
			new SimpleDbType(typeof(int), DbType.Int32 , false),
			new SimpleDbType(typeof(Guid), DbType.Guid , false),

			new SimpleDbType(typeof(bool?), DbType.Boolean, true ),
			new SimpleDbType(typeof(DateTime?), DbType.DateTime, true ),
			new SimpleDbType(typeof(double?), DbType.Double, true ),
			new SimpleDbType(typeof(decimal?), DbType.Decimal, true ),
			new SimpleDbType(typeof(float?), DbType.Single, true ),
			new SimpleDbType(typeof(long?), DbType.Int64, true ),
			new SimpleDbType( typeof(byte?), DbType.Byte , true),
			new SimpleDbType(typeof(short?), DbType.Int16 , true),
			new SimpleDbType(typeof(int?), DbType.Int32 , true),
			new SimpleDbType(typeof(Guid?), DbType.Guid, true )
		};

		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="type">The <see cref="JTokenType"/></param>
		/// <returns>The <see cref="SimpleDbType"/></returns>
		public static SimpleDbType Get(JTokenType type) {
			Type t = null;
			if (type == JTokenType.String) {
				t = typeof(string);
			}

			if (type == JTokenType.Float) {
				t = typeof(double);
			}

			if (type == JTokenType.Integer) {
				t = typeof(long);
			}

			if (type == JTokenType.Date) {
				t = typeof(DateTime);
			}

			if (type == JTokenType.Boolean) {
				t = typeof(bool);
			}

			if (type == JTokenType.TimeSpan) {
				t = typeof(TimeSpan);
			}
			if (t == null) {
				throw new TypeLoadException($"SimpleDbType.Get(JTokenType)  => Unknown type: {type}");

			}
			return SimpleDbType.Get(t);
		}
	}
}
