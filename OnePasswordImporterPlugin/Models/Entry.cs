using System;
using System.Collections.Generic;
using System.IO;

namespace OnePasswordImporterPlugin.Models
{
    partial class Entry
    {
        public string _name;

        public EntryType Type { get; private set; }

        public Guid Id { get; private set; }

        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? "[Unknown]" : _name;
            set => _name = value;
        }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string Url { get; private set; }

        public List<string> Keys { get; } = new List<string>();

        public List<Entry> Sections { get; } = new List<Entry>();

        public Dictionary<string, string> CustomFields { get; } = new Dictionary<string, string>();

        public bool IsEmpty
            => string.IsNullOrEmpty(UserName)
            && string.IsNullOrEmpty(Password)
            && string.IsNullOrEmpty(Url)
            && Keys.Count == 0
            && Sections.Count == 0
            && CustomFields.Count == 0;

        public static Entry Read(TextReader reader)
        {
            var fields = new List<KeyValuePair<string, string>>();

            do
            {
                string line = reader.ReadLine();

                // End of entry
                if (string.IsNullOrEmpty(line))
                    break;
                
                var pair = line.Split(new[] { '=' }, 2);
                if (fields.Count == 0)
                {
                    if (pair.Length != 2 || pair[0] != "uuid")
                        throw new InvalidDataException("An entry must start with UUID.");
                }

                if (pair.Length == 2)
                {
                    // New key/value pair
                    var key = pair[0];
                    fields.Add(new KeyValuePair<string, string>(key, pair[1]));
                }
                else if (fields.Count > 0)
                {
                    // Append
                    var last = fields[fields.Count - 1];
                    var value = last.Value + '\n' + line;
                    fields[fields.Count - 1] = new KeyValuePair<string, string>(last.Key, value);
                }

            } while (true);

            return Parse(fields);
        }

        private static Entry Parse(IEnumerable<KeyValuePair<string, string>> fields)
        {
            var entry = new Entry();

            foreach (var field in fields)
                entry.Parse(field.Key, field.Value);

            entry.Sections.RemoveAll(e => e.IsEmpty);

            return entry;
        }

        private void Parse(string key, string value)
        {
            var entry = Sections.Count == 0
                      ? this
                      : Sections[Sections.Count-1];

            switch (key)
            {
                case "uuid":
                    var id = Encoding.Base32.FromBase32String(value);
                    entry.Id = new Guid(id);
                    break;
                case "category":
                    if (int.TryParse(value, out int type))
                        Type = (EntryType)type;
                    break;
                case "section":
                    switch (value)
                    {
                        case "Customer":
                        case "Publisher":
                        case "Order":
                            // Ignore
                            break;
                        default:
                            AddSection(value);
                            break;
                    }
                    break;
                case "title":
                    entry.Name = value;
                    break;
                case "key":
                case "license key":
                    entry.Keys.Add(value);
                    break;
                case "username":
                case "licensed to":
                case "network name":
                case "name on account":
                    entry.UserName = value;
                    break;
                case "password":
                case "wireless network password":
                    entry.Password = value;
                    break;
                case "host":
                case "website":
                    entry.Url = value;
                    break;
                case "ainfo":
                case "scope":
                case "autoSubmit":
                    // Ignore
                    break;
                default:
                    // Unknown field, save in dict
                    if (!string.IsNullOrEmpty(key))
                        CustomFields.Add(key, value);
                    break;
            }
        }

        private Entry AddSection(string name)
        {
            var i = 0;
            var baseName = name;
            Entry section = null;

            do
            {
                name = i == 0 ? baseName : baseName + $" ({i})";
                section = Sections.Find(s => s.Name == name);
                i++;
            }
            while (section != null);

            section = new Entry { Name = name };
            Sections.Add(section);
            return section;
        }
    }
}