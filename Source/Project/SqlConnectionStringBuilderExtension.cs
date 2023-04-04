using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

namespace Project.Data.SqlClient.Extensions
{
	public static class SqlConnectionStringBuilderExtension
	{
		#region Fields

		public const string DataDirectoryKey = "DataDirectory";
		public const string DataDirectorySubstitution = $"|{DataDirectoryKey}|";
		public const string LocalDatabasePrefix = "(LocalDb)";

		#endregion

		#region Methods

		private static string GetFullPath(string path, string basePath)
		{
			if(path == null)
				throw new ArgumentNullException(nameof(path));

			if(basePath == null)
				throw new ArgumentNullException(nameof(basePath));

#if NETCOREAPP2_1_OR_GREATER
			return Path.GetFullPath(path, basePath);
#else
			if(Path.IsPathRooted(path))
				return path;

			var fullPath = Path.Combine(basePath, path);

			// To convert forward slashes, "/", to backslashes, "\", on Windows.
			fullPath = Path.GetFullPath(fullPath);

			return fullPath;
#endif
		}

		public static bool IsLocalDatabaseConnectionString(this SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			if(sqlConnectionStringBuilder == null)
				throw new ArgumentNullException(nameof(sqlConnectionStringBuilder));

			return sqlConnectionStringBuilder.DataSource.StartsWith(LocalDatabasePrefix, StringComparison.OrdinalIgnoreCase);
		}

		public static bool Resolve(this SqlConnectionStringBuilder sqlConnectionStringBuilder, IHostEnvironment hostEnvironment)
		{
			if(sqlConnectionStringBuilder == null)
				throw new ArgumentNullException(nameof(sqlConnectionStringBuilder));

			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			if(!sqlConnectionStringBuilder.IsLocalDatabaseConnectionString())
				return false;

			var attachDbFilename = sqlConnectionStringBuilder.AttachDBFilename;

			if(string.IsNullOrWhiteSpace(attachDbFilename))
				return false;

			string fullAttachDbFilename;

			if(attachDbFilename.StartsWith(DataDirectorySubstitution, StringComparison.OrdinalIgnoreCase))
			{
				if(AppDomain.CurrentDomain.GetData(DataDirectoryKey) is not string dataDirectoryPath)
					throw new InvalidOperationException($"The connection-string contains \"{nameof(SqlConnectionStringBuilder.AttachDBFilename)}={attachDbFilename}\" but the AppDomain does not have the {DataDirectoryKey.ToStringRepresentation()}-key set. You need to set the {DataDirectoryKey.ToStringRepresentation()}-key: AppDomain.CurrentDomain.SetData({DataDirectoryKey.ToStringRepresentation()}, {@"C:\Directory".ToStringRepresentation()}).");

				if(!Directory.Exists(dataDirectoryPath))
					throw new InvalidOperationException($"The directory-path {dataDirectoryPath.ToStringRepresentation()}, set as {DataDirectoryKey.ToStringRepresentation()}-key for the AppDomain, does not exist.");

				var dataDirectoryRelativeAttachDbFilename = attachDbFilename.Substring(DataDirectorySubstitution.Length);
				fullAttachDbFilename = GetFullPath(dataDirectoryRelativeAttachDbFilename, dataDirectoryPath);
			}
			else
			{
				fullAttachDbFilename = GetFullPath(attachDbFilename, hostEnvironment.ContentRootPath);
			}

			if(string.Equals(attachDbFilename, fullAttachDbFilename, StringComparison.OrdinalIgnoreCase))
				return false;

			sqlConnectionStringBuilder.AttachDBFilename = fullAttachDbFilename;

			if(string.IsNullOrEmpty(sqlConnectionStringBuilder.InitialCatalog))
				sqlConnectionStringBuilder.InitialCatalog = fullAttachDbFilename;

			return true;
		}

		public static string ResolveConnectionString(string connectionString, IHostEnvironment hostEnvironment)
		{
			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			SqlConnectionStringBuilder sqlConnectionStringBuilder;

			try
			{
				sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not create a sql-connection-string-builder from connection-string.", exception);
			}

			if(!sqlConnectionStringBuilder.Resolve(hostEnvironment))
				return connectionString;

			return sqlConnectionStringBuilder.ConnectionString;
		}

		private static string ToStringRepresentation(this object instance)
		{
			return instance switch
			{
				null => "null",
				string value => $"\"{value}\"",
				_ => instance.ToString(),
			};
		}

		#endregion
	}
}