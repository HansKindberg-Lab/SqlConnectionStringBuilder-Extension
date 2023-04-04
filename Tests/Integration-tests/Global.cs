using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace IntegrationTests
{
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Fields

		public static readonly string ProjectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
		public static readonly string DataDirectoryPath = Path.Combine(ProjectDirectoryPath, "Data");

		#endregion
	}
}