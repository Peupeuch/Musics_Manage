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

namespace Musics_Manage
{
    /// <summary>
    /// Logique d'interaction pour UC_Tracklist.xaml
    /// </summary>
    public partial class UC_Tracklist : UserControl
    {
        public UC_Tracklist(string _title, string _artist, uint _duration, string _coverLink)
        {
            InitializeComponent();

            rect_Cover.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(_coverLink))
            };
            txt_Title.Text = _title;
            txt_Artist.Text = _artist;

            TimeSpan time = TimeSpan.FromSeconds(_duration);
            string str = time.ToString(@"mm\:ss");
            txt_Duration.Text = str;
        }
    }
}
