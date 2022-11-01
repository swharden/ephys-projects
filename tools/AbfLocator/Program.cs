/* ABF Locator
 * 
 * The purpose of this application is to help locate ABF files
 * on an indexed filesystem to facilitate correcting broken links
 * that occur when ABF files are moved.
 *
 * This console application scans a folder for all ABF files
 * and stores the list of ABF file paths in a text file that
 * can be easily read by any application.
 */
using System.Text;
using System.Data.OleDb;
using System.Diagnostics;

if (args.Length == 2)
{
    string searchFolder = args[0];
    Console.WriteLine($"Searching for ABF files in {searchFolder}...");
    Stopwatch sw = Stopwatch.StartNew();
    string[] paths = FindABFs(searchFolder);
    StringBuilder sb = new();
    sb.AppendLine($"# Updated {DateTime.Now}");
    sb.AppendLine($"# Found {paths.Length:N0} ABF files");
    sb.AppendLine($"# Search took {sw.Elapsed.TotalSeconds} seconds");
    Console.WriteLine(sb.ToString());
    sb.AppendLine(string.Join(Environment.NewLine, paths));
    string saveAs = Path.GetFullPath(args[1]);
    string saveFolder = Path.GetDirectoryName(saveAs) ?? string.Empty;
    if (!Directory.Exists(saveFolder))
        throw new DirectoryNotFoundException(saveFolder);
    File.WriteAllText(saveAs, sb.ToString());
    Console.WriteLine($"Saved: {saveAs}");
}
else
{
    Console.WriteLine("Command line arguments required.");
    Console.WriteLine("Arguments: [search folder] [save file]");
    Console.WriteLine("Example: dotnet run D:/X_Drive C:/abfs.txt");
}

static string[] FindABFs(string searchFolder)
{
    if (!Directory.Exists(searchFolder))
        throw new DirectoryNotFoundException(searchFolder);

    string query =
        $"SELECT System.ItemPathDisplay FROM SystemIndex " +
        $"WHERE scope ='file:{searchFolder}' " +
        $"AND System.ItemName LIKE '%.abf'";

    string connString = @"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows""";
    using OleDbConnection connection = new(connString);
    connection.Open();

    using OleDbCommand command = new(query, connection);
    using OleDbDataReader reader = command.ExecuteReader();

    List<string> paths = new();
    while (reader.Read())
        paths.Add(reader.GetString(0));

    connection.Close();

    return paths.ToArray();
}