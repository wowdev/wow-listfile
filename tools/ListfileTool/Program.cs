using System.Collections.Immutable;
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
                Console.WriteLine("No arguments specified, available modes: check, merge, checkandmerge, split, compile");
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
                    case "split":
                        Split(args[1], args[2]);
                        break;
                    case "compile":
                        var force100MBLimit = false;
                        if (args.Length == 4)
                            force100MBLimit = bool.Parse(args[3]);

                        Compile(args[1], args[2], force100MBLimit);
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
            else if (Directory.Exists(args[2]))
            {
                var files = Directory.GetFiles(args[2], "*.csv");

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

            var sourceFileLines = File.ReadLines(Path.Combine(args[1], "community-listfile-withcapitals.csv"));
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

                var remove = split[1].Trim().Length == 0;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    if (remove)
                    {
                        //Console.WriteLine("Removing " + fileDataID + " from listfile");
                        sourceListfile.Remove(fileDataID);
                    }
                    else
                    {
                        if (sourceListfile.ContainsKey(fileDataID))
                        {
                            // FileDataID is already present in listfile
                            if (sourceListfile[fileDataID] != split[1].Trim())
                            {
                                // FileDataID is present, but the filename is different
                                //Console.WriteLine("FileDataID " + fileDataID + " is present in listfile, but the filename is different: " + split[1].Trim() + " vs " + sourceListfile[fileDataID]);
                            }
                            else
                            {
                                // FileDataID is present and the filename is the same
                                //Console.WriteLine("FileDataID " + fileDataID + " is present in listfile and the filename is the same: " + split[1].Trim());
                            }
                        }
                        else
                        {
                            // FileDataID is new
                            // Console.WriteLine("FileDataID " + fileDataID + " is not present in listfile: " + split[1].Trim());
                        }
                    }
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

            var sourceDir = args[1];
            var inFile = args[2];
            var outFile = args.Length == 4 ? args[3] : Path.Combine(sourceDir, "community-listfile-withcapitals.csv");
            var outFileLC = args.Length == 4 ? Path.Combine(Path.GetDirectoryName(args[3]), "community-listfile.csv") : Path.Combine(sourceDir, "community-listfile.csv");
            var mergedListfile = new Dictionary<uint, string>();
            var sourceFileLines = File.ReadLines(Path.Combine(sourceDir, "community-listfile-withcapitals.csv"));

            foreach (var line in sourceFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    mergedListfile.Add(fileDataID, split[1].Trim());
                }
            }

            var sourceFilenames = mergedListfile.Values.ToHashSet();

            var inFileLines = File.ReadLines(inFile);
            foreach (var line in inFileLines)
            {
                var split = line.Split(';');
                if (split.Length != 2)
                    continue;

                var remove = split[1].Trim().Length == 0;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    if (mergedListfile.ContainsKey(fileDataID))
                    {
                        if (remove)
                        {
                            Console.WriteLine("Removing " + fileDataID + " from listfile");
                            mergedListfile.Remove(fileDataID);
                        }
                        else
                        {
                            mergedListfile[fileDataID] = split[1].Trim();
                        }
                    }
                    else
                    {
                        if (sourceFilenames.Contains(split[1].Trim()))
                        {
                            if (split[1].Trim().EndsWith(".blp"))
                            {
                                Console.WriteLine("Appending FileDataID to duplicate BLP " + split[0] + " " + split[1].Trim());
                                mergedListfile[fileDataID] = Path.GetDirectoryName(split[1].Trim()) + "/" + Path.GetFileNameWithoutExtension(split[1].Trim()) + "_" + fileDataID + ".blp";
                            }
                            else
                            {
                                Console.WriteLine("!!! DUPLICATE " + line);
                                continue;
                            }
                        }
                        else
                        {
                            mergedListfile.Add(fileDataID, split[1].Trim());
                        }
                    }
                }
            }

            var mergedListfileCopy = mergedListfile.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var existingFilenames = new HashSet<string>();

            foreach (var file in mergedListfileCopy)
            {
                if (existingFilenames.Contains(file.Value.Trim().Replace("\\", "/").ToLower()))
                {
                    if (file.Value.EndsWith(".blp"))
                    {
                        Console.WriteLine("Appending FileDataID to duplicate BLP " + file.Key + " " + file.Value);
                        mergedListfile[file.Key] = Path.GetDirectoryName(mergedListfile[file.Key]) + "/" + Path.GetFileNameWithoutExtension(mergedListfile[file.Key]) + "_" + file.Key + ".blp";
                    }
                    else
                    {
                        Console.WriteLine("Removing duplicate " + file.Key + " " + file.Value);
                        mergedListfile.Remove(file.Key);
                    }
                }
                else
                {
                    existingFilenames.Add(file.Value.Trim().Replace("\\", "/").ToLower());
                }
            }

            mergedListfile = mergedListfile.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var numWithCase = mergedListfile.Where(x => x.Value == x.Value.ToLower()).Count();

            File.WriteAllText(outFile, string.Join("\n", mergedListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");
            Console.WriteLine("Wrote " + mergedListfile.Count + " entries to " + outFile);
            var percentage = (float)numWithCase / (float)mergedListfile.Count * 100.0f;
            Console.WriteLine(numWithCase + " / " + mergedListfile.Count + " (" + percentage + "%) entries are lowercase");

            File.WriteAllText(outFileLC, string.Join("\n", mergedListfile.Select(x => x.Key + ";" + x.Value.ToLower().Replace("\\", "/"))) + "\r\n");
        }

        static void Split(string input, string outputDir)
        {
            if (!File.Exists(input)) return;

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var sourceListfile = new Dictionary<uint, string>();

            foreach (var line in File.ReadLines(input))
            {
                var splitLine = line.Split(';');
                if (splitLine.Length != 2)
                    continue;

                if (uint.TryParse(splitLine[0], out var fileDataID))
                    sourceListfile.Add(fileDataID, splitLine[1].Trim());
            }


            var placeholderFiles = new Dictionary<uint, string>();
            foreach (var file in sourceListfile)
            {
                var filenameLower = file.Value.ToLower();
                if (
                    filenameLower.Contains(file.Key.ToString()) ||
                    filenameLower.Contains("unk_exp09") ||
                    filenameLower.Contains("unknown_901_33978_000") ||
                    filenameLower.Contains("nazjatar_unused") ||
                    filenameLower.Contains("mechagon_unused") ||
                    filenameLower.Contains("tileset/unused")
                    )
                {
                    placeholderFiles.TryAdd(file.Key, file.Value);
                }
            }

            sourceListfile = sourceListfile.Where(x => !placeholderFiles.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            // Ensure that referenced files from unknown M2s are also added
            var placeholderM2s = placeholderFiles.Where(x => x.Value.ToLower().EndsWith(".m2")).Select(x => x.Key.ToString()).ToList();
            foreach (var file in sourceListfile)
            {
                var filename = Path.GetFileNameWithoutExtension(file.Value);
                if (filename.Length == 0 || !char.IsDigit(filename[0]))
                    continue;

                foreach (var phFile in placeholderM2s)
                {
                    if (filename.StartsWith(phFile))
                        placeholderFiles.Add(file.Key, file.Value);
                }
            }

            sourceListfile = sourceListfile.Where(x => !placeholderFiles.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            File.WriteAllText(Path.Combine(outputDir, "placeholder.csv"), string.Join("\n", placeholderFiles.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\n");

            var splitFilters = new string[] { "creature", "item", "spells", "sound", "interface", "world/maps", "world/wmo", "world/minimaps", "world" };

            foreach (var filter in splitFilters)
            {
                var filteredListfile = sourceListfile.Where(x => x.Value.ToLower().StartsWith(filter)).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                File.WriteAllText(Path.Combine(outputDir, filter.Replace("/", "-") + ".csv"), string.Join("\n", filteredListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\n");
                sourceListfile = sourceListfile.Where(x => !filteredListfile.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            }

            File.WriteAllText(Path.Combine(outputDir, "misc.csv"), string.Join("\n", sourceListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\n");
        }

        static void Compile(string inputDir, string outputDir, bool force100MBLimit = false)
        {
            var mergedListfile = new Dictionary<uint, string>();
            
            foreach (var file in Directory.GetFiles(inputDir, "*.csv"))
            {
                foreach (var line in File.ReadLines(file))
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;

                    // If we're forcing a 100MB limit, skip all .meta, .unk and .dat files, that gives us some temporary breathing room.
                    if (force100MBLimit)
                        if (split[1].EndsWith(".meta") || split[1].EndsWith(".unk") || split[1].EndsWith(".dat"))
                            continue;

                    if (uint.TryParse(split[0], out var fileDataID))
                    {
                        if (mergedListfile.ContainsKey(fileDataID))
                        {
                            Console.WriteLine("!!! Warning: duplicate FileDataID " + fileDataID + " in " + file);
                            continue;
                        }

                        mergedListfile.Add(fileDataID, split[1].Trim());
                    }
                }
            }

            mergedListfile = mergedListfile.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var withCapitalOutput = Path.Combine(outputDir, "community-listfile-withcapitals.csv");

            if (File.Exists(withCapitalOutput))
                File.Move(withCapitalOutput, Path.Combine(outputDir, "community-listfile-withcapitals-old.csv"));

            File.WriteAllText(withCapitalOutput, string.Join("\n", mergedListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");

            var sizeWithCase = new FileInfo(withCapitalOutput).Length;
            if(sizeWithCase > 100 * 1024 * 1024)
            {
                Console.WriteLine("!!! Warning: community-listfile-withcapitals.csv is " + sizeWithCase + " bytes, which is over the 100MB limit, keeping old listfile!");
                File.Delete(withCapitalOutput);
                File.Move(Path.Combine(outputDir, "community-listfile-withcapitals-old.csv"), withCapitalOutput);
            }
            else
            {
                File.Delete(Path.Combine(outputDir, "community-listfile-withcapitals-old.csv"));
            }

            File.WriteAllText(Path.Combine(outputDir, "community-listfile.csv"), string.Join("\n", mergedListfile.Select(x => x.Key + ";" + x.Value.ToLower().Replace("\\", "/"))) + "\r\n");
        }
    }
}