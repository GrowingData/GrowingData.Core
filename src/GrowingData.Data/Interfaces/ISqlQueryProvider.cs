// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="ISqlQueryProvider" />
	/// </summary>
	public interface ISqlQueryProvider {
		/// <summary>
		/// Gets the StringTypeKey
		/// </summary>
		string StringTypeKey { get; }

		/// <summary>
		/// Gets the StringTypeLong
		/// </summary>
		string StringTypeLong { get; }

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		string CreateTableSql(object ps);

		/// <summary>
		/// The CreateTableSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="fields">The <see cref="List{SqlColumn}"/></param>
		/// <param name="primaryKeys">The <see cref="List{string}"/></param>
		/// <returns>The <see cref="string"/></returns>
		string CreateTableSql(object ps, List<SqlColumn> fields, List<string> primaryKeys);

		/// <summary>
		/// The DropTableIfExistsSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		string DropTableIfExistsSql(object ps);

		/// <summary>
		/// The GetSchemaName
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetSchemaName(object ps);

		/// <summary>
		/// The GetSqlType
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetSqlType(Type t);

		/// <summary>
		/// The GetTableName
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetTableName(object ps);

		/// <summary>
		/// The GetTableReference
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetTableReference(object ps);

		/// <summary>
		/// The GetTypeMap
		/// </summary>
		/// <returns>The <see cref="List{SqlTypeMap}"/></returns>
		List<SqlTypeMap> GetTypeMap();

		/// <summary>
		/// The SqlColumns
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="List{SqlColumn}"/></returns>
		List<SqlColumn> SqlColumns(object ps);

		/// <summary>
		/// The TranslateType
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		string TranslateType(Type t);

		/// <summary>
		/// The TranslateTypeSql
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="string"/></returns>
		string TranslateTypeSql(Type t);
	}
}
