using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace async_read_file
{
    class Program
    {
        static readonly int _bufferSize = 8192; // get a larger buffer size to increase performance

        static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            var fullFileName = "";
            int linesNumbers = 0;

            for (int i = 0; i < 11; i++)
            {
                sw.Restart();
                linesNumbers = GetLinesCount(fullFileName);
                sw.Stop();
                WriteReport(nameof(GetLinesCount), linesNumbers, sw.ElapsedMilliseconds);

                sw.Restart();
                linesNumbers = await GetLinesCountAsync(fullFileName);
                sw.Stop();
                WriteReport(nameof(GetLinesCountAsync), linesNumbers, sw.ElapsedMilliseconds);

                sw.Restart();
                linesNumbers = await GetLinesCountViaReadLineAsync(fullFileName);
                sw.Stop();
                WriteReport(nameof(GetLinesCountViaReadLineAsync), linesNumbers, sw.ElapsedMilliseconds);

                sw.Restart();
                linesNumbers = await GetLinesCountViaReadToEndAsync(fullFileName);
                sw.Stop();
                WriteReport(nameof(GetLinesCountViaReadToEndAsync), linesNumbers, sw.ElapsedMilliseconds);

                sw.Restart();
                linesNumbers = await GetLinesCountViaReadAllLinesAsync(fullFileName);
                sw.Stop();
                WriteReport(nameof(GetLinesCountViaReadAllLinesAsync), linesNumbers, sw.ElapsedMilliseconds);

                Console.WriteLine();
            }

            Console.WriteLine("Completed");
            Console.ReadLine();
        }

        private static void WriteReport(string methodName, int linesNumbers, long elapsedMilliseconds)
        {
            Console.WriteLine($"{methodName}, lines: {linesNumbers}, elapsed {elapsedMilliseconds} ms");
        }

        private static int GetLinesCount(string fullFileName)
        {
            using StreamReader reader = new(fullFileName);
            int lineCounter = 0;
            while (reader.ReadLine() != null)
            {
                lineCounter++;
            }
            return lineCounter;
        }

        static async Task<int> GetLinesCountAsync(string filePath)
        {
            int lineCount = 0;
            using (StreamReader reader = new StreamReader(new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: _bufferSize, useAsync: true)))
            {
                while (await reader.ReadLineAsync().ConfigureAwait(false) != null)
                    lineCount++;
            }
            return lineCount;
        }

        static async Task<int> GetLinesCountViaReadLineAsync(string filePath)
        {
            int lineCount = 0;
            FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            using (StreamReader reader = new StreamReader(new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: _bufferSize, DefaultOptions)))
            {
                while (await reader.ReadLineAsync().ConfigureAwait(false) != null)
                    lineCount++;
            }
            return lineCount;
        }

        static async Task<int> GetLinesCountViaReadToEndAsync(string filePath)
        {
            FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            using (StreamReader reader = new StreamReader(new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: _bufferSize, DefaultOptions)))
            {
                var fileText = await reader.ReadToEndAsync().ConfigureAwait(false);
                return fileText.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None).Length;
            }
        }

        static async Task<int> GetLinesCountViaReadAllLinesAsync(string filePath)
        {
            string[] lines = await
                File.ReadAllLinesAsync(filePath).ConfigureAwait(false);
            int lineCount = lines.Length;
            return lineCount;
        }
    }
}
