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

// ReSharper disable ConvertClosureToMethodGroup

namespace TinyClient
{
    public static class AsyncExtensions
    {
        public static Task ConfigureSafe(this Task task)
        {
            task.ConfigureAwait(false);
            return task;
        }

        public static Task<T> ConfigureSafe<T>(this Task<T> task)
        {
            task.ConfigureAwait(false);
            return task;
        }

        public static Task<string[]> ConnectAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(() => client.Connect())
                .ConfigureSafe();
        }

        public static Task DisposeAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(client.Dispose)
                .ConfigureSafe();
        }

        public static Task<string> FormatRecordAsync(this IrbisClient client, string format, int mfn)
        {
            return Task.Factory.StartNew(() => client.FormatRecord(format, mfn))
                .ConfigureSafe();
        }

        public static Task<string> FormatRecordAsync(this IrbisClient client, string format, MarcRecord record)
        {
            return Task.Factory.StartNew(() => client.FormatRecord(format, record))
                .ConfigureSafe();
        }

        public static Task<int> GetMaxMfnAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(() => client.GetMaxMfn())
                .ConfigureSafe();
        }

        public static Task<DatabaseInfo[]> ListDatabasesAsync(this IrbisClient client, string menuName)
        {
            return Task.Factory.StartNew(() => client.ListDatabases(menuName))
                .ConfigureSafe();
        }

        public static Task<TermInfo[]> ListTermsAsync(this IrbisClient client, string start, int count)
        {
            return Task.Factory.StartNew(() => client.ListTerms(start, count))
                .ConfigureSafe();
        }

        public static Task<MenuFile> LoadMenuAsync(this IrbisClient client, string menuName)
        {
            return Task.Factory.StartNew(() => client.LoadMenu(menuName))
                .ConfigureSafe();
        }

        public static Task<IniFile> LoadIniFileAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.LoadIniFile(fileName))
                .ConfigureSafe();
        }

        public static Task<SearchScenario[]> LoadSearchScenarioAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.LoadSearchScenario(fileName))
                .ConfigureSafe();
        }

        public static Task NopAsync(this IrbisClient client)
        {
            return Task.Factory.StartNew(client.Nop)
                .ConfigureSafe();
        }

        public static Task<MarcRecord> ReadRecordAsync(this IrbisClient client, int mfn)
        {
            return Task.Factory.StartNew(() => client.ReadRecord(mfn))
                .ConfigureSafe();
        }

        public static Task<string> ReadTextFileAsync(this IrbisClient client, string fileName)
        {
            return Task.Factory.StartNew(() => client.ReadTextFile(fileName))
                .ConfigureSafe();
        }

        public static Task<int[]> SearchAsync(this IrbisClient client, string expression)
        {
            return Task.Factory.StartNew(() => client.Search(expression))
                .ConfigureSafe();
        }

        public static Task<MarcRecord> WriteRecordAsync(this IrbisClient client, MarcRecord record)
        {
            return Task.Factory.StartNew(() => client.WriteRecord(record))
                .ConfigureSafe();
        }
    }
}
