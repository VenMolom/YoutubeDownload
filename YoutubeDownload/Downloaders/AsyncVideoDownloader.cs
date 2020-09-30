using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownload.Option;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownload.Downloaders
{
    internal class AsyncVideoDownloader : AsyncDownloader
    {
        private readonly FormatOptions format;

        public AsyncVideoDownloader(FileNameOptions fileName, FormatOptions format)
            : base(fileName)
        {
            this.format = format;
        }

        private async Task DownloadAudioVideoAsync(string id, Func<StreamManifest, IStreamInfo> GetStreamInfo)
        {
            try
            {
                var videoMeta = await client.Videos.GetAsync(id);

                var file = HandleName(fileName, videoMeta.Title, videoMeta.Id);

                var streamManifest = await client.Videos.Streams.GetManifestAsync(id);
                var streamInfo = GetStreamInfo(streamManifest);
                if (streamInfo != null)
                {
                    await client.Videos.Streams.DownloadAsync(streamInfo, $"{file}.{streamInfo.Container.Name}");
                    Console.WriteLine($"Downloaded {videoMeta.Title}");
                }
                else
                {
                    Console.Error.WriteLine("Cannot download");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task DownloadMuxedAsync(string id)
        {
            try
            {
                var videoMeta = await client.Videos.GetAsync(id);

                var file = HandleName(fileName, videoMeta.Title, videoMeta.Id);

                var streamManifest = await client.Videos.Streams.GetManifestAsync(id);
                var audioStreamInfo = GetAudioStreamInfo(streamManifest);
                var videoStreamInfo = GetVideoStreamInfo(streamManifest);
                if (audioStreamInfo != null && videoStreamInfo != null)
                {
                    await DownloadMuxedFromStreamsAsync(videoStreamInfo, audioStreamInfo, file);
                    Console.WriteLine($"Downloaded {videoMeta.Title}");
                }
                else
                {
                    Console.Error.WriteLine("Cannot download");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task DownloadMuxedFromStreamsAsync(IVideoStreamInfo videoStreamInfo, IAudioStreamInfo audioStreamInfo, string file)
        {
            var videoStream = await client.Videos.Streams.GetAsync(videoStreamInfo);
            var audioStream = await client.Videos.Streams.GetAsync(audioStreamInfo);

            var videoPipeName = "ffvideopipe";
            var audioPipeName = "ffaudiopipe";
            var args = $@"-y -i \\.\pipe\{videoPipeName} -i \\.\pipe\{audioPipeName} -c copy -map 0:v:0 -map 1:a:0 {file}.{videoStreamInfo.Container.Name}";

            var videoPipe = new NamedPipeServerStream(videoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 10000, 10000);
            var audioPipe = new NamedPipeServerStream(audioPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 10000, 10000);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            process.Start();

            videoPipe.WaitForConnection();
            var videoCopy = videoStream.CopyToAsync(videoPipe);

            audioPipe.WaitForConnection();
            var audioCopy = audioStream.CopyToAsync(audioPipe);

            Task.WaitAll(videoCopy, audioCopy);
            videoPipe.Flush();
            audioPipe.Flush();
            videoPipe.Dispose();
            audioPipe.Dispose();

            process.WaitForExit();
        }

        private IAudioStreamInfo GetAudioStreamInfo(StreamManifest manifest) =>
                                    manifest.GetAudioOnly().OrderByDescending(streamInfo => streamInfo.Bitrate).First();

        private IVideoStreamInfo GetVideoStreamInfo(StreamManifest manifest) =>
            manifest.GetVideoOnly().OrderByDescending(streamInfo => streamInfo.Bitrate).First();

        public override async Task DownloadAsync(string id)
        {
            switch (format)
            {
                case FormatOptions.Audio:
                    await DownloadAudioVideoAsync(id, GetAudioStreamInfo);
                    break;

                case FormatOptions.Video:
                    await DownloadAudioVideoAsync(id, GetVideoStreamInfo);
                    break;

                case FormatOptions.Muxed:
                    await DownloadMuxedAsync(id);
                    break;
            }
        }
    }
}