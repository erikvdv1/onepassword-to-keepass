using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using OnePasswordImporterPlugin.Utility;
using System;

namespace OnePasswordImporterPlugin.Models
{
    partial class Entry
    {
        public IStructureItem ToPw(PwDatabase database)
        {
            var entry = ToPwEntry(database, this);

            if (Sections.Count == 0)
                return entry;

            var group = new PwGroup
            {
                Uuid = GetUuid("[Group]"),
                Name = Name,
            };

            if (!IsEmpty)
                group.AddEntry(entry, true);

            foreach (var section in Sections)
            {
                entry = ToPwEntry(database, section);
                group.AddEntry(entry, true);
            }

            return group;
        }

        private PwEntry ToPwEntry(PwDatabase database, Entry entry)
        {
            var pwEntry = new PwEntry(false, true)
            {
                Uuid = GetUuid(entry),
            };

            var name = this == entry ? entry.Name : $"{Name} {entry.Name}";

            // Set standard fields
            pwEntry.Strings.Set(PwDefs.TitleField, new ProtectedString(database.MemoryProtection.ProtectTitle, name));
            if (!string.IsNullOrEmpty(entry.UserName))
                pwEntry.Strings.Set(PwDefs.UserNameField, new ProtectedString(database.MemoryProtection.ProtectUserName, entry.UserName));
            if (!string.IsNullOrEmpty(entry.Password))
                pwEntry.Strings.Set(PwDefs.PasswordField, new ProtectedString(database.MemoryProtection.ProtectPassword, entry.Password));
            if (!string.IsNullOrEmpty(entry.Url))
                pwEntry.Strings.Set(PwDefs.UrlField, new ProtectedString(database.MemoryProtection.ProtectUrl, entry.Url));

            // Keys
            var notes = string.Join("\n", entry.Keys.ToArray());
            if (!string.IsNullOrEmpty(notes))
                pwEntry.Strings.Set(PwDefs.NotesField, new ProtectedString(database.MemoryProtection.ProtectNotes, notes));
            
            // Custom fields
            foreach (var field in entry.CustomFields)
                pwEntry.Strings.Set(field.Key, new ProtectedString(false, field.Value));

            return pwEntry;
        }

        private PwUuid GetUuid(Entry entry)
        {
            if (this == entry && Id != default(Guid))
                return new PwUuid(entry.Id.ToByteArray());
            else
                return GetUuid(entry.Name);
        }

        private PwUuid GetUuid(string name)
        {
            Guid guid;
            if (Id == default(Guid) || string.IsNullOrEmpty(name))
                guid = Guid.NewGuid();
            else
                guid = GuidUtility.Create(Id, name);

            return new PwUuid(guid.ToByteArray());
        }
    }
}