using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Project.Data.SqlClient.Extensions;

namespace IntegrationTests
{
	[TestClass]
	public class SqlConnectionStringBuilderExtensionTest
	{
		#region Fields

		private const string _connectionStringFormat = @"Server=(LocalDB)\MSSQLLocalDB;AttachDbFileName={0};Integrated Security=True";
		private static object _dataDirectory;
		private static IHostEnvironment _hostEnvironment;
		private const string _resolvedConnectionStringFormat = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Initial Catalog={0};Integrated Security=True";

		#endregion

		#region Properties

		protected internal virtual string ConnectionStringFormat => _connectionStringFormat;

		protected internal virtual IHostEnvironment HostEnvironment
		{
			get
			{
				if(_hostEnvironment == null)
				{
					var hostEnvironmentMock = new Mock<IHostEnvironment>();

					hostEnvironmentMock.Setup(hostEnvironment => hostEnvironment.ContentRootPath).Returns(Global.ProjectDirectoryPath);

					_hostEnvironment = hostEnvironmentMock.Object;
				}

				return _hostEnvironment;
			}
		}

		protected internal virtual string ResolvedConnectionStringFormat => _resolvedConnectionStringFormat;

		#endregion

		#region Methods

		[ClassCleanup]
		public static async Task CleanupAsync()
		{
			await Task.CompletedTask;

			AppDomain.CurrentDomain.SetData(SqlConnectionStringBuilderExtension.DataDirectoryKey, _dataDirectory);
		}

		protected internal virtual async Task<string> CreateConnectionStringAsync(string attachDbFileName)
		{
			return await Task.FromResult(string.Format(null, this.ConnectionStringFormat, attachDbFileName));
		}

		protected internal virtual async Task<string> CreateResolvedConnectionStringAsync(string attachDbFileName)
		{
			return await Task.FromResult(string.Format(null, this.ResolvedConnectionStringFormat, attachDbFileName));
		}

		[ClassInitialize]
		public static async Task InitializeAsync(TestContext _)
		{
			await Task.CompletedTask;

			_dataDirectory = AppDomain.CurrentDomain.GetData(SqlConnectionStringBuilderExtension.DataDirectoryKey);
		}

		[TestMethod]
		public async Task ResolveConnectionString_DataDirectory_Test()
		{
			AppDomain.CurrentDomain.SetData(SqlConnectionStringBuilderExtension.DataDirectoryKey, Global.DataDirectoryPath);

			var connectionString = await this.CreateConnectionStringAsync($"{SqlConnectionStringBuilderExtension.DataDirectorySubstitution}Database.mdf");

			connectionString = SqlConnectionStringBuilderExtension.ResolveConnectionString(connectionString, this.HostEnvironment);

			var expectedConnectionString = await this.CreateResolvedConnectionStringAsync(Path.Combine(Global.DataDirectoryPath, "Database.mdf"));

			Assert.AreEqual(expectedConnectionString, connectionString);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task ResolveConnectionString_NonexistentDataDirectory_Test()
		{
			var nonexistentDataDirectory = Path.Combine(Global.ProjectDirectoryPath, Guid.NewGuid().ToString());

			Assert.IsFalse(Directory.Exists(nonexistentDataDirectory));

			AppDomain.CurrentDomain.SetData(SqlConnectionStringBuilderExtension.DataDirectoryKey, nonexistentDataDirectory);

			var connectionString = await this.CreateConnectionStringAsync($"{SqlConnectionStringBuilderExtension.DataDirectorySubstitution}Database.mdf");

			try
			{
				SqlConnectionStringBuilderExtension.ResolveConnectionString(connectionString, this.HostEnvironment);
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(string.Equals(invalidOperationException.Message, $"The directory-path \"{nonexistentDataDirectory}\", set as \"DataDirectory\"-key for the AppDomain, does not exist.", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		public async Task ResolveConnectionString_NonexistentRealitvePath_Test()
		{
			var nonexistentDirectoryName = Guid.NewGuid().ToString();
			var nonexistentDirectoryPath = Path.Combine(Global.ProjectDirectoryPath, nonexistentDirectoryName);

			Assert.IsFalse(Directory.Exists(nonexistentDirectoryPath));

			var connectionString = await this.CreateConnectionStringAsync($"{nonexistentDirectoryName}/Database.mdf");

			connectionString = SqlConnectionStringBuilderExtension.ResolveConnectionString(connectionString, this.HostEnvironment);

			var expectedConnectionString = await this.CreateResolvedConnectionStringAsync(Path.Combine(Global.ProjectDirectoryPath, nonexistentDirectoryName, "Database.mdf"));

			Assert.AreEqual(expectedConnectionString, connectionString);
		}

		[TestMethod]
		public async Task ResolveConnectionString_RelativePath_Test()
		{
			var connectionString = await this.CreateConnectionStringAsync("Data/Database.mdf");

			connectionString = SqlConnectionStringBuilderExtension.ResolveConnectionString(connectionString, this.HostEnvironment);

			var expectedConnectionString = await this.CreateResolvedConnectionStringAsync(Path.Combine(Global.DataDirectoryPath, "Database.mdf"));

			Assert.AreEqual(expectedConnectionString, connectionString);
		}

		[TestMethod]
		public async Task ResolveConnectionString_RelativePathWithLeadingSlash_Test()
		{
			var connectionString = await this.CreateConnectionStringAsync("/Data/Database.mdf");

			connectionString = SqlConnectionStringBuilderExtension.ResolveConnectionString(connectionString, this.HostEnvironment);

#if NETCOREAPP2_1_OR_GREATER
			var expectedConnectionString = await this.CreateResolvedConnectionStringAsync(@"C:\Data\Database.mdf");

			Assert.AreEqual(expectedConnectionString, connectionString);
#else
			// .NET Framework 4.6.2 does not work properly.
			var expectedConnectionString = @"Server=(LocalDB)\MSSQLLocalDB;AttachDbFileName=/Data/Database.mdf;Integrated Security=True";

			Assert.AreEqual(expectedConnectionString, connectionString);
#endif
		}

		[TestCleanup]
		public async Task TestCleanupAsync()
		{
			await CleanupAsync();
		}

		[TestInitialize]
		public async Task TestInitializeAsync()
		{
			await CleanupAsync();
		}

		#endregion
	}
}