using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using xNet;

namespace ZingDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool checkidm = false;
        public static string idmpath = "C:\\Program Files (x86)\\Internet Download Manager\\IDMan.exe";

        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        #region Event

        private void BtnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            if (txbInputLink.Text == "")
            {
                MessageBox.Show("Ban can nhap vao duong dan audio!");
            }
            else
            {
                CreateFolder();
                string item = CheckInputLink(txbInputLink.Text);
                switch (item)
                {
                    case "video":
                        {
                            MessageBox.Show("Chuc nang chua hoan thanh!");
                            break;
                        }

                    case "audio":
                        {
                            GetASongInfomation(txbInputLink.Text);
                            break;
                        }

                    case "playlist":
                        {
                            GetPlaylistInformation(txbInputLink.Text);
                            break;
                        }
                }
            }
            
        }

        private void TxbInputLink_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txbInputLink.Text = "";
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                txbInputLink.Text = Clipboard.GetText(TextDataFormat.Text);
            }
        }

        #endregion Event

        #region Method

        private void Load()
        {
            checkidm = CheckIDM();
        }

        private void GetASongInfomation(string URL)
        {
            HttpRequest httpRequest = new HttpRequest();
            string htmlSong = httpRequest.Get(URL).ToString();
            string getURLJsonFromHtmlSong = Regex.Match(htmlSong, @"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""(.*?)""", RegexOptions.Singleline).Value;
            string URLJson = "http://mp3.zing.vn" + getURLJsonFromHtmlSong.Replace(@"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""", "").Replace("\"", "");
            string getJsonFromURLJson = httpRequest.Get(URLJson).ToString();

            JObject jobject = JObject.Parse(getJsonFromURLJson);
            string songname = jobject["data"][0]["name"].ToString().Replace(" ", "_");
            string artist = jobject["data"][0]["artist"].ToString().Replace(" ", "_");
            string audiolink = jobject["data"][0]["source_list"].ToString();
            audiolink = audiolink.Substring(audiolink.IndexOf("http"), audiolink.IndexOf(",") - audiolink.IndexOf("http") - 1);
            string fullname = ReplaceUnicode(songname) + "--" + ReplaceUnicode(artist) + ".mp3";

            try
            {
                if (checkidm == true)
                {
                    DownloadWithIDM(audiolink, fullname, "song");
                }
                else
                {
                    DownloadWithoutIDM(audiolink, fullname);
                    MessageBox.Show("Download thanh cong!");
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void GetPlaylistInformation(string URL)
        {
            HttpRequest httpRequest = new HttpRequest();
            string htmlPlaylist = httpRequest.Get(URL).ToString();
            string pattern = "<div id=\"zplayerjs-wrapper\" class=\"player \" data-xml=\"";
            int index = htmlPlaylist.IndexOf(pattern);
            string URLJson = htmlPlaylist.Substring(index, 131).Replace(pattern, "");

            string getJsonFromURLJson = httpRequest.Get(URLJson).ToString();

            int countSong = new Regex(Regex.Escape("name")).Matches(getJsonFromURLJson).Count;

            JObject jobject = JObject.Parse(getJsonFromURLJson);
            string songname = "";
            string artist = "";
            string audiolink = "";
            string fullname = "";
            for (int i = 0; i < countSong; i++)
            {
                songname = jobject["data"][i]["name"].ToString().Replace(" ", "_");
                artist = jobject["data"][i]["artist"].ToString().Replace(" ", "_");
                audiolink = jobject["data"][i]["source_list"].ToString();
                audiolink = audiolink.Substring(audiolink.IndexOf("http"), audiolink.IndexOf(",") - audiolink.IndexOf("http") - 1);
                fullname = ReplaceUnicode(songname) + "--" + ReplaceUnicode(artist) + ".mp3";
                try
                {
                    if (checkidm == true)
                    {
                        DownloadWithIDM(audiolink, fullname, "playlist");
                        Process.Start(idmpath, "/s");
                    }
                    else
                    {
                        DownloadWithoutIDM(audiolink, fullname);
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.ToString());
                }
            }
        }

        private string ReplaceUnicode(string text)
        {
            string[] array1 = new string[]
            {
                "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",
            };

            string[] array2 = new string[]
            {
                "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",
            };
            for (int i = 0; i < array1.Length; i++)
            {
                text = text.Replace(array1[i], array2[i]);
                text = text.Replace(array1[i].ToUpper(), array2[i].ToUpper());
            }
            return text;
        }

        private void DownloadWithIDM(string URL, string fullname, string choose)
        {
            switch (choose)
            {
                case "playlist":
                    {
                        try
                        {
                            //Process.Start(idmpath, "/n /a /s /d " + URL + " /f " + fullname + "");
                            Process.Start(idmpath, "/n /a /s /d " + URL + " /f " + fullname + " /p "+ AppDomain.CurrentDomain.BaseDirectory + "\\Song" + "");
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Download khong thanh cong!");
                        }
                        break;
                    }
                case "song":
                    {
                        try
                        {
                            //Process.Start(idmpath, "/n /d " + URL + " /f " + fullname + "");
                            Process.Start(idmpath, "/n /d " + URL + " /f " + fullname + " /p " + AppDomain.CurrentDomain.BaseDirectory + "\\Song" + "");
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Download khong thanh cong!");
                        }
                        break;
                    }
            }
        }

        private void DownloadWithoutIDM(string URL, string fullname)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(URL, AppDomain.CurrentDomain.BaseDirectory + "\\Song" + fullname);
            }
            catch (Exception)
            {
                MessageBox.Show("Download khong thanh cong!");
            }
        }

        private bool CheckIDM()
        {
            if (File.Exists("C:\\Program Files (x86)\\Internet Download Manager\\IDMan.exe"))
                return true;
            return false;
        }

        private string CheckInputLink(string inputLink)
        {
            if (inputLink.IndexOf("bai-hat") > 0)
                return "audio";
            if (inputLink.IndexOf("video-clip") > 0)
                return "video";
            if (inputLink.IndexOf("album") > 0)
                return "playlist";
            return "khong-ton-tai";
        }

        private bool CreateFolder()
        {
            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory+"\\Song");
            return true;
        }

        #endregion Method
    }
}