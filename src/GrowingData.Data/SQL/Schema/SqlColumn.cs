// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Reflection;
	using GrowingData.Utilities;
	using Newtonsoft.Json;
	using YamlDotNet.Serialization;

	/// <summary>
	/// Defines the <see cref="SqlColumn" />
	/// </summary>
	public class SqlColumn {

		/// <summary>
		/// Gets or sets the ColumnName
		/// </summary>
		[JsonProperty]
		public string ColumnName { get; private set; }



		/// <summary>
		/// Gets the simple type as string
		/// </summary>
		[JsonProperty]
		[YamlMember]
		public string DataType { get; set; }

		/// <summary>
		/// Gets a value indicating whether IsNullable
		/// </summary>
		[JsonProperty]
		[YamlMember]
		public bool IsNullable { get; set; }


		[JsonIgnore]
		[YamlIgnore]
		public Type DotNetType {
			get {
				var simpleType = SimpleDbType.Get(DataType);
				return simpleType.DotNetType;
			}
		}

		[JsonIgnore]
		[YamlIgnore]
		public SimpleDbType SimpleType {
			get {
				return SimpleDbType.Get(DataType);
			}
		}

		/// <summary>
		/// For De-Serialization
		/// </summary>
		public SqlColumn() {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlColumn"/> class.
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="type">The <see cref="Type"/></param>
		public SqlColumn(string name, Type type) {
			// This seems retarded - shouldn't it atleast be ".ToUnderscoreCase()"
			//ColumnName = name.ToLower();
			ColumnName = name.ToDatabaseSafeLabel();
			DataType = SimpleDbType.Get(type).DatabaseType.ToString();
			IsNullable = type.IsGenericType || !type.IsValueType;


		}

		/// <summary>
		/// For includign extra information about the Field (e.g. if it has 
		/// attributes like [SqlPk] associated with it).
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="reflectedField"></param>
		public SqlColumn(string name, Type type, FieldInfo reflectedField) {
			// This seems retarded - shouldn't it atleast be ".ToUnderscoreCase()"
			//ColumnName = name.ToLower();
			ColumnName = name.ToDatabaseSafeLabel();
			DataType = SimpleDbType.Get(type).DatabaseType.ToString();
		}

		/// <summary>
		/// The CompareTo
		/// </summary>
		/// <param name="obj">The <see cref="object"/></param>
		/// <returns>The <see cref="int"/></returns>
		public int CompareTo(object obj) {
			var other = obj as SqlColumn;
			return this.ColumnName.CompareTo(other.ColumnName);
		}

		/// <summary>
		/// The GetHashCode
		/// </summary>
		/// <returns>The <see cref="int"/></returns>
		public override int GetHashCode() {
			return ColumnName.GetHashCode();
		}

		/// <summary>
		/// The MarkTypeNullable
		/// </summary>
		public void MarkTypeNullable() {
			IsNullable = true;
		}
	}
}
