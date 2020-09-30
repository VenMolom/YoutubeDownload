using YoutubeDownload.Option;

namespace YoutubeDownload.Downloaders
{
    internal static class AsyncDownloaderFactory
    {
        public static IAsyncDownloader CreateDownloader(bool playlist, FileNameOptions fileName, FormatOptions format)
        {
            var downloader = new AsyncVideoDownloader(fileName, format);
            if (playlist)
            {
                return new AsyncPlaylistDownloader(fileName, downloader);
            }
            else
            {
                return downloader;
            }
        }
    }
}