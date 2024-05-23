namespace ManifestMetaTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments specified, available modes: dump, merge, convert");
                Console.WriteLine("Dump CSV from root manifest: dump <decoded root manifest> <target CSV>");
                Console.WriteLine("Merge input CSV file into meta CSV: merge <input CSV> <target CSV>");
                Console.WriteLine("Convert old format manifests and merge into target CSV <old CSV manifest> <target CSV>");
                Environment.Exit(-1);
            }

            switch (args[0])
            {
                case "dump":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: dump <decoded root manifest> <target CSV>");
                        Environment.Exit(-1);
                    }
                    Dump(args[1], args[2]);
                    break;
                case "merge":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: merge <input CSV> <target CSV>");
                        Environment.Exit(-1);
                    }
                    Merge(args[1], args[2]);
                    break;
                case "convert":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: convert <old CSV manifest/folder> <target CSV>");
                        Environment.Exit(-1);
                    }
                    Convert(args[1], args[2]);
                    break;
                default:
                    Console.WriteLine("Unknown mode: " + args[0]);
                    break;
            }
        }

        private static void Convert(string oldManifest, string target)
        {
            var fdidDict = new Dictionary<int, string>();

            if (Directory.Exists(oldManifest))
            {
                foreach (var file in Directory.EnumerateFiles(oldManifest))
                {
                    ParseOldManifest(file, fdidDict);
                }
            }
            else if (File.Exists(oldManifest))
            {
                ParseOldManifest(oldManifest, fdidDict);
            }
            else
            {
                Console.WriteLine("Old manifest file not found: " + oldManifest);
            }

            Console.WriteLine("Writing " + fdidDict.Count + " entries to " + target);
            using (var writer = new StreamWriter(target))
            {
                foreach (var entry in fdidDict)
                {
                    writer.WriteLine(entry.Key + ";" + entry.Value);
                }
            }
        }

        private static void ParseOldManifest(string filename, Dictionary<int, string> outputDictionary)
        {
            Console.WriteLine("Parsing old manifest " + Path.GetFileName(filename));
            foreach (var entry in File.ReadAllLines(filename))
            {
                var parts = entry.Split(';');
                var fdidIndex = 2;
                var lookupIndex = 1;
                var convertToHex = false;

                if (parts.Length == 4)
                {
                    fdidIndex = 2;
                    lookupIndex = 1;
                }
                else if (parts.Length == 2)
                {
                    fdidIndex = 0;
                    lookupIndex = 1;
                    convertToHex = true;
                }
                else
                {
                    Console.WriteLine("Got entry with " + parts.Length + " parts, skipping line");
                    continue;
                }

                // Skip lines with empty lookup
                if (string.IsNullOrEmpty(parts[lookupIndex]))
                    continue;

                var lookup = parts[lookupIndex];
                if (convertToHex)
                {
                    lookup = ulong.Parse(lookup).ToString("X2").ToLowerInvariant().PadLeft(16, '0');
                }

                if (!int.TryParse(parts[fdidIndex], out var fdid))
                {
                    Console.WriteLine("Failed to parse FDID: " + parts[fdidIndex]);
                    continue;
                }

                if (outputDictionary.TryGetValue(fdid, out var currentEntry))
                {
                    if (currentEntry != lookup)
                        Console.WriteLine("Lookup mismatch: " + currentEntry + " vs " + lookup);

                    continue;
                }

                outputDictionary[fdid] = lookup;
            }
        }

        struct OldManifestEntry
        {
            public string Lookup;
        }
        private static void Merge(string inputManifest, string outputManifest)
        {
            var outputDict = new Dictionary<int, string>();
            var outputLines = File.ReadAllLines(outputManifest);
            foreach (var line in outputLines)
            {
                var parts = line.Split(';');
                if (parts.Length != 2)
                {
                    Console.WriteLine("Got entry with " + parts.Length + " parts, skipping line");
                    continue;
                }

                if (!int.TryParse(parts[0], out var fdid))
                {
                    Console.WriteLine("Failed to parse FDID: " + parts[0]);
                    continue;
                }

                outputDict[fdid] = parts[1];
            }

            var inputDict = new Dictionary<int, string>();
            var inputLines = File.ReadAllLines(inputManifest);
            foreach (var line in inputLines)
            {
                var parts = line.Split(';');
                if (parts.Length != 2)
                {
                    Console.WriteLine("Got entry with " + parts.Length + " parts, skipping line");
                    continue;
                }

                if (!int.TryParse(parts[0], out var fdid))
                {
                    Console.WriteLine("Failed to parse FDID: " + parts[0]);
                    continue;
                }

                inputDict[fdid] = parts[1];
            }

            foreach (var entry in inputDict)
            {
                if (!outputDict.TryGetValue(entry.Key, out string? value))
                {
                    outputDict[entry.Key] = entry.Value;
                }
                else
                {
                    if(value != entry.Value)
                        Console.WriteLine("Lookup mismatch: " + value + " vs " + entry.Value + ", skipping");
                }
            }
            
            Console.WriteLine("Merging " + outputDict.Count + " entries into " + outputManifest);
            using (var writer = new StreamWriter(outputManifest, false))
            {
                foreach (var entry in outputDict.OrderBy(x => x.Key))
                {
                    writer.WriteLine(entry.Key + ";" + entry.Value);
                }
            }
        }

        private static void Dump(string v1, string v2)
        {
            // TODO: Read root manifest (both old and new formats) and dump to CSV for merging
            throw new NotImplementedException();
        }
    }
}
