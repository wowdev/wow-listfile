using System.Text.Json;
using System.Text.RegularExpressions;

namespace ListfileTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments specified, available modes: check, merge, split, compile, compileVerified");
                Console.WriteLine("Check input CSV file against listfile: check <base listfile|parts directory> <input listfile>");
                Console.WriteLine("Merge input CSV file into listfile (parts): merge <base listfile|parts directory> <input listfile>");
                Console.WriteLine("Split single CSV listfile into parts: split <listfile> <output directory>");
                Console.WriteLine("Compile listfile parts into single CSV: compile <parts directory> <output directory>");
                Console.WriteLine("Compile listfile parts with verified names into single CSV: compileVerified <parts directory> <output directory>");
                Environment.Exit(-1);
            }

            if (args.Length > 1)
            {
                switch (args[0])
                {
                    case "check":
                        Check(args[1], args[2]);
                        break;
                    case "merge":
                        Merge(args[1], args[2]);
                        break;
                    case "split":
                        if (!File.Exists(args[1])) return;
                        var sourceListfile = new Dictionary<uint, string>();

                        foreach (var line in File.ReadLines(args[1]))
                        {
                            var splitLine = line.Split(';');
                            if (splitLine.Length != 2)
                                continue;

                            if (uint.TryParse(splitLine[0], out var fileDataID))
                                sourceListfile.Add(fileDataID, splitLine[1].Trim());
                        }
                        Split(sourceListfile, args[2]);
                        break;
                    case "compile":
                        Compile(args[1], args[2], false);
                        break;
                    case "compileVerified":
                        Compile(args[1], args[2], true);
                        break;
                    default:
                        Console.WriteLine("Unknown mode: " + args[0]);
                        Environment.Exit(-1);
                        break;
                }
            }
        }

        static string Check(string baseLocation, string input)
        {
            var inFile = input;

            if (input[0] == '#')
            {
                var issueNumber = int.Parse(input.TrimStart('#'));
                inFile = DownloadGitHubAttachment(issueNumber);
            }

            if (!File.Exists(inFile))
            {
                throw new FileNotFoundException("Input file " + inFile + " does not exist!");
            }

            var sourceListfile = new SortedDictionary<uint, string>();

            if (Directory.Exists(baseLocation))
            {
                foreach (var sourceFile in Directory.GetFiles(baseLocation, "*.csv"))
                {
                    var sourceFileLines = File.ReadLines(sourceFile);
                    foreach (var line in sourceFileLines)
                    {
                        var split = line.Split(';');
                        if (split.Length != 2)
                            continue;

                        if (uint.TryParse(split[0], out var fileDataID))
                            sourceListfile.Add(fileDataID, split[1].Trim());
                    }
                }
            }
            else if (File.Exists(baseLocation))
            {
                var sourceFileLines = File.ReadLines(baseLocation);
                foreach (var line in sourceFileLines)
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;

                    if (uint.TryParse(split[0], out var fileDataID))
                        sourceListfile.Add(fileDataID, split[1].Trim());
                }
            }

            Console.WriteLine("Checking " + inFile);

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
                        Console.WriteLine(fileDataID + " will be removed from listfile");
                        sourceListfile.Remove(fileDataID);
                    }
                    else
                    {
                        if (sourceListfile.TryGetValue(fileDataID, out string? sourceName))
                        {
                            // FileDataID is already present in listfile
                            if (sourceName != split[1].Trim())
                            {
                                // FileDataID is present, but the filename is different
                                Console.WriteLine("FileDataID " + fileDataID + " is present in listfile, but the filename is different: " + split[1].Trim() + " vs " + sourceName);
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
                }
            }

            return inFile;
        }

        static void Merge(string baseLocation, string input)
        {
            // If the input file is a GitHub issue, retrieve the attachment
            if (input[0] == '#')
            {
                var issueNumber = int.Parse(input.TrimStart('#'));
                input = DownloadGitHubAttachment(issueNumber);
            }

            Console.WriteLine("Merging..");
            var mergedListfile = new SortedDictionary<uint, string>();

            if (Directory.Exists(baseLocation))
            {
                foreach (var sourceFile in Directory.GetFiles(baseLocation, "*.csv"))
                {
                    var sourceFileLines = File.ReadLines(sourceFile);
                    foreach (var line in sourceFileLines)
                    {
                        var split = line.Split(';');
                        if (split.Length != 2)
                            continue;

                        if (uint.TryParse(split[0], out var fileDataID))
                            mergedListfile.Add(fileDataID, split[1].Trim());
                    }
                }
            }
            else if (File.Exists(baseLocation))
            {
                Console.WriteLine("Using old listfile CSV merging method, this does not check for >100MB resulting listfiles so do not use this in automation.");
                var sourceFileLines = File.ReadLines(baseLocation);
                foreach (var line in sourceFileLines)
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;

                    if (uint.TryParse(split[0], out var fileDataID))
                        mergedListfile.Add(fileDataID, split[1].Trim());
                }
            }
            else
            {
                throw new Exception("Base directory or file " + baseLocation + " does not exist, cannot continue.");
            }

            var sourceFilenames = mergedListfile.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var oldListfile = mergedListfile.ToDictionary(x => x.Key, x => x.Value);

            if (!File.Exists(input))
                throw new Exception("Input file does not exist, cannot continue.");

            // Load lookups
            var lookupFile = Path.Combine(Path.GetDirectoryName(baseLocation) ?? "", "meta", "lookup.csv");
            var lookups = new Dictionary<uint, ulong>();
            var hasher = new Jenkins96();

            if (File.Exists(lookupFile))
            {
                foreach (var line in File.ReadLines(lookupFile))
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;
                    var fileDataID = uint.Parse(split[0]);
                    var lookup = Convert.ToUInt64(split[1], 16);
                    lookups.Add(fileDataID, lookup);
                }
            }

            var anyChanges = false;

            var inFileLines = File.ReadLines(input);
            foreach (var line in inFileLines)
            {
                var split = line.Split(';');

                if (split.Length != 2)
                    continue;

                var inputName = split[1].Trim().Replace("\\", "/");
                var remove = inputName.Length == 0;

                if (uint.TryParse(split[0], out var fileDataID))
                {
                    if (lookups.TryGetValue(fileDataID, out ulong lookup))
                    {
                        if (lookup != hasher.ComputeHash(inputName))
                        {
                            Console.WriteLine("!!! Warning: Suggestion for FDID " + fileDataID + " (" + inputName + ") does not match known lookup, skipping addition.");
                            continue;
                        }
                    }

                    if (mergedListfile.ContainsKey(fileDataID))
                    {
                        if (remove)
                        {
                            Console.WriteLine("Removing " + fileDataID + " from listfile");
                            mergedListfile.Remove(fileDataID);
                        }
                        else
                        {
                            // Don't ignore case, we might want to prefer files with case over files that have no casing. Manual preference check before merging.
                            if (mergedListfile[fileDataID].Equals(inputName, StringComparison.Ordinal))
                            {
                                //Console.WriteLine("!!! Warning: input suggestion for FileDataID " + fileDataID + " (" + inputName + ") is the same as existing suggestion (" + mergedListfile[fileDataID] + ", skipping!");
                            }
                            else
                            {
                                mergedListfile[fileDataID] = inputName;
                                sourceFilenames.Add(inputName);
                                anyChanges = true;
                            }
                        }
                    }
                    else
                    {
                        if (sourceFilenames.Contains(inputName))
                        {
                            if (inputName.EndsWith(".blp"))
                            {
                                var filenameMatch = mergedListfile.FirstOrDefault(x => x.Value.Equals(inputName, StringComparison.OrdinalIgnoreCase));
                                if (lookups.TryGetValue(filenameMatch.Key, out ulong existingFileLookup))
                                {
                                    if (existingFileLookup == hasher.ComputeHash(inputName))
                                    {
                                        Console.WriteLine("!!! Warning: Incoming suggestion would append a FDID to existing name for " + filenameMatch.Key + " (" + filenameMatch.Value + ") but lookup matches for this file. Skipping change.");
                                        continue;
                                    }
                                }

                                Console.WriteLine("Appending FileDataID to duplicate BLP " + split[0] + " " + inputName);
                                mergedListfile[fileDataID] = Path.GetDirectoryName(inputName) + "/" + Path.GetFileNameWithoutExtension(inputName) + "_" + fileDataID + ".blp";
                                anyChanges = true;
                            }
                            else
                            {
                                Console.WriteLine("!!! DUPLICATE " + line);
                                continue;
                            }
                        }
                        else
                        {
                            mergedListfile.Add(fileDataID, inputName);
                            sourceFilenames.Add(inputName);
                            anyChanges = true;
                        }
                    }
                }
            }

            var existingFilenames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mergedListfileCopy = mergedListfile.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            foreach (var file in mergedListfileCopy)
            {
                if (existingFilenames.Contains(file.Value))
                {
                    if (file.Value.EndsWith(".blp"))
                    {
                        if (lookups.TryGetValue(file.Key, out ulong existingFileLookup))
                        {
                            if (existingFileLookup == hasher.ComputeHash(file.Value))
                            {
                                Console.WriteLine("!!! Warning: Was about to append a FDID to existing name for " + file.Key + " (" + file.Value + ") but lookup matches for this file.");
                                var filenameMatch = mergedListfile.FirstOrDefault(x => x.Value.Equals(file.Value, StringComparison.OrdinalIgnoreCase));
                                mergedListfile[filenameMatch.Key] = oldListfile[filenameMatch.Key];
                                continue;
                            }
                        }

                        Console.WriteLine("Appending FileDataID to duplicate BLP " + file.Key + " " + file.Value);
                        mergedListfile[file.Key] = Path.GetDirectoryName(mergedListfile[file.Key]) + "/" + Path.GetFileNameWithoutExtension(mergedListfile[file.Key]) + "_" + file.Key + ".blp";
                        anyChanges = true;
                    }
                    else
                    {
                        Console.WriteLine("Removing duplicate " + file.Key + " " + file.Value);
                        mergedListfile.Remove(file.Key);
                        anyChanges = true;
                    }
                }
                else
                {
                    existingFilenames.Add(file.Value);
                }
            }

            if (!anyChanges)
            {
                Console.WriteLine("No changes to listfile, exiting.");
                return;
            }

            if (Directory.Exists(baseLocation))
            {
                Console.WriteLine("Saving results to parts");
                Split(mergedListfile.ToDictionary(x => x.Key, x => x.Value), baseLocation);
            }
            else
            {
                Console.WriteLine("Saving results to CSV");
                var noCapitalOutput = Path.Combine(baseLocation, "community-listfile.csv");
                var withCapitalOutput = Path.Combine(baseLocation, "community-listfile-withcapitals.csv");

                File.WriteAllText(withCapitalOutput, string.Join("\r\n", mergedListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");
                Console.WriteLine("Wrote " + mergedListfile.Count + " entries to " + withCapitalOutput);

                var numWithCase = mergedListfile.Where(x => x.Value.Equals(x.Value, StringComparison.OrdinalIgnoreCase)).Count();
                var percentage = (float)numWithCase / (float)mergedListfile.Count * 100.0f;
                Console.WriteLine(numWithCase + " / " + mergedListfile.Count + " (" + percentage + "%) entries are lowercase");

                File.WriteAllText(noCapitalOutput, string.Join("\r\n", mergedListfile.Select(x => x.Key + ";" + x.Value.ToLower().Replace("\\", "/"))) + "\r\n");
            }
        }

        static string DownloadGitHubAttachment(int issueNumber)
        {
            var inFile = "";
            var apiURL = "https://api.github.com/repos/wowdev/wow-listfile/issues/" + issueNumber;

            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "ListfileTool");
                Console.WriteLine("Getting issue JSON " + apiURL);
                var response = client.GetStringAsync(apiURL).Result;
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;
                var attachmentRegex = new Regex(@"!?\[([^\]]*)\]\(([^\)]+)\)", RegexOptions.Multiline);

                var issueBody = root.GetProperty("body").GetString() ?? throw new Exception("Unable to load body from JSON!");
                var matches = attachmentRegex.Matches(issueBody);
                if (matches.Count == 0)
                    throw new Exception("No attachment found in issue body!");

                var attachmentURL = matches[0].Groups[2].Value;
                Console.WriteLine("Retrieving " + attachmentURL);
                var attachmentContents = client.GetStringAsync(attachmentURL).Result;
                File.WriteAllText(inFile = "wow-listfile-" + issueNumber + ".txt", attachmentContents);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to retrieve attachment from GitHub issue: " + e.Message);
                Environment.Exit(-1);
            }

            return inFile;
        }
        static void Split(Dictionary<uint, string> sourceListfile, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var placeholderFiles = new Dictionary<uint, string>();
            foreach (var file in sourceListfile)
            {
                var filenameLower = file.Value.ToLower();
                if (
                    filenameLower.StartsWith("models") ||
                    filenameLower.StartsWith("unkmaps") ||
                    filenameLower.Contains("autogen-names") ||
                    filenameLower.Contains(file.Key.ToString()) ||
                    filenameLower.Contains("unk_exp") ||
                    filenameLower.Contains("tileset/unused")
                    )
                {
                    placeholderFiles.TryAdd(file.Key, file.Value);
                }

                if (file.Value.EndsWith(".ogg") && file.Value.Substring(file.Value.Length - 10, 6).All(char.IsDigit))
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
                if (filename.Length == 0 || !char.IsDigit(filename[0]) || file.Value.StartsWith("textures", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var phFile in placeholderM2s)
                {
                    if (filename.StartsWith(phFile))
                        placeholderFiles.Add(file.Key, file.Value);
                }
            }

            sourceListfile = sourceListfile.Where(x => !placeholderFiles.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            File.WriteAllText(Path.Combine(outputDir, "placeholder.csv"), string.Join("\r\n", placeholderFiles.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");

            var splitFilters = new string[] { "creature", "item", "spells", "sound", "interface", "world/maps", "world/wmo", "world/minimaps", "world" };

            foreach (var filter in splitFilters)
            {
                var filteredListfile = sourceListfile.Where(x => x.Value.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                var tries = 0;
                while (tries < 10)
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(outputDir, filter.Replace("/", "-") + ".csv"), string.Join("\r\n", filteredListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");
                        break;
                    }
                    catch (IOException e)
                    {
                        tries++;
                        Console.WriteLine("Error writing file, retrying " + tries + "/10");
                    }
                }

                sourceListfile = sourceListfile.Where(x => !filteredListfile.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            }

            File.WriteAllText(Path.Combine(outputDir, "misc.csv"), string.Join("\r\n", sourceListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");
        }

        static void Compile(string inputDir, string outputDir, bool verifiedNames = false)
        {
            var mergedListfile = new Dictionary<uint, string>();

            var outputNameCapitals = "community-listfile-withcapitals.csv";
            var outputNameNoCapitals = "community-listfile.csv";

            if (verifiedNames)
            {
                outputNameCapitals = "verified-listfile-withcapitals.csv";
                outputNameNoCapitals = "verified-listfile.csv";
            }

            foreach (var file in Directory.GetFiles(inputDir, "*.csv"))
            {
                foreach (var line in File.ReadLines(file))
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;

                    if (uint.TryParse(split[0], out var fileDataID))
                    {
                        if (mergedListfile.ContainsKey(fileDataID))
                        {
                            Console.WriteLine("!!! Warning: duplicate FileDataID " + fileDataID + " (" + split[1] + ") in " + file);
                            continue;
                        }

                        mergedListfile.Add(fileDataID, split[1].Trim());
                    }
                }
            }

            if (verifiedNames)
            {
                // meta/lookup.csv hould be one path above inputDir
                var lookupFile = Path.Combine(Directory.GetParent(inputDir).FullName, "meta", "lookup.csv");

                if (!File.Exists(lookupFile))
                {
                    Console.WriteLine("!!! Warning: lookup.csv not found, skipping verified names compilation");
                    return;
                }

                var hasher = new Jenkins96();

                var lookups = new Dictionary<uint, ulong>();

                foreach (var line in File.ReadLines(lookupFile))
                {
                    var split = line.Split(';');
                    if (split.Length != 2)
                        continue;

                    var fileDataID = uint.Parse(split[0]);
                    var lookup = Convert.ToUInt64(split[1], 16);

                    lookups.Add(fileDataID, lookup);
                }

                foreach (var mergedEntry in mergedListfile)
                {
                    if (lookups.TryGetValue(mergedEntry.Key, out var lookup))
                    {
                        if (lookup != hasher.ComputeHash(mergedEntry.Value))
                        {
                            mergedListfile.Remove(mergedEntry.Key);
                        }
                    }
                    else
                    {
                        mergedListfile.Remove(mergedEntry.Key);
                    }
                }
            }

            mergedListfile = mergedListfile.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            File.WriteAllText(Path.Combine(outputDir, outputNameCapitals), string.Join("\r\n", mergedListfile.Select(x => x.Key + ";" + x.Value.Replace("\\", "/"))) + "\r\n");
            File.WriteAllText(Path.Combine(outputDir, outputNameNoCapitals), string.Join("\r\n", mergedListfile.Select(x => x.Key + ";" + x.Value.ToLower().Replace("\\", "/"))) + "\r\n");
        }
    }
}