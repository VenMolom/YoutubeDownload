using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeDownload.Option;
using YoutubeExplode;

namespace YoutubeDownload.Downloaders
{
    internal abstract class AsyncDownloader : IAsyncDownloader
    {
        protected readonly YoutubeClient client = new YoutubeClient();
        protected readonly FileNameOptions fileName;

        protected AsyncDownloader(FileNameOptions fileName)
        {
            this.fileName = fileName;
        }

        protected static string HandleName(FileNameOptions fileNameOption, string title, string id)
        {
            if (fileNameOption == FileNameOptions.Title)
            {
                return Regex.Replace(title, @"[<>:""/\|?*]", "_");
            }
            else
            {
                return id;
            }
        }

        public abstract Task DownloadAsync(string id);
    }
}