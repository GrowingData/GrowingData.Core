// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data{
	using System;
	using System.Collections.Generic;
	using GrowingData.Data;

	/// <summary>
	/// Defines the <see cref="IDatawarehouseService" />
	/// </summary>
	public interface IDatawarehouseService {
		/// <summary>
		/// Create the dataset given by "datasetId" if it does not already exist.
		/// </summary>
		/// <param name="datasetId">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool CreateDataset(string datasetId);

		/// <summary>
		/// The DropTableIfExists
		/// </summary>
		/// <param name="datasetId">The <see cref="string"/></param>
		/// <param name="tableId">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool DropTableIfExists(string datasetId, string tableId);

		/// <summary>
		/// Check to see if the speciifed table exists int he specified dataset
		/// </summary>
		/// <param name="datasetId"></param>
		/// <param name="tableId"></param>
		/// <returns></returns>
		bool TableExists(string datasetId, string tableId);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		bool CreateTable(SqlTable model);

		/// <summary>
		/// Bulk Load a table from the StorageService
		/// </summary>
		/// <param name="bucket">Bucket where the file exists</param>
		/// <param name="storagePath">Path to the file / files</param>
		/// <param name="model">The schema of the incoming files</param>
		/// <param name="createTable">Create & Drop a new table, or insert into an existing one</param>
		/// <param name="csvOptions">Information about the CSF file to insert</param>
		/// <returns></returns>
		DataWarehouseLoadResult ImportFromStorage(StorageBucket bucket, string storagePath, SqlTable model, CsvFileOptions csvOptions, string strategy = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="rootFolder"></param>
		/// <param name="datasetId"></param>
		/// <param name="tableId"></param>
		/// <param name="csvSettings"></param>
		/// <returns></returns>
		List<string> ExportToStorage(StorageBucket bucket, string rootFolder, string datasetId, string tableId, CsvFileOptions csvSettings);


		/// <summary>
		/// For the given table, return the fields / columns and their types so that we can create
		/// tables in another system using the same types.
		/// </summary>
		/// <param name="datasetId"></param>
		/// <param name="tableId"></param>
		/// <returns></returns>
		SqlTable GetTableSchema(string datasetId, string tableId);

		/// <summary>
		/// The SelectInto
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="destinationDatasetId">The <see cref="string"/></param>
		/// <param name="destinationTableId">The <see cref="string"/></param>
		/// <returns>The <see cref="DataWarehouseLoadResult"/></returns>
		DataWarehouseLoadResult SelectInto(string sql, string destinationDatasetId, string destinationTableId, Dictionary<string, object> parameters);

		/// <summary>
		/// Execute the query and return an enumerable cursor to stream the result set.
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <returns>The <see cref="IEnumerable{object}"/></returns>
		IDataWarehouseCursor Select(string sql, Dictionary<string, object> parameters);

		/// <summary>
		/// Perform a Dry Run of the query to check to see if it is well formed and what its output
		/// schema looks like, without actuall returning rows
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		DataWarehouseDryRunResult TestQuery(string sql, Dictionary<string, object> parameters);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="datasetId"></param>
		/// <returns></returns>
		bool DeleteDataset(string datasetId);

		/// <summary>
		/// List all the Datasets within the project
		/// </summary>
		/// <returns></returns>
		List<string> ListDatasets();

		/// <summary>
		/// List all the tables within a dataset
		/// </summary>
		/// <param name="datasetId"></param>
		/// <returns></returns>
		List<string> ListTables(string datasetId);


	}
}
