// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* RetryExtensions.cs -- 
 * Ars Magna project, http://arsmagna.ru
 * -------------------------------------------------------
 * Status: poor
 */

#region Using directives

using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace TinyClient
{
    public static class RetryExtensions
    {
        /// <summary>
        /// Number of retry attempts.
        /// </summary>
        public static int RetryCount = 5;

        /// <summary>
        /// Delay between attempts, milliseconds.
        /// </summary>
        public static int DelayInterval = 300;

        private static void _HandleException(int attempt,
            Exception exception)
        {
            // Log exception here
            Debug.WriteLine("Attempt " + attempt + ", Exception "
                            + exception.Message);

            Thread.Sleep(DelayInterval);
        }

        public static void Try(Action action)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static void Try<T1>(Action<T1> action,
            T1 argument1)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    action(argument1);
                    return;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static void Try<T1, T2>(Action<T1, T2> action,
            T1 argument1, T2 argument2)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    action(argument1, argument2);
                    return;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static void Try<T1, T2, T3>(Action<T1, T2, T3> action,
            T1 argument1, T2 argument2, T3 argument3)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    action(argument1, argument2, argument3);
                    return;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static TResult Try<TResult>(Func<TResult> func)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    TResult result = func();
                    return result;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static TResult Try<TResult, T1>(Func<T1, TResult> func,
            T1 argument1)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    TResult result = func(argument1);
                    return result;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static TResult Try<TResult, T1, T2>(
            Func<T1, T2, TResult> func, T1 argument1, T2 argument2)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    TResult result = func(argument1, argument2);
                    return result;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static TResult Try<TResult, T1, T2, T3>(
            Func<T1, T2, T3, TResult> func, T1 argument1,
            T2 argument2, T3 argument3)
        {
            for (int attempt = 0; attempt < RetryCount; attempt++)
            {
                try
                {
                    TResult result = func(argument1, argument2, argument3);
                    return result;
                }
                catch (Exception ex)
                {
                    _HandleException(attempt, ex);
                }
            }

            throw new Exception("All attempts failed");
        }

        public static string[] ConnectRetry(this IrbisClient client)
        {
            Func<string[]> func = client.Connect;
            return Try(func);
        }

        public static void DisposeRetry(this IrbisClient client)
        {
            Try(client.Dispose);
        }

        public static string FormatRecordRetry(this IrbisClient client,
            string format, int mfn)
        {
            Func<string, int, string> func = client.FormatRecord;
            return Try(func, format, mfn);
        }

        public static string FormatRecordRetry(this IrbisClient client,
            string format, MarcRecord record)
        {
            Func<string, MarcRecord, string> func = client.FormatRecord;
            return Try(func, format, record);
        }

        public static int GetMaxMfnRetry(this IrbisClient client)
        {
            Func<int> func = client.GetMaxMfn;
            return Try(func);
        }

        public static DatabaseInfo[] ListDatabasesRetry(
            this IrbisClient client, string menuName)
        {
            Func<string, DatabaseInfo[]> func = client.ListDatabases;
            return Try(func, menuName);
        }

        public static TermInfo[] ListTermsRetry(this IrbisClient client,
            string start, int count)
        {
            Func<string, int, TermInfo[]> func = client.ListTerms;
            return Try(func, start, count);
        }

        public static MenuFile LoadMenuRetry(this IrbisClient client,
            string menuName)
        {
            Func<string, MenuFile> func = client.LoadMenu;
            return Try(func, menuName);
        }

        public static IniFile LoadIniFileRetry(this IrbisClient client,
            string fileName)
        {
            Func<string, IniFile> func = client.LoadIniFile;
            return Try(func, fileName);
        }

        public static SearchScenario[] LoadSearchScenarioRetry(
            this IrbisClient client, string fileName)
        {
            Func<string, SearchScenario[]> func = client.LoadSearchScenario;
            return Try(func, fileName);
        }

        public static void NopRetry(this IrbisClient client)
        {
            Try(client.Nop);
        }

        public static MarcRecord ReadRecordRetry(this IrbisClient client,
            int mfn)
        {
            Func<int, MarcRecord> func = client.ReadRecord;
            return Try(func, mfn);
        }

        public static string ReadTextFileRetry(this IrbisClient client,
            string specification)
        {
            Func<string, string> func = client.ReadTextFile;
            return Try(func, specification);
        }

        public static int[] SearchRetry(this IrbisClient client,
            string expression)
        {
            Func<string, int[]> func = client.Search;
            return Try(func, expression);
        }

        public static MarcRecord WriteRecordRetry(this IrbisClient client,
            MarcRecord record)
        {
            Func<MarcRecord, MarcRecord> func = client.WriteRecord;
            return Try(func, record);
        }
    }
}
