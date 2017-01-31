// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* TinyClient.cs --
 * Ars Magna project, http://arsmagna.ru
 * -------------------------------------------------------
 * Status: poor
 */

#region Using directives

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

#endregion

namespace TinyClient
{
    public sealed class SubField
    {
        public char Code { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                string text = value;
                if (!string.IsNullOrEmpty(text))
                {
                    text = Utility.ReplaceControlChars(text, ' ').Trim();
                }
                _value = text;
            }
        }

        private string _value;

        public override string ToString()
        {
            return "^" + Code + Value;
        }
    }

    public sealed class SubFieldCollection
        : IEnumerable
    {
        private readonly ArrayList _list;

        public int Length { get { return _list.Count; } }

        public SubField this[int index]
        {
            get { return (SubField)_list[index]; }
            set { _list[index] = value; }
        }

        public SubFieldCollection()
        {
            _list = new ArrayList();
        }

        public void Add(SubField item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Remove(SubField item)
        {
            _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public sealed class RecordField
    {
        private readonly SubFieldCollection _subFields;

        public string Tag { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                string text = value;
                if (!string.IsNullOrEmpty(text))
                {
                    text = Utility.ReplaceControlChars(text, ' ').Trim();
                }
                _value = text;
            }
        }

        private string _value;

        public SubFieldCollection SubFields
        {
            get { return _subFields; }
        }

        public RecordField(string tag)
        {
            Tag = tag;
            _subFields = new SubFieldCollection();
        }

        internal static RecordField Parse(string line)
        {
            StringReader reader = new StringReader(line);
            RecordField result = new RecordField(Utility.ReadTo(reader, '#'))
            {
                Value = Utility.ReadTo(reader, '^')
            };
            while (true)
            {
                int next = reader.Read();
                if (next < 0)
                {
                    break;
                }
                char code = char.ToLower((char)next);
                string value = Utility.ReadTo(reader, '^');
                SubField subField = new SubField
                {
                    Code = code,
                    Value = value
                };
                result.SubFields.Add(subField);
            }
            return result;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(Tag);
            result.Append("#");
            if (!string.IsNullOrEmpty(Value))
            {
                result.Append(Value);
            }
            foreach (SubField subField in SubFields)
            {
                result.Append(subField);
            }
            return result.ToString();
        }
    }

    public sealed class FieldCollection
        : IEnumerable
    {
        private readonly ArrayList _list;

        public int Length { get { return _list.Count; } }

        public RecordField this[int index]
        {
            get { return (RecordField)_list[index]; }
            set { _list[index] = value; }
        }

        public FieldCollection()
        {
            _list = new ArrayList();
        }

        public void Add(RecordField item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Remove(RecordField item)
        {
            _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public sealed class MarcRecord
    {
        private readonly FieldCollection _fields;

        public FieldCollection Fields
        {
            get { return _fields; }
        }

        public int Mfn { get; set; }

        public int Status { get; set; }

        public int Version { get; set; }

        public string Database { get; set; }

        public MarcRecord()
        {
            _fields = new FieldCollection();
        }

        internal static void ParseOneOfMany(MarcRecord record, string text)
        {
            record.Fields.Clear();
            string[] lines = text.Split(new[] { Utility.ShortDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            ParseSingle(record, lines);
        }

        internal static void ParseSingle(MarcRecord record, string[] text)
        {
            char[] delimiters = { '#' };
            string line = text[0];
            string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            record.Mfn = int.Parse(parts[0]);
            if (parts.Length != 1)
            {
                record.Status = int.Parse(parts[1]);
            }
            line = text[1];
            parts = line.Split(delimiters);
            record.Version = int.Parse(parts[1]);
            for (int i = 2; i < text.Length; i++)
            {
                RecordField field = RecordField.Parse(text[i]);
                record.Fields.Add(field);
            }
        }

        internal string Encode()
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0}#{1}", Mfn, Status);
            result.Append(Utility.IrbisDelimiter);
            result.AppendFormat("0#{0}", Version);
            foreach (RecordField field in Fields)
            {
                result.Append(Utility.IrbisDelimiter);
                result.Append(field);
            }
            return result.ToString();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0}#{1}", Mfn, Status);
            result.AppendLine();
            result.AppendFormat("0#{0}", Version);
            foreach (RecordField field in Fields)
            {
                result.AppendLine();
                result.Append(field);
            }
            return result.ToString();
        }
    }

    public sealed class DatabaseInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public static DatabaseInfo[] ParseMenu(string text)
        {
            string[] lines = Utility.SplitIrbisLines(text);
            ArrayList list = new ArrayList(lines.Length / 2 + 1);
            for (int i = 0; i < text.Length; i += 2)
            {
                string name = lines[i];
                if (string.IsNullOrEmpty(name)
                    || name.StartsWith("*"))
                {
                    break;
                }
                if (name.StartsWith("-"))
                {
                    name = name.Substring(1);
                }
                string description = lines[i + 1];
                DatabaseInfo oneBase = new DatabaseInfo
                {
                    Name = name,
                    Description = description,
                };
                list.Add(oneBase);
            }
            DatabaseInfo[] result = new DatabaseInfo[list.Count];
            list.CopyTo(result);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Description);
        }
    }

    public sealed class MenuEntry
    {
        public string Code { get; set; }

        public string Comment { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Code, Comment);
        }
    }

    public sealed class MenuFile
        : IEnumerable
    {
        private readonly ArrayList _entries;

        public int Length { get { return _entries.Count; } }

        public MenuFile()
        {
            _entries = new ArrayList();
        }

        private MenuFile(int capacity)
        {
            _entries = new ArrayList(capacity);
        }

        public void Add(MenuEntry entry)
        {
            _entries.Add(entry);
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public MenuEntry FindEntry(string code)
        {
            foreach (MenuEntry entry in _entries)
            {
                if (Utility.SameString(entry.Code, code))
                {
                    return entry;
                }
            }
            return null;
        }

        public static MenuFile ParseMenu(string text)
        {
            string[] lines = Utility.SplitIrbisLines(text);
            MenuFile result = new MenuFile(lines.Length / 2 + 1);
            for (int i = 0; i < lines.Length; i += 2)
            {
                string code = lines[i];
                if (code.StartsWith("*****"))
                {
                    break;
                }
                string comment = lines[i + 1];
                MenuEntry entry = new MenuEntry
                {
                    Code = code,
                    Comment = comment
                };
                result.Add(entry);
            }
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }

    public sealed class IniLine
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public IniLine()
        {
        }

        public IniLine(string key)
        {
            Key = key;
        }

        public IniLine(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Key, Value);
        }
    }

    public sealed class IniSection
        : IEnumerable
    {
        private readonly ArrayList _lines;

        public int Length { get { return _lines.Count; } }

        public string Name { get; set; }

        public IniLine this[int index]
        {
            get { return (IniLine)_lines[index]; }
            set { _lines[index] = value; }
        }

        public string this[string key]
        {
            get
            {
                IniLine line = FindLine(key);
                return ReferenceEquals(line, null)
                    ? string.Empty
                    : line.Value;
            }
            set
            {
                SetValue(key, value);
            }
        }

        public IniSection()
        {
            _lines = new ArrayList();
        }

        public void Add(IniLine line)
        {
            _lines.Add(line);
        }

        public void Clear()
        {
            _lines.Clear();
        }

        public IniLine FindLine(string key)
        {
            foreach (IniLine line in _lines)
            {
                if (Utility.SameString(line.Key, key))
                {
                    return line;
                }
            }
            return null;
        }

        public void SetValue(string key, string value)
        {
            IniLine line = FindLine(key);
            if (ReferenceEquals(line, null))
            {
                line = new IniLine(key, value);
                Add(line);
            }
            else
            {
                line.Value = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Name);
        }
    }

    public sealed class IniFile
        : IEnumerable
    {
        private readonly ArrayList _sections;

        public int Length { get { return _sections.Count; } }

        public IniSection this[int index]
        {
            get { return (IniSection)_sections[index]; }
            set { _sections[index] = value; }
        }

        public string this[string sectionName, string key]
        {
            get { return GetValue(sectionName, key, string.Empty); }
            set { SetValue(sectionName, key, value); }
        }

        public IniFile()
        {
            _sections = new ArrayList();
        }

        public void Add(IniSection section)
        {
            _sections.Add(section);
        }

        public void Clear()
        {
            _sections.Clear();
        }

        public IniSection GetSection(string name)
        {
            foreach (IniSection section in _sections)
            {
                if (Utility.SameString(section.Name, name))
                {
                    return section;
                }
            }
            return null;
        }

        public string GetValue(string sectionName, string key, string defaultValue)
        {
            IniSection section = GetSection(sectionName);
            if (ReferenceEquals(section, null))
            {
                return defaultValue;
            }
            IniLine line = section.FindLine(key);
            if (ReferenceEquals(line, null))
            {
                return defaultValue;
            }
            return line.Value;
        }

        public static IniFile Parse(string[] lines)
        {
            IniFile result = new IniFile();
            IniSection section = null;
            char[] delimiters = { '=' };
            foreach (string line in lines)
            {
                string text = line.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                if (text.StartsWith("["))
                {
                    text = text.Trim('[', ']');
                    section = new IniSection { Name = text };
                    result.Add(section);
                }
                else
                {
                    string[] parts = text.Split(delimiters, 2);
                    string key = parts[0];
                    string value = parts.Length == 2
                        ? parts[1]
                        : string.Empty;
                    IniLine item = new IniLine(key, value);
                    if (!ReferenceEquals(section, null))
                    {
                        section.Add(item);
                    }
                }
            }

            return result;
        }

        public void SetValue(string sectionName, string key, string value)
        {
            IniSection section = GetSection(sectionName);
            if (ReferenceEquals(section, null))
            {
                section = new IniSection { Name = sectionName };
                Add(section);
            }
            section.SetValue(key, value);
        }

        public IEnumerator GetEnumerator()
        {
            return _sections.GetEnumerator();
        }
    }

    public sealed class SearchScenario
    {
        public string Name { get; set; }

        public string Prefix { get; set; }

        public string DictionaryType { get; set; }

        public string MenuName { get; set; }

        public string OldFormat { get; set; }

        public string Correction { get; set; }

        public string Truncation { get; set; }

        public string Hint { get; set; }

        public string ModByDicAuto { get; set; }

        public string Logic { get; set; }

        public string Advance { get; set; }

        public string Format { get; set; }

        public static SearchScenario[] ParseIniFile(IniFile iniFile)
        {
            IniSection section = iniFile.GetSection("SEARCH");
            if (ReferenceEquals(section, null))
            {
                return new SearchScenario[0];
            }
            int count = int.Parse(section["ItemNumb"]);
            if (count == 0)
            {
                return new SearchScenario[0];
            }
            ArrayList list = new ArrayList(count);
            for (int i = 0; i < count; i++)
            {
                string name = section["ItemName" + i];
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                SearchScenario scenario = new SearchScenario
                {
                    Name = name,
                    Prefix = section["ItemPref" + i],
                    DictionaryType = section["ItemDictionType" + i],
                    Advance = section["ItemAdv" + i],
                    Format = section["ItemPft" + i],
                    Hint = section["ItemHint" + i],
                    Logic = section["ItemLogic" + i],
                    MenuName = section["ItemMenu" + i],
                    ModByDicAuto = section["ItemModByDicAuto" + i],
                    Correction = section["ModByDic" + i],
                    Truncation = section["ItemTranc" + i]
                };

                list.Add(scenario);
            }
            SearchScenario[] result = new SearchScenario[list.Count];
            list.CopyTo(result);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Prefix);
        }
    }

    public sealed class TermInfo
    {
        public int Count { get; set; }

        public string Text { get; set; }

        public static TermInfo[] Parse(string[] lines)
        {
            ArrayList list = new ArrayList(lines.Length + 1);
            char[] delimiters = { '#' };
            foreach (string line in lines)
            {
                string[] parts = line.Split(delimiters, 2);
                TermInfo info = new TermInfo
                {
                    Count = int.Parse(parts[0]),
                    Text = parts[1]
                };
                list.Add(info);
            }
            TermInfo[] result = new TermInfo[list.Count];
            list.CopyTo(result);
            return result;
        }

        public static TermInfo[] TrimPrefix(TermInfo[] terms, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return terms;
            }
            int prefixLength = prefix.Length;
            ArrayList list = new ArrayList(terms.Length);
            foreach (TermInfo term in terms)
            {
                string text = term.Text;
                if (!string.IsNullOrEmpty(text)
                    && text.StartsWith(prefix))
                {
                    text = text.Substring(prefixLength);
                }
                TermInfo clone = new TermInfo
                {
                    Count = term.Count,
                    Text = text
                };
                list.Add(clone);
            }
            TermInfo[] result = new TermInfo[list.Count];
            list.CopyTo(result);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Count, Text);
        }
    }

    public sealed class IrbisClient
    {
        public IPAddress Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public char Workstation { get; set; }

        public string Database { get; set; }

        public int ClientID { get; set; }

        public int QueryID { get; set; }

        public TextWriter DebugOutput { get; set; }

        public bool Connected { get { return _connected; } }

        public IrbisClient()
        {
            Host = IPAddress.Loopback;
            Port = 6666;
            Workstation = 'C';
            Database = "IBIS";
            ClientID = new Random().Next(100000, 900000);
            QueryID = 0;
        }

        private bool _connected;

        private Response ExecuteQuery(Query query)
        {
            using (TcpClient connection = new TcpClient())
            {
                connection.Connect(Host, Port);
                byte[] data = query.Encode();
                if (!ReferenceEquals(DebugOutput, null))
                {
                    DebugOutput.WriteLine("QUERY {0}:", QueryID);
                    DebugOutput.WriteLine();
                    Utility.DumpBytes(DebugOutput, data);
                    DebugOutput.WriteLine();
                }
                Socket socket = connection.Client;
                socket.Send(data);
                return new Response(this, socket);
            }
        }

        public string[] Connect()
        {
            if (Connected)
            {
                return new string[0];
            }
            Query query = new Query(this, "A");
            query.AddAnsi(Username);
            query.AddAnsiNoLF(Password);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode();
            response.ReadAnsi();
            response.ReadAnsi();
            _connected = true;
            return response.ReadRemainingAnsiLines();
        }

        public void Dispose()
        {
            if (Connected)
            {
                Query query = new Query(this, "B");
                query.AddAnsiNoLF(Username);
                ExecuteQuery(query);
                _connected = false;
            }
        }

        public string FormatRecord(string format, int mfn)
        {
            Query query = new Query(this, "G");
            query.AddAnsi(Database);
            query.AddAnsi(format);
            query.Add(1);
            query.Add(mfn);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode();
            return response.ReadRemainingUtfText().Trim();
        }

        public string FormatRecord(string format, MarcRecord record)
        {
            Query query = new Query(this, "G");
            query.AddAnsi(Database);
            query.AddAnsi(format);
            query.Add(-2);
            query.AddUtf(record.Encode());
            Response response = ExecuteQuery(query);
            response.CheckReturnCode();
            return response.ReadUtf().Trim();
        }

        public int GetMaxMfn()
        {
            Query query = new Query(this, "O");
            query.AddAnsiNoLF(Database);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode();
            return response.ReturnCode;
        }

        public DatabaseInfo[] ListDatabases(string menuName)
        {
            return DatabaseInfo.ParseMenu(ReadTextFile(menuName));
        }

        public TermInfo[] ListTerms(string start, int count)
        {
            Query query = new Query(this, "H");
            query.AddAnsi(Database);
            query.AddAnsi(start);
            query.Add(count);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode(-202, -203, -204);
            string[] lines = response.ReadRemainingUtfLines();
            return TermInfo.Parse(lines);
        }

        public MenuFile LoadMenu(string menuName)
        {
            return MenuFile.ParseMenu(ReadTextFile(menuName));
        }

        public IniFile LoadIniFile(string fileName)
        {
            string[] lines = Utility.SplitIrbisLines(ReadTextFile(fileName));
            return IniFile.Parse(lines);
        }

        public SearchScenario[] LoadSearchScenario(string fileName)
        {
            string[] lines = Utility.SplitIrbisLines(ReadTextFile(fileName));
            if (lines.Length == 0)
            {
                return null;
            }
            IniFile iniFile = IniFile.Parse(lines);
            IniSection section = iniFile.GetSection("SEARCH");
            if (ReferenceEquals(section, null))
            {
                return null;
            }
            return SearchScenario.ParseIniFile(iniFile);
        }

        public void Nop()
        {
            Query query = new Query(this, "N");
            ExecuteQuery(query);
        }

        public MarcRecord ReadRecord(int mfn)
        {
            Query query = new Query(this, "C");
            query.AddAnsi(Database);
            query.Add(mfn);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode(-201, -600, -602, -603);
            string[] lines = response.ReadRemainingUtfLines();
            MarcRecord result = new MarcRecord();
            MarcRecord.ParseSingle(result, lines);
            result.Database = Database;
            return result;
        }

        public string ReadTextFile(string specification)
        {
            Query query = new Query(this, "L");
            query.AddAnsi(specification);
            Response response = ExecuteQuery(query);
            return response.ReadAnsi();
        }

        public int[] Search(string expression)
        {
            Query query = new Query(this, "K");
            query.AddAnsi(Database);
            query.AddUtf(expression);
            query.Add(0);
            query.Add(1);
            Response response = ExecuteQuery(query);
            response.CheckReturnCode();
            int howMany = response.ReadInt32();
            int[] result = new int[howMany];
            char[] delimiters = { '#' };
            for (int i = 0; i < howMany; i++)
            {
                string line = response.ReadAnsi();
                string[] parts = line.Split(delimiters);
                int mfn = int.Parse(parts[0]);
                result[i] = mfn;
            }
            return result;
        }

        public MarcRecord WriteRecord(MarcRecord record)
        {
            string database = record.Database;
            if (string.IsNullOrEmpty(database))
            {
                database = Database;
            }
            Query query = new Query(this, "D");
            query.AddAnsi(database);
            query.Add(0);
            query.Add(1);
            query.AddUtf(record.Encode());
            Response response = ExecuteQuery(query);
            response.CheckReturnCode(-201, -600, -602, -603);
            string line1 = response.ReadUtf();
            string line2 = response.ReadUtf();
            MarcRecord.ParseOneOfMany(record, line1 + Utility.ShortDelimiter + line2);
            record.Database = Database;
            return record;
        }
    }

    sealed class Query
    {
        private readonly MemoryStream _stream;

        public Query(IrbisClient client, string command)
        {
            _stream = new MemoryStream();
            AddAnsi(command);
            AddAnsi(client.Workstation.ToString());
            AddAnsi(command);
            Add(client.ClientID);
            Add(client.QueryID);
            client.QueryID++;
            AddAnsi(client.Password);
            AddAnsi(client.Username);
            AddLineFeed();
            AddLineFeed();
            AddLineFeed();
        }

        public void Add(bool value)
        {
            string text = value ? "1" : "0";
            AddAnsi(text);
        }

        public void Add(int value)
        {
            string text = value.ToString(CultureInfo.InvariantCulture);
            AddAnsi(text);
        }

        public void AddAnsi(string text)
        {
            AddAnsiNoLF(text);
            AddLineFeed();
        }

        public void AddAnsiNoLF(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes = Utility.Ansi.GetBytes(text);
                _stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void AddLineFeed()
        {
            _stream.Write(Utility.LF, 0, Utility.LF.Length);
        }

        public void AddUtf(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes = Utility.Utf.GetBytes(text);
                _stream.Write(bytes, 0, bytes.Length);
            }
            AddLineFeed();
        }

        public byte[] Encode()
        {
            byte[] buffer = _stream.ToArray();
            byte[] prefix = Utility.Ansi.GetBytes
                (
                    buffer.Length.ToString(CultureInfo.InvariantCulture)
                    + "\n"
                );
            byte[] result = new byte[prefix.Length + buffer.Length];
            Array.Copy(prefix, result, prefix.Length);
            Array.Copy(buffer, 0, result, prefix.Length, buffer.Length);

            return result;
        }
    }

    sealed class Response
    {
        private readonly MemoryStream _stream;

        public string Command { get; set; }

        public int ClientID { get; set; }

        public int QueryID { get; set; }

        public int ReturnCode { get; set; }

        public bool IsEOF { get { return _stream.Position >= _stream.Length; } }

        public Response(IrbisClient client, Socket socket)
        {
            _stream = new MemoryStream();

            byte[] buffer = new byte[32 * 1024];
            while (true)
            {
                int read = socket.Receive(buffer);
                if (read <= 0)
                {
                    break;
                }
                _stream.Write(buffer, 0, read);
            }

            if (!ReferenceEquals(client.DebugOutput, null))
            {
                client.DebugOutput.WriteLine("RESPONSE {0}:", client.QueryID);
                client.DebugOutput.WriteLine();
                Utility.DumpBytes(client.DebugOutput, _stream.ToArray());
                client.DebugOutput.WriteLine();
            }

            _stream.Seek(0, SeekOrigin.Begin);

            Command = ReadAnsi();
            ClientID = ReadInt32();
            QueryID = ReadInt32();
            for (int i = 0; i < 7; i++)
            {
                ReadAnsi();
            }
        }

        public void CheckReturnCode(params int[] allowed)
        {
            if (GetReturnCode() < 0)
            {
                if (Array.IndexOf(allowed, ReturnCode) < 0)
                {
                    throw new IrbisException(ReturnCode);
                }
            }
        }

        public byte[] GetLine()
        {
            MemoryStream result = new MemoryStream();
            while (true)
            {
                int one = _stream.ReadByte();
                if (one < 0)
                {
                    break;
                }
                if (one == 0x0D)
                {
                    one = _stream.ReadByte();
                    if (one == 0x0A)
                    {
                        break;
                    }
                }
                else
                {
                    result.WriteByte((byte)one);
                }
            }

            return result.ToArray();
        }

        public int GetReturnCode()
        {
            ReturnCode = ReadInt32();
            return ReturnCode;
        }

        public string ReadAnsi()
        {
            byte[] line = GetLine();
            return Utility.Ansi.GetString(line);
        }

        public int ReadInt32()
        {
            string text = ReadAnsi();
            int result;
            int.TryParse(text, out result);
            return result;
        }

        public string[] ReadRemainingAnsiLines()
        {
            ArrayList list = new ArrayList();
            while (!IsEOF)
            {
                string line = ReadAnsi();
                list.Add(line);
            }
            string[] result = new string[list.Count];
            list.CopyTo(result);
            return result;
        }

        public string[] ReadRemainingUtfLines()
        {
            ArrayList list = new ArrayList();
            while (!IsEOF)
            {
                string line = ReadUtf();
                list.Add(line);
            }
            string[] result = new string[list.Count];
            list.CopyTo(result);
            return result;
        }

        public string ReadRemainingUtfText()
        {
            MemoryStream buffer = new MemoryStream();
            _stream.CopyTo(buffer);
            return Utility.Utf.GetString(buffer.ToArray());
        }

        public string ReadUtf()
        {
            byte[] line = GetLine();
            return Utility.Utf.GetString(line);
        }
    }

    public sealed class Utility
    {
        private static readonly Encoding _utf8 = new UTF8Encoding(false, false);
        private static readonly Encoding _cp1251 = Encoding.GetEncoding(1251);

        public static Encoding Utf { get { return _utf8; } }
        public static Encoding Ansi { get { return _cp1251; } }

        public static byte[] CRLF = { 0x0D, 0x0A };
        public static byte[] LF = { 0x0A };

        public const string IrbisDelimiter = "\x001F\x001E";
        public const string ShortDelimiter = "\x001E";

        public static string ReadTo
            (
                StringReader reader,
                char delimiter
            )
        {
            StringBuilder result = new StringBuilder();

            while (true)
            {
                int next = reader.Read();
                if (next < 0)
                {
                    break;
                }
                char c = (char)next;
                if (c == delimiter)
                {
                    break;
                }
                result.Append(c);
            }

            return result.ToString();
        }

        public static string ReplaceControlChars(string text, char substitute)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            bool found = false;
            foreach (char c in text)
            {
                if (c < ' ')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return text;
            }
            StringBuilder result = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                result.Append(c < ' ' ? substitute : c);
            }
            return result.ToString();
        }

        public static string[] SplitIrbisLines(string text)
        {
            return text.Split(new[] { IrbisDelimiter }, StringSplitOptions.None);
        }

        public static bool SameString(string first, string second)
        {
            return string.Compare(first, second, true, CultureInfo.CurrentCulture) == 0;
        }

        public static bool SameChar(char first, char second)
        {
            return Char.ToUpper(first) == Char.ToUpper(second);
        }

        public static bool OneOf(char c1, params char[] array)
        {
            foreach (char c2 in array)
            {
                if (SameChar(c1, c2))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool OneOf(string c1, params string[] array)
        {
            foreach (string c2 in array)
            {
                if (SameString(c1, c2))
                {
                    return true;
                }
            }
            return false;
        }

        public static void DumpBytes(TextWriter writer, byte[] buffer)
        {
            for (int offset = 0; offset < buffer.Length; offset += 16)
            {
                writer.Write("{0:X6}", offset);
                int run = Math.Min(buffer.Length - offset, 16);
                for (int i = 0; i < run; i++)
                {
                    writer.Write(" {0:X2}", buffer[offset + i]);
                }
                writer.Write("  ");
                for (int i = 0; i < run; i++)
                {
                    char c = (char)buffer[offset + i];
                    if (c < 32 || c > 127)
                    {
                        c = ' ';
                    }
                    writer.Write(c);
                }
                writer.WriteLine();
            }
        }
    }

    public sealed class IrbisException : Exception
    {
        public int ErrorCode { get; set; }

        public IrbisException()
        {
        }

        public IrbisException(int code)
        {
            ErrorCode = code;
        }

        public IrbisException(int code, string message)
            : base(message)
        {
            ErrorCode = code;
        }

        public IrbisException(string message)
            : base(message)
        {
        }

        public override string ToString()
        {
            string text = base.ToString();
            if (ErrorCode != 0)
            {
                return string.Format("IRBIS exception. ErrorCode: {0}{1}{2}",
                    ErrorCode, Environment.NewLine, text);
            }
            return text;
        }
    }
}
