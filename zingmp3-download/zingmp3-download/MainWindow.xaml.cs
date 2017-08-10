using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using xNet;

namespace zingmp3_download
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool RedirectStandardOutput { get; private set; }
        public bool UseShellExecute { get; private set; }
        public bool CreateNoWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Event

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (tbxInputLink.Text == "")
            {
                MessageBox.Show("Ban can nhap link muon tai ");
            }
            string inputLink = tbxInputLink.Text;
            bool checkidm = CheckIDM();
            DownloadMp3(inputLink, checkidm);
        }

        #endregion Event

        #region Method

        private void DownloadMp3(string inputLink, bool checkidm)
        {
            HttpRequest http = new HttpRequest();
            string htmlSong = http.Get(inputLink).ToString();
            //string regular = @"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""(.*?)""";
            //string getJson = @"http://mp3.zing.vn" + Regex.Match(htmlSong, regular, RegexOptions.Singleline).Value.Replace(@"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""", "").Replace("\"", "");
            string getJsonURL = Regex.Match(htmlSong, @"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""(.*?)""", RegexOptions.Singleline).Value.Replace(@"<div id=""zplayerjs-wrapper"" class=""player"" data-xml=""", "").Replace("\"", "");
            string jsonInfo = http.Get(@"http://mp3.zing.vn" + getJsonURL).ToString();

            JObject jObject = JObject.Parse(jsonInfo);
            string downloadURL = jObject["data"][0]["source_list"].ToString();
            downloadURL = downloadURL.Substring(downloadURL.IndexOf("http"), downloadURL.IndexOf(",") - downloadURL.IndexOf("http") - 1);
            string songname = jObject["data"][0]["name"].ToString();
            string artist = jObject["data"][0]["artist"].ToString();
            string full_name = songname + "_" + artist + ".mp3";

            full_name = RemoveUnicode(full_name).Replace(" ", "-");

            try
            {
                if (checkidm == true)
                {
                    DownloadFromIDM(downloadURL, full_name);
                }
                else
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(downloadURL, AppDomain.CurrentDomain.BaseDirectory + full_name);
                    MessageBox.Show("Download thanh cong!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DownloadFromIDM(string link, string full_name)
        {
            Process.Start("C:\\Program Files (x86)\\Internet Download Manager\\IDMan.exe", "/n /d " + link + " /f " + full_name + "");
        }

        private bool CheckIDM()
        {
            if (File.Exists("C:\\Program Files (x86)\\Internet Download Manager\\IDMan.exe"))
                return true;
            return false;
        }

        private static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                                            "đ",
                                            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                                            "í","ì","ỉ","ĩ","ị",
                                            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                                            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                                            "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                                            "d",
                                            "e","e","e","e","e","e","e","e","e","e","e",
                                            "i","i","i","i","i",
                                            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                                            "u","u","u","u","u","u","u","u","u","u","u",
                                            "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }

        #endregion Method

        private void tbxInputLink_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            tbxInputLink.Text = "";
        }
    }
}