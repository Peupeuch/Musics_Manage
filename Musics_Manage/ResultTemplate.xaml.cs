using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using E.Deezer;
using E.Deezer.Api;

namespace Musics_Manage
{
    /// <summary>
    /// Logique d'interaction pour ResultTemplate.xaml
    /// </summary>
    public partial class ResultTemplate : UserControl
    {
        private readonly Deezer session;
        public IPlaylist playlist;

        public ResultTemplate(Deezer _session, ulong _id)
        {
            InitializeComponent();

            this.session = _session;

            SetPlaylist(_id);
        }

        private void SetPlaylist(ulong _id)
        {
            playlist = this.session.Browse.GetPlaylistById(_id).Result;

            SetCover(playlist.GetPicture(PictureSize.Medium));
            SetTitle(playlist.Title);
            SetNumberTracks(playlist.TrackCount);
        }

        private void SetCover(string _imageLink)
        {
            rectangle.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(_imageLink))
            };
        }

        private void SetTitle(string _title)
        {
            text.Text = _title;
        }

        private void SetNumberTracks(uint _numberTracks)
        {
            nbChansons.Text = _numberTracks.ToString() + " ♪";
        }

        public IEnumerable<ITrack> GetTracks()
        {
            return playlist.GetTracks().Result;
        }
    }
}
