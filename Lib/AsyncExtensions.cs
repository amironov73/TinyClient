// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* AsyncExtensions.cs --
 * Ars Magna project, http://arsmagna.ru
 * -------------------------------------------------------
 * Status: poor
 */

#region Using directives

using System.Threading.Tasks;

#endregion

namespace TinyClient
{
    public static class AsyncExtensions
    {
        public static Task<string[]> ConnectAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(() => client.Connect());
        }

        public static Task DisposeAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(client.Dispose);
        }

        public static Task<string> FormatRecordAsync(this IrbisClient client, string format, int mfn)
        {
            return Task.Factory.StartNew(() => client.FormatRecord(format, mfn));
        }

        public static Task<string> FormatRecordAsync(this IrbisClient client, string format, MarcRecord record)
        {
            return Task.Factory.StartNew(() => client.FormatRecord(format, record));
        }

        public static Task<int> GetMaxMfnAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(() => client.GetMaxMfn());
        }

        public static Task<DatabaseInfo[]> ListDatabasesAsync(this IrbisClient client, string menuName)
        {
            return Task.Factory.StartNew(() => client.ListDatabases(menuName));
        }

        public static Task<TermInfo[]> ListTermsAsync(this IrbisClient client, string start, int count)
        {
            return Task.Factory.StartNew(() => client.ListTerms(start, count));
        }

        public static Task<MenuFile> LoadMenuAsync(this IrbisClient client, string menuName)
        {
            return Task.Factory.StartNew(() => client.LoadMenu(menuName));
        }

        public static Task<IniFile> LoadIniFileAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.LoadIniFile(fileName));
        }

        public static Task<SearchScenario[]> LoadSearchScenarioAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.LoadSearchScenario(fileName));
        }

        public static Task NopAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(client.Nop);
        }

        public static Task<MarcRecord> ReadRecordAsync(this IrbisClient client, int mfn)
        {
            return Task.Factory.StartNew(() => client.ReadRecord(mfn));
        }

        public static Task<string> ReadTextFileAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.ReadTextFile(fileName));
        }

        public static Task<int[]> SearchAsync(this IrbisClient client, string expression)
        {
            return Task.Factory.StartNew(() => client.Search(expression));
        }

        public static Task<MarcRecord> WriteRecordAsync(this IrbisClient client, MarcRecord record)
        {
            return Task.Factory.StartNew(() => client.WriteRecord(record));
        }
    }
}
