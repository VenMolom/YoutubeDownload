using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeDownload.Option;

namespace YoutubeDownload.Downloaders
{
    internal class AsyncPlaylistDownloader : AsyncDownloader
    {
        private readonly IAsyncDownloader downloader;

        public AsyncPlaylistDownloader(FileNameOptions fileName, IAsyncDownloader downloader)
            : base(fileName)
        {
            this.downloader = downloader;
        }

        public override async Task DownloadAsync(string id)
        {
            try
            {
                var playlist = await client.Playlists.GetAsync(id);

                var dirName = HandleName(fileName, playlist.Title, playlist.Id);

                Directory.CreateDirectory(dirName);
                var currentDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(dirName);

                var videosRequests = new List<Task>();
                await foreach (var video in client.Playlists.GetVideosAsync(id))
                {
                    videosRequests.Add(downloader.DownloadAsync(video.Id));
                }
                Task.WaitAll(videosRequests.ToArray());

                Console.WriteLine($"Downloaded playlist {playlist.Title}");
                Directory.SetCurrentDirectory(currentDir);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}