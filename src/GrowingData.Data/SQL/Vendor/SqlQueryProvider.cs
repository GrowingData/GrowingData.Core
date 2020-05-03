// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using GrowingData.Utilities;

	/// <summary>
	/// Defines the <see cref="SqlQueryProvider" />
	/// </summary>
	public abstract class SqlQueryProvider : ISqlQueryProvider {
		/// <summary>
		/// Defines the _schemaNameOverride
		/// </summary>
		protected Func<string> _schemaNameOverride;

		/// <summary>
		/// The GetTypeMap
		/// </summary>
		/// <returns>The <see cref="List{SqlTypeMap}"/></returns>
		public abstract List<SqlTypeMap> GetTypeMap();

		/// <summary>
		/// Defines the _sqlTypeMapping
		/// </summary>
		protected List<SqlTypeMap> _sqlTypeMapping;

		/// <summary>
		/// Gets the StringTypeKey
		/// </summary>
		public virtual string StringTypeKey {
			get { return "VARCHAR(512)"; }
		}

		/// <summary>
		/// Gets the StringTypeLong
		/// </summary>
		public virtual string StringTypeLong {
			get { return "TEXT"; }
		}

		/// <summary>
		/// The GetTableName
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string GetTableName(object ps) {
			var t = ps.GetType();
			var a = t.GetTypeInfo().GetCustomAttribute<SqlTableAttribute>();
			if (a != null) {
				return a.TableName;
			}
			var schema = t.Namespace.Split('.').Last();
			var table = t.Name;
			return table.ToDnsSafeLabel();
		}

		/// <summary>
		/// The GetSchemaName
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string GetSchemaName(object ps) {
			if (_schemaNameOverride != null) {
				return _schemaNameOverride();
			}
			var t = ps.GetType();
			var a = t.GetTypeInfo().GetCustomAttribute<SqlTableAttribute>();

			if (a != null) {
				return a.SchemaName;
			}

			var schema = t.Namespace.Split('.').Last();
			var table = t.Name;
			return schema.ToDnsSafeLabel();
		}

		/// <summary>
		/// The GetTableReference
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string GetTableReference(object ps) {
			return $"{GetSchemaName(ps)}.{GetTableName(ps)}";
		}

		/// <summary>
		/// The DropTableIfExistsSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string DropTableIfExistsSql(object ps) {
			return $@"DROP TABLE IF EXISTS {GetTableReference(ps)} ";
		}

		/// <summary>
		/// The TranslateTypeSql
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string TranslateTypeSql(Type t) {
			var type = _sqlTypeMapping.FirstOrDefault(x => x.DotNetType == t);
			var sqlType = type?.SqlType;
			return sqlType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlQueryProvider"/> class.
		/// </summary>
		public SqlQueryProvider() {
			_sqlTypeMapping = GetTypeMap();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlQueryProvider"/> class.
		/// </summary>
		/// <param name="overrideSchemaName">The <see cref="Func{string}"/></param>
		public SqlQueryProvider(Func<string> overrideSchemaName) {
			_schemaNameOverride = overrideSchemaName;
		}

		/// <summary>
		/// The TranslateType
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		public string TranslateType(Type t) {

			var type = _sqlTypeMapping.FirstOrDefault(x => x.DotNetType == t);
			var sqlType = type?.SqlType;
			return sqlType;
		}

		/// <summary>
		/// The SqlColumns
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="List{SqlColumn}"/></returns>
		public List<SqlColumn> SqlColumns(object ps) {
			if (ps == null) {
				return null;
			}
			var type = ps.GetType();
			var fields = type.GetFields();
			var items = new List<SqlColumn>();
			foreach (var f in fields) {
				//var obj = f.GetValue(ps);
				var isIgnore = f.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isIgnore) {
					items.Add(new SqlColumn(f.Name, f.FieldType, f));
				}

			}
			return items;
		}

		/// <summary>
		/// The GetSqlType
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string GetSqlType(Type t) {
			var typeMapping = _sqlTypeMapping.FirstOrDefault(x => x.DotNetType == t);
			var sqlType = typeMapping.SqlType;
			return sqlType;
		}

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string CreateTableSql(object ps) {
			var fields = SqlColumns(ps);
			return CreateTableSql(ps, fields, null);
		}

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="table">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string CreateTableSql(SqlTable table) {
			return CreateTableSql(table.DatasetId + "." + table.TableId, table.Columns, table.PrimaryKeys);
		}

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="fields">The <see cref="List{SqlColumn}"/></param>
		/// <param name="keys">The <see cref="List{string}"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string CreateTableSql(object ps, List<SqlColumn> fields, List<string> keys) {
			if (ps == null) {
				return null;
			}
			var name = GetTableReference(ps);
			return CreateTableSql(name, fields, keys);
		}

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="fields">The <see cref="List{SqlColumn}"/></param>
		/// <param name="keys">The <see cref="List{string}"/></param>
		/// <returns>The <see cref="string"/></returns>
		public virtual string CreateTableSql(string name, List<SqlColumn> fields, List<string> keys) {

			var columns = new List<string>();
			foreach (var sqlColumn in fields) {
				var sqlName = sqlColumn.ColumnName;
				var type = _sqlTypeMapping.FirstOrDefault(x => x.DotNetType == sqlColumn.DotNetType);
				if (type == null) {
					throw new Exception($"Unable to load mapping type for {sqlColumn.DotNetType}, using {this.GetType().Name}");
				}
				var sqlType = type.SqlType;

				//var field = sqlColumn.ReflectedField;
				//if (field != null) {
				//	var pkAttr = field.GetCustomAttribute<SqlPKAttribute>();
				//	if (pkAttr != null) {
				//		// Strings can always be NULL, so lets look for the SqlPk marker and use that
				//		// to ensure that its never NULL
				//		if (pkAttr.Length != -1 && field.FieldType == typeof(string)) {
				//			sqlType = $"{StringTypeKey} NOT NULL";
				//		}
				//		//keys.Add(sqlName);
				//	}
				//	// For when we want a text field to be of a certain length;
				//	var ltAttr = field.GetCustomAttribute<SqlLongTextAttribute>();
				//	if (ltAttr != null) {
				//		if (field.FieldType == typeof(string)) {
				//			sqlType = $"{StringTypeLong} NULL";
				//		}
				//	}
				//}
				if (type == null) {
					throw new Exception($"Unable to find a TypeSqlMap for type {sqlColumn.DotNetType.FullName}");
				}

				columns.Add($"{sqlName} {sqlType}");
			}

			var pkTemplate = "";
			if (keys != null && keys.Count > 0) {
				pkTemplate = $",\r\nCONSTRAINT PK_{name.Replace(".", "")} PRIMARY KEY ({string.Join(",", keys)})";
			}

			var template = $@"
				CREATE TABLE {name} (
					{string.Join(",\r\n\t\t\t", columns)}
					{pkTemplate}
				)
			";
			return template;
		}
	}
}
