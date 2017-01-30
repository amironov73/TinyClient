// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* Program.cs --
 * Ars Magna project, http://arsmagna.ru
 * -------------------------------------------------------
 * Status: poor
 */

#region Using directives

using System;
using System.Net;

using TinyClient;

#endregion

namespace MicroTestRunner
{
    class Program
    {
        static void Main()
        {
            IrbisClient client = null;
            try
            {
                client = new IrbisClient
                {
                    Host = IPAddress.Parse("127.0.0.1"),
                    Port = 6666,
                    Username = "1",
                    Password = "1",
                    Database = "IBIS"
                };

                string[] iniLines = client.Connect();
                Console.WriteLine("Connected: {0} lines in INI-file", iniLines.Length);

                IniFile iniFile = IniFile.Parse(iniLines);
                Console.WriteLine("INI FILE: {0}", iniFile.Length);

                DatabaseInfo[] databases = client.ListDatabases("1..DBNAM2.MNU");
                Console.WriteLine("DATABASES: {0}", databases.Length);
                foreach (DatabaseInfo database in databases)
                {
                    Console.WriteLine(database);
                }

                Console.WriteLine("CURDB={0}", client.Database);

                SearchScenario[] scenarios = client.LoadSearchScenario("10.IBIS.IBIS.INI");
                if (ReferenceEquals(scenarios, null))
                {
                    Console.WriteLine("No custom search scenarios");
                }
                else
                {
                    Console.WriteLine("Custom search scenarios: {0}", scenarios.Length);
                }

                int maxMfn = client.GetMaxMfn();
                Console.WriteLine("Max MFN={0}", maxMfn);

                MenuFile menu = client.LoadMenu("10.IBIS.PFTW.MNU");
                Console.WriteLine("MENU: {0} entries", menu.Length);

                client.Nop();
                Console.WriteLine("NOP");

                string brief = client.FormatRecord("@brief", 1);
                Console.WriteLine("BRIEF: {0}", brief);

                string full = client.FormatRecord("@", 1);
                Console.WriteLine("FULL: {0}", full);

                int[] found = client.Search("\"K=БЕ$\"");
                Console.WriteLine("SEARCH: {0} found", found.Length);
                if (found.Length != 0)
                {
                    Console.Write(" => ");
                    for (int i = 0; i < found.Length; i++)
                    {
                        if (i != 0)
                        {
                            Console.Write(", ");
                        }
                        Console.Write(found[i]);
                    }
                    Console.WriteLine();
                }

                TermInfo[] terms = client.ListTerms("K=", 10);
                Console.WriteLine("TERMS: {0}", terms.Length);
                foreach (TermInfo term in terms)
                {
                    Console.WriteLine(term);
                }

                MarcRecord record = client.ReadRecord(1);
                Console.WriteLine("READ: {0}", record);

                string formatted = client.FormatRecord("@brief", record);
                Console.WriteLine("FORMAT: {0}", formatted);

                record.Mfn = 0;
                record.Version = 0;
                RecordField field3000 = new RecordField("3000")
                {
                    Value = DateTime.Now.ToLongDateString()
                };
                record.Fields.Add(field3000);
                record = client.WriteRecord(record);
                Console.WriteLine("WRITE: {0}", record);
            }
            finally
            {
                if (!ReferenceEquals(client, null))
                {
                    client.Dispose();
                    Console.WriteLine("Disconnected");
                }
            }
        }
    }
}
