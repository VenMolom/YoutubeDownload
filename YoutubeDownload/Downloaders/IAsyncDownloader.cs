using System.Threading.Tasks;

namespace YoutubeDownload.Downloaders
{
    internal interface IAsyncDownloader
    {
        Task DownloadAsync(string id);
    }
}