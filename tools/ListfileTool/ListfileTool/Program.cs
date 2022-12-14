using System.Text.Json;
using System.Text.RegularExpressions;

namespace ListfileTool
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments specified, available modes: check, merge");
                Environment.Exit(-1);
            }

            if (args.Length > 1)
            {
                switch (args[0])
                {
                    case "check":
                        await Check(args);
                        break;
                    case "merge":
                        Merge(args);
                        break;
                    case "checkandmerge":
                        var inFile = await Check(args);
                        Merge(new string[] { "merge", args[1], inFile });
                        break;
                    default:
                        Console.WriteLine("Unknown mode: " + args[0]);
                        break;
                }
            }
        }

        static async Task<string> Check(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid number of arguments for check mode, expected 3: check sourceListfile (#githubIssueID or inListfile)");
                Environment.Exit(-1);
            }

            var inFile = "";

            // Retrieve attachment from GitHub issue
            if (args[2][0] == '#')
            {
                var issueNumber = args[2].TrimStart('#');
                var apiURL = "https://api.github.com/repos/wowdev/wow-listfile/issues/" + issueNumber;

                try
                {
                    using HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "ListfileTool");
                    Console.WriteLine("Getting issue JSON " + apiURL);
                    var response = await client.GetStringAsync(apiURL);
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;
                    var attachmentRegex = new Regex(@"!?\[([^\]]*)\]\(([^\)]+)\)", RegexOptions.Multiline);

                    var issueBody = root.GetProperty("body").GetString();
                    if (issueBody == null)
                        throw new Exception("Unable to load body from JSON!");

                    var matches = attachmentRegex.Matches(issueBody);
                    if (matches.Count == 0)
                        throw new Exception("No attachment found in issue body!");

                    var attachmentURL = matches[0].Groups[2].Value;
                    Console.WriteLine("Retrieving " + attachmentURL);
                    var attachmentContents = await client.GetStringAsync(attachmentURL);
                    File.WriteAllText(inFile = "wow-listfile-" + issueNumber + ".txt", attachmentContents);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to retrieve attachment from GitHub issue: " + e.Message);
                    Environment.Exit(-1);
                }
            }
            else
            {
                if (File.Exists(args[2]))
                {
                    inFile = args[2];
                }
                else
                {
                    Console.WriteLine("File to check not found: " + args[2]);
                    Environment.Exit(-1);
                }
            }

            Console.WriteLine("Checking " + inFile);

            var sourceListfile = new SortedDictionary<uint, string>();

            var sourceFileLines = File.ReadLines(args[1]);
            foreach (var line in sourceFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                if (uint.TryParse(split[0], out var fileDataID))
                    sourceListfile.Add(fileDataID, split[1].Trim());
            }

            var inFileLines = File.ReadLines(inFile);
            foreach (var line in inFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    if (sourceListfile.ContainsKey(fileDataID))
                    {
                        // FileDataID is already present in listfile
                        if (sourceListfile[fileDataID] != split[1].Trim())
                        {
                            // FileDataID is present, but the filename is different
                            Console.WriteLine("FileDataID " + fileDataID + " is present in listfile, but the filename is different: " + split[1].Trim() + " vs " + sourceListfile[fileDataID]);
                        }
                        else
                        {
                            // FileDataID is present and the filename is the same
                            Console.WriteLine("FileDataID " + fileDataID + " is present in listfile and the filename is the same: " + split[1].Trim());
                        }
                    }
                    else
                    {
                        // FileDataID is new
                        Console.WriteLine("FileDataID " + fileDataID + " is not present in listfile: " + split[1].Trim());
                    }
                }

                // Scan filenames in listfile to avoid duplicates
                var filename = split[1].Trim();
                if (sourceListfile.ContainsValue(filename))
                {
                    Console.WriteLine("!!! Error !!! Filename " + filename + " is already present in listfile!");
                    Environment.Exit(-1);
                }
            }
            return inFile;
        }

        static void Merge(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid number of arguments, expected at least 3: mode sourceListfile inListfile (outputListfile, defaults to sourceFile)");
                Environment.Exit(-1);
            }

            var sourceFile = args[1];
            var inFile = args[2];
            var outFile = args.Length == 4 ? args[3] : sourceFile;

            var mergedListfile = new SortedDictionary<uint, string>();

            var sourceFileLines = File.ReadLines(sourceFile);
            foreach (var line in sourceFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                if (uint.TryParse(split[0], out var fileDataID))
                    mergedListfile.Add(fileDataID, split[1].Trim());
            }

            var inFileLines = File.ReadLines(inFile);
            foreach (var line in inFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    if (mergedListfile.ContainsKey(fileDataID))
                    {
                        mergedListfile[fileDataID] = split[1].Trim();
                    }
                    else
                    {
                        mergedListfile.Add(fileDataID, split[1].Trim());
                    }
                }
            }

            File.WriteAllText(outFile, string.Join("\n", mergedListfile.Select(x => x.Key + ";" + x.Value.ToLower())) + "\n");
        }
    }
}