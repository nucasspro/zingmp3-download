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
        private static bool checkidm = false;

        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        #region Event

        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            if (txbInputLink.Text == "")
            {
                MessageBox.Show("Ban can nhap vao duong dan audio!");
            }
            else
            {
                GetSongInfomation(txbInputLink.Text);
            }
        }

        private void txbInputLink_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txbInputLink.Text = "";
        }

        #endregion Event

        #region Method

        private void Load()
        {
            checkidm = CheckIDM();
        }

        private void GetSongInfomation(string URL)
        {
            HttpRequest httpRequest = new HttpRequest();
            string htmlSong = httpRequest.Get(URL).ToString();
            string getURLJsonFromHtmlSong = Regex.Match(htmlSong, @"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""(.*?)""", RegexOptions.Singleline).Value;
            string URLJson = "http://mp3.zing.vn" + getURLJsonFromHtmlSong.Replace(@"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""", "").Replace("\"", "");
            string getJsonFromURLJson = httpRequest.Get(URLJson).ToString();

            JObject jobject = JObject.Parse(getJsonFromURLJson);
            string songname = jobject["data"][0]["name"].ToString().Replace(" ", "-");
            string artist = jobject["data"][0]["artist"].ToString().Replace(" ", "-");
            string audiolink = jobject["data"][0]["source_list"].ToString();
            audiolink = audiolink.Substring(audiolink.IndexOf("http"), audiolink.IndexOf(",") - audiolink.IndexOf("http") - 1);
            string fullname = ReplaceUnicode(songname) + "_" + ReplaceUnicode(artist) + ".mp3";

            try
            {
                if (checkidm == true)
                {
                    DownloadWithIDM(audiolink, fullname);
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

        private void DownloadWithIDM(string URL, string fullname)
        {
            try
            {
                Process.Start("C:\\Program Files (x86)\\Internet Download Manager\\IDMan.exe", "/n /d " + URL + " /f " + fullname + "");
            }
            catch (Exception)
            {
                MessageBox.Show("Download khong thanh cong!");
            }
        }

        private void DownloadWithoutIDM(string URL, string fullname)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(URL, AppDomain.CurrentDomain.BaseDirectory + fullname);
                MessageBox.Show("Download thanh cong!");
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

        #endregion Method
    }
}