using CommandLine;
using System.Collections.Generic;

namespace YoutubeDownload.Option
{
    internal class Options
    {
        [Option(Default = FileNameOptions.Title, HelpText = "Specify how files should be titled")]
        public FileNameOptions FileName { get; set; }

        [Option(Default = FormatOptions.Audio, HelpText = "Output file format")]
        public FormatOptions Format { get; set; }

        [Value(0, HelpText = "IDs or URLs of videos", Min = 1, MetaName = "IDs")]
        public IEnumerable<string> IDs { get; set; }

        [Option('p', Default = false, HelpText = "Playlist download switch")]
        public bool Playlist { get; set; }
    }
}