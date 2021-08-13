using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using E.Deezer;
using E.Deezer.Api;
using TagLib;

namespace Musics_Manage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string appUrlRedirect = Properties.Settings.Default.appDeezerRedirect;
        private string appId = Properties.Settings.Default.appDeezerId;
        private string appPwd = Properties.Settings.Default.appDeezerPwd;
        private string pathSavePlaylists = Properties.Settings.Default.pathSavePlaylists;
        private string pathLocalizeAllSongs = Properties.Settings.Default.pathLocalizeAllSongs;
        private string pathSaveFilesToDl = Properties.Settings.Default.pathSaveFilesToDl;

        private readonly Deezer session;
        private ulong currentPlaylistId;
        private int stepConnection = 0;
        private bool statusDeezer = false;

        private List<LocalTrackInformation> listLocalTracks;

        public MainWindow()
        {
            InitializeComponent();

            this.session = DeezerSession.CreateNew();

            txtBox_prm1.Text = Properties.Settings.Default.appDeezerId;
            txtBox_prm2.Text = Properties.Settings.Default.appDeezerPwd;
            txtBox_prm3.Text = Properties.Settings.Default.appDeezerRedirect;
            txtBox_prm4.Text = Properties.Settings.Default.pathSavePlaylists;
            txtBox_prm5.Text = Properties.Settings.Default.pathLocalizeAllSongs;
            txtBox_prm6.Text = Properties.Settings.Default.pathSaveFilesToDl;
        }

        private async void ConnectDeezer(int step)
        {
            string url1 = $"https://connect.deezer.com/oauth/auth.php?app_id={appId}&redirect_uri={appUrlRedirect}&perms=basic_access,manage_library,delete_library,offline_access,manage_community,listening_history,email";
            string url2 = $"https://connect.deezer.com/oauth/access_token.php?app_id={appId}&secret={appPwd}&code=";

            switch (step)
            {
                case 0:
                    LaunchBrowser(url1, "Veuillez entrer le code de votre site ici");
                    break;
                case 1:
                    url2 += txtBox_urlDeezer.Text;
                    LaunchBrowser(url2, "Veuillez entrer votre token ici");
                    break;
                case 2:
                    string token = txtBox_urlDeezer.Text;
                    txtBox_urlDeezer.Text = "Connexion en cours...";
                    try
                    {
                        await this.session.Login(token);
                        txtBox_urlDeezer.Text = "Session deezer connectée !";
                        txtBox_urlDeezer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#660CF101"));
                        statusDeezer = true;
                        btn_connectDeezer.IsEnabled = false;
                        txt_connectDeezer.Text = "";
                    }
                    catch
                    {
                        txtBox_urlDeezer.Text = "Impossible de se connecter à Deezer !";
                        txtBox_urlDeezer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66FF0000"));
                        statusDeezer = false;
                    }
                    stepConnection = 0;
                    break;
            }
        }

        private void LaunchBrowser(string url, string message)
        {
            try
            {
                var ps = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                    Verb = "open",
                };
                Process.Start(ps);
                txtBox_urlDeezer.Text = string.Empty;
                this.Resources["txt_connectionDeeezer"] = message;
                stepConnection++;
            }
            catch
            {
                stepConnection = 0;
                throw new Exception($"La connexion à l'adresse : \"{url}\" est impossible");
            }
        }


        private void Search()
        {
            grid_searchResults.Children.Clear();

            IEnumerable<IPlaylist> playlists = session.Search.Playlists(txtBox_Search.Text, 0, 16).Result;
            List<IPlaylist> listResults = playlists.ToList();

            int row = 0;
            int column = 0;

            for (int i = 0; i < listResults.Count; i++)
            {
                UC_ResultTemplate resultTemplate = new UC_ResultTemplate(session, listResults[i].Id);
                resultTemplate.VerticalAlignment = VerticalAlignment.Center;
                resultTemplate.HorizontalAlignment = HorizontalAlignment.Center;
                resultTemplate.MouseDown += (sender, e) => this.Playlist_Click(resultTemplate);
                Grid.SetColumn(resultTemplate, column);
                Grid.SetRow(resultTemplate, row);
                grid_searchResults.Children.Add(resultTemplate);

                column++;
                if (column > 3)
                {
                    column = 0;
                    row++;
                }
            }
        }

        private void Playlist_Click(UC_ResultTemplate resultTemplate)
        {
            list_Tracks.Items.Clear();

            IEnumerable<ITrack> tracks = resultTemplate.GetTracks();

            foreach (ITrack track in tracks)
            {
                UC_Tracklist tracklist = new UC_Tracklist(track.Title, track.ArtistName, track.Duration, track.GetPicture(PictureSize.Small));
                list_Tracks.Items.Add(tracklist);
            }

            FillPlaylistInfos(resultTemplate.playlist);
        }

        private void FillPlaylistInfos(IPlaylist playlist)
        {
            txt_titlePlaylist.Text = "Titre de la playlist : " + playlist.Title;
            TimeSpan time = TimeSpan.FromSeconds(playlist.Duration);
            string str = time.ToString(@"mm\'ss");
            txt_dureePlaylist.Text = "Durée : " + str;
            txt_creatorPlaylist.Text = "Créateur : " + playlist.CreatorName;
            txt_tracksPlaylist.Text = "Nombre de tracks : " + playlist.TrackCount;
            currentPlaylistId = playlist.Id;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (((System.Windows.Controls.TextBox)sender).Name == "txtBox_Search")
                {
                    Search();
                }
                else if (((System.Windows.Controls.TextBox)sender).Name == "txtBox_urlDeezer")
                {
                    ConnectDeezer(stepConnection);
                }
            }
        }

        private async void btn_analyse_Click(object sender, RoutedEventArgs e)
        {
            txt_progressBar.Text = "Analyse des fichiers...";
            progressBar.Value = 0;

            string[] files = Directory.GetFiles(pathLocalizeAllSongs, "*.mp3", SearchOption.AllDirectories);
            progressBar.Maximum = files.Length;

            List<string> listAnalyseFiles = new List<string>();
            List<string> listErrorFiles = new List<string>();
            listLocalTracks = new List<LocalTrackInformation>();

            foreach (string file in files)
            {
                try
                {
                    LocalTrackInformation trackInfo = new LocalTrackInformation(InitType.FromAnalyse, file);
                    listLocalTracks.Add(trackInfo);
                    listAnalyseFiles.Add(trackInfo.GetLine());
                    progressBar.Value++;
                    txt_progressBar.Text = "Fichier " + progressBar.Value + "/" + files.Length + " analysé";
                }
                catch
                {
                    listErrorFiles.Add(file);
                }
                await Task.Delay(1);
            }

            FileWriter analyseFiles = new FileWriter(Environment.CurrentDirectory + @"\localTracks.json");
            analyseFiles.WriteFile(listAnalyseFiles, true);
            FileWriter errorFile = new FileWriter(Environment.CurrentDirectory + @"\errorfiles.json");
            errorFile.WriteFile(listErrorFiles, true);
        }

        private async void btn_createPlaylist_Click(object sender, RoutedEventArgs e)
        {
            txt_progressBar.Text = "Création de la playlist...";
            progressBar.Value = 0;

            IPlaylist playlist = this.session.Browse.GetPlaylistById(currentPlaylistId).Result;
            IEnumerable<ITrack> tracks = playlist.GetTracks().Result;
            string[] files = System.IO.File.ReadAllLines(Environment.CurrentDirectory + @"\localTracks.json");
            listLocalTracks = new List<LocalTrackInformation>();
            foreach (string line in files)
            {
                LocalTrackInformation localTrack = new LocalTrackInformation(InitType.FromFileList, line);
                listLocalTracks.Add(localTrack);
            }

            List<string> listM3U = new List<string>();
            List<string> listDl = new List<string>();
            HttpClient client = new HttpClient();

            progressBar.Maximum = playlist.TrackCount;

            foreach (ITrack track in tracks)
            {
                string link = "http://api.deezer.com/track/" + track.Id.ToString();
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                string apiResponse = await response.Content.ReadAsStringAsync();
                string isrcSource = ReturnIsrc(apiResponse);
                bool state = true;
                for (int i = 0; i < listLocalTracks.Count; i++)
                {
                    if (listLocalTracks[i].GetIsrc() == isrcSource)
                    {
                        listM3U.Add($"#EXTINF:{listLocalTracks[i].GetDuration()},{listLocalTracks[i].GetArtists()[0]} - {listLocalTracks[i].GetTitle()}");
                        listM3U.Add(listLocalTracks[i].GetPath());
                        state = true;
                        break;
                    }
                    else
                    {
                        state = false;
                    }
                }

                if (!state)
                {
                    listDl.Add($"{track.Title} - {track.Artist.Name}");
                    if (statusDeezer)
                    {
                        await this.session.User.AddTrackToFavourite(track);
                    }
                }

                progressBar.Value++;
                txt_progressBar.Text = "Fichier " + progressBar.Value + "/" + playlist.TrackCount;
                await Task.Delay(25);
            }

            listM3U.Insert(0, $"#EXTM3U");
            listM3U.Insert(1, $"#Title : {playlist.Title}");
            listM3U.Insert(2, $"#Description : {playlist.Description}");
            listM3U.Insert(3, $"#Total duration : {TimeSpan.FromSeconds(Double.Parse(playlist.Duration.ToString())).ToString(@"hh\:mm\:ss")}");
            listM3U.Insert(4, $"#Total tracks : {playlist.TrackCount}");
            listM3U.Insert(5, $"#Playlist link : {playlist.Link}");
            listM3U.Insert(6, $"#Created by : {playlist.CreatorName} / Id {playlist.Creator.Id}");

            //Création du fichier m3u
            txt_progressBar.Text = "Création du fichier m3u";
            string m3uFilePath = pathSavePlaylists + @"\" + playlist.Title + @".m3u";
            FileWriter m3uFile = new FileWriter(m3uFilePath);
            m3uFile.WriteFile(listM3U, true);

            //Création du fichier à télécharger
            txt_progressBar.Text = "Création du fichier a dl";
            string dlFilePath = pathSaveFilesToDl + @"\" + "Musiques à télécharger.txt";
            FileWriter dlFile = new FileWriter(dlFilePath);
            m3uFile.WriteFile(listDl, false);

            txt_progressBar.Text = "Création de la playlist ok !";
        }

        private string ReturnIsrc(string text)
        {
            string[] split1 = text.Split("\"isrc\":\"", StringSplitOptions.None);
            return split1[1].Split("\"", StringSplitOptions.None)[0];
        }

        private void btn_connectDeezer_Click(object sender, RoutedEventArgs e)
        {
            ConnectDeezer(stepConnection);
        }

        private void btn_parameters_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.appDeezerId = txtBox_prm1.Text;
            Properties.Settings.Default.appDeezerPwd = txtBox_prm2.Text;
            Properties.Settings.Default.appDeezerRedirect = txtBox_prm3.Text;
            Properties.Settings.Default.pathSavePlaylists = txtBox_prm4.Text;
            Properties.Settings.Default.pathLocalizeAllSongs = txtBox_prm5.Text;
            Properties.Settings.Default.pathSaveFilesToDl = txtBox_prm6.Text;
            appId = Properties.Settings.Default.appDeezerId;
            appPwd = Properties.Settings.Default.appDeezerPwd;
            appUrlRedirect = Properties.Settings.Default.appDeezerRedirect;
            pathSavePlaylists = Properties.Settings.Default.pathSavePlaylists;
            pathLocalizeAllSongs = Properties.Settings.Default.pathLocalizeAllSongs;
            pathSaveFilesToDl = Properties.Settings.Default.pathSaveFilesToDl;
            Properties.Settings.Default.Save();
        }

        private void btn_Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.session.Logout();
            this.session.Dispose();
            this.Close();
        }
    }
}
