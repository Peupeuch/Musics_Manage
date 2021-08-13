using System;
using System.Collections.Generic;
using System.Text;
using TagLib;

namespace Musics_Manage
{
    public enum InitType
    {
        FromAnalyse,
        FromFileList
    }

    class LocalTrackInformation
    {
        private string path, title, album, isrc, duration;
        private string[] artists;
        private TagLib.File fileTag;

        public LocalTrackInformation(InitType init, string parameter)
        {
            switch (init)
            {
                case InitType.FromAnalyse:
                    InitFromAnalyse(parameter);
                    break;
                case InitType.FromFileList:
                    InitFromFileList(parameter);
                    break;
            }
        }

        private void InitFromFileList(string _line)
        {
            isrc = _line.Split('[', ']')[1];
            title = _line.Split('[', ']')[3];
            artists = _line.Split('[', ']')[5].Split(',');
            album = _line.Split('[', ']')[7];
            duration = _line.Split('[', ']')[9];
            path = _line.Split('[', ']')[11];
        }

        private void InitFromAnalyse(string _path)
        {
            path = _path;
            fileTag = TagLib.File.Create(path);
            title = fileTag.Tag.Title;
            artists = fileTag.Tag.Performers;
            album = fileTag.Tag.Album;
            duration = TimeSpan.FromSeconds(fileTag.Properties.Duration.TotalSeconds).ToString(@"mm\'ss");
            isrc = fileTag.Tag.ISRC;
        }

        public string GetTitle()
            => title;

        public string[] GetArtists()
            => artists;

        public string GetAlbum()
            => album;

        public string GetDuration()
            => duration;

        public string GetIsrc()
            => isrc;

        public string GetPath()
            => path;

        public string GetLine()
            => $"ISRC[{isrc}]/TITLE[{title}]/ARTISTS[{string.Join(',', artists)}]/ALBUM[{album}]/DUREE[{duration}]/PATH[{path}]";

    }
}