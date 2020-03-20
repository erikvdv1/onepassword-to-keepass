using KeePass.DataExchange;
using KeePassLib;
using KeePassLib.Interfaces;
using System.IO;
using System.Collections.Generic;
using OnePasswordImporterPlugin.Models;
using KeePass.Resources;

namespace OnePasswordImporterPlugin
{
    public class OnePasswordFileFormatProvider : FileFormatProvider
    {
        public override bool SupportsImport => true;
        public override bool SupportsExport => false;
        public override string FormatName => "1Password 6 TXT";
        public override string DefaultExtension => "txt";
        public override string ApplicationGroup => KPRes.PasswordManagers;
        public override System.Drawing.Image SmallIcon => Resources.OnePasswordSmall;

        public override void Import(PwDatabase pwStorage, Stream sInput, IStatusLogger slLogger)
        {
            var entries = new List<Entry>();
            var name = GetName(sInput);
            var rootGroup = CreateGroup(pwStorage, name);

            using (var reader = new StreamReader(sInput, System.Text.Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    var entry = Entry.Read(reader);
                    entries.Add(entry);
                }
            }

            for (int i = 0; i < entries.Count; i++ )
            {
                var entry = entries[i];
                slLogger.SetText(string.Format("{0} ({1} of {2})", entry.Name, i + 1, entries.Count), LogStatusType.Info);
                AddEntry(pwStorage, rootGroup, entry);
            }
        }

        private string GetName(Stream sInput)
        {
            var fileName = (sInput as FileStream)?.Name ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(fileName);
            return name ?? string.Empty;
        }

        private void AddEntry(PwDatabase pwStorage, PwGroup group, Entry entry)
        {
            var pwModel = entry.ToPw(pwStorage);

            // Add group
            var pwGroup = pwModel as PwGroup;
            if (pwGroup != null)
            {
                group.AddGroup(pwGroup, true);
                return;
            }

            // Add entry
            var pwEntry = pwModel as PwEntry;
            if (pwEntry != null)
            {
                group.AddEntry(pwEntry, true);
                return;
            }
        }

        private PwGroup CreateGroup(PwDatabase pwStorage, string name)
        {
            name = string.IsNullOrEmpty(name) ? "1Password" : $"{name} (1Password)";
            return pwStorage.RootGroup.FindCreateGroup(name, true);
        }
    }
}
