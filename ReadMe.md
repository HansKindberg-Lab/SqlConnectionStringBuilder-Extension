# SqlConnectionStringBuilder-Extension

SqlConnectionStringBuilder-extension to resolve Microsoft SQL Server connection strings for local development.

## Examples

### Relative path

Resolve connection-strings like this:

	Server=(LocalDB)\\MSSQLLocalDB;AttachDbFileName=Data/Database.mdf;Integrated Security=True

to this:

	Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\Projects\\My-project\\Data\\Database.mdf;Initial Catalog=D:\\Projects\\My-project\\Data\\Database.mdf;Integrated Security=True

### |DataDirectory| substitution

Resolve connection-strings like this:

	Server=(LocalDB)\\MSSQLLocalDB;AttachDbFileName=|DataDirectory|Database.mdf;Integrated Security=True

to this:

	Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\Projects\\My-project\\Data\\Database.mdf;Initial Catalog=D:\\Projects\\My-project\\Data\\Database.mdf;Integrated Security=True