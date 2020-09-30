using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownload.Downloaders;
using YoutubeDownload.Option;

namespace YoutubeDownload
{
    internal class Program
    {
        private static void DisplayHelp<T>(ParserResult<T> result)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AddEnumValuesToHelpText = true;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.Error.WriteLine(helpText);
        }

        private static void Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.HelpWriter = null;
            });
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed(options => ManageDownloads(options))
                .WithNotParsed(errs => DisplayHelp(parserResult));
        }

        private static void ManageDownloads(Options options)
        {
            var requests = new List<Task>(options.IDs.Count());
            IAsyncDownloader downloader = AsyncDownloaderFactory.CreateDownloader(options.Playlist, options.FileName, options.Format);
            foreach (var id in options.IDs)
            {
                requests.Add(downloader.DownloadAsync(id));
            }
            Task.WaitAll(requests.ToArray());
        }
    }
}