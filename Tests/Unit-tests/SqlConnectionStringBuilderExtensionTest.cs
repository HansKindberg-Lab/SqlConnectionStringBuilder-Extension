using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project.Data.SqlClient.Extensions;

namespace UnitTests
{
	[TestClass]
	public class SqlConnectionStringBuilderExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task DataDirectoryKey_Test()
		{
			await Task.CompletedTask;

			Assert.AreEqual("DataDirectory", SqlConnectionStringBuilderExtension.DataDirectoryKey);
		}

		[TestMethod]
		public async Task DataDirectorySubstitution_Test()
		{
			await Task.CompletedTask;

			Assert.AreEqual("|DataDirectory|", SqlConnectionStringBuilderExtension.DataDirectorySubstitution);
		}

		[TestMethod]
		public async Task LocalDatabasePrefix_Test()
		{
			await Task.CompletedTask;

			Assert.AreEqual("(LocalDb)", SqlConnectionStringBuilderExtension.LocalDatabasePrefix);
		}

		#endregion
	}
}