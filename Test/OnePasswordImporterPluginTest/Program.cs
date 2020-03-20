using KeePassLib;
using OnePasswordImporterPlugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace OnePasswordImporterPluginTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new PwDatabase
            {
                RootGroup = new PwGroup(true, true, "Root", PwIcon.Folder)
            };

            var logger = new ConsoleLogger();
            var file = new FileStream("Resources/example.txt", FileMode.Open);
            var provider = new OnePasswordFileFormatProvider();

            provider.Import(database, file, logger);

            CheckForDuplicates(database);

            Console.WriteLine("Done");
        }

        private static void CheckForDuplicates(PwDatabase pwStorage)
        {
            var dict = new Dictionary<Guid, string>();
            var root = pwStorage.RootGroup;

            Action<PwGroup> func = null;
            func = (group) =>
            {
                var key = new Guid(group.Uuid.UuidBytes);
                var value = group.Name;

                try
                {
                    dict.Add(key, value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Duplicate key for: '{dict[key]}' and '{value}'", ex);
                }

                foreach (var entry in group.Entries)
                {
                    key = new Guid(entry.Uuid.UuidBytes);
                    value = entry.Strings.GetSafe(PwDefs.TitleField).ReadString();

                    try
                    {
                        dict.Add(key, value);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"Duplicate key for: '{dict[key]}' and '{value}'", ex);
                    }
                }

                foreach (var g in group.Groups)
                    func(g);
            };

            func(root);
        }
    }
}
