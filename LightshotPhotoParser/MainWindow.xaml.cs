using AngleSharp.Parser.Html;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightshotPhotoParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string Path = "";
        private string path = ""; 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WPFFolderBrowser.WPFFolderBrowserDialog openFileDialog = new WPFFolderBrowser.WPFFolderBrowserDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Path = openFileDialog.FileName;
                PathFileDirectory.Content = openFileDialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(Path == "")
            {
                MessageBox.Show("Put path to folder");
                return;
            }
            int count = 0;
            try
            {
                count = int.Parse(Count_Items.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("You put`s wrong value");
                return;
            }
            var countOfTrying = 0 ;
                for (int i = 0; i < count;)
                {
                    Trying.Content = "Count : " + countOfTrying;
                    Task<bool>[] tasks = CreateTaskArray(10, IsDownloadJPG);
                    foreach (var t in tasks)
                        t.Start();
                    Task.WaitAll(tasks);
                    foreach (var task in tasks)
                    {
                        if (task.Result == true)
                        {
                            i++;
                        }
                    }
                    countOfTrying += 10;
                    IsDownloadJPG();
                }
            MessageBox.Show("Photo have been downloaded succes.");
            
        }
        private Task<bool>[] CreateTaskArray(int size,Func<bool> func)
        {
            Task<bool>[] tasks = new Task<bool>[size];
            for (int i =0; i<size;i++){
                tasks[i] = new Task<bool>(func);
            }
            return tasks;
        }


        private string CreateURL()
        {
            Random rand = new Random();
            StringBuilder url = new StringBuilder("https://prnt.sc/"); 
            for (int j = 0; j <= 5; j++)
            {
                int num = rand.Next(0, 25);
                char let = (char)('a' + num);
                url.Append(let);
            } 
            return url.ToString();
        }
        private bool IsDownloadJPG()
        {
            if (this.DownloadJPG(CreateURL()))
            {
                return true;
            }
            else { return false; }
        }
        private bool DownloadJPG(string url)
        { 
            try
            {

                HttpWebRequest logIn6 = (HttpWebRequest)WebRequest.Create(new Uri(url));
                logIn6.CookieContainer = new CookieContainer();
                logIn6.KeepAlive = true;
                logIn6.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.220 Safari/535.1";
                
                logIn6.AllowAutoRedirect = false;
                string html = "";
                using (HttpWebResponse logIn6Response = (HttpWebResponse)logIn6.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(logIn6Response.GetResponseStream()))
                    {
                        html = reader.ReadToEnd();

                    }
                } 
                
                    var parser = new HtmlParser();
                    var document = parser.Parse(html);
                    var element =  document.GetElementById("screenshot-image");
                if (element.GetAttribute("src") != "//st.prntscr.com/2018/10/13/2048/img/0_173a7b_211be8ff.png")
                {
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(new Uri(url));
                    wr.CookieContainer = new CookieContainer();
                    wr.KeepAlive = true;
                    wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.220 Safari/535.1";
                    string src = element.GetAttribute("src");
                    string[] fileName = src.Split('/');
                    string path = System.IO.Path.Combine(Path + "/" + fileName[fileName.Length - 1]);
                    if (!Directory.GetFiles(Path).Contains(fileName[fileName.Length - 1]))
                    {

                        wr.AllowAutoRedirect = false;
                        using (HttpWebResponse logIn6Response = (HttpWebResponse)logIn6.GetResponse())
                        {
                           

                            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(element.GetAttribute("src"));
                            httpRequest.Method = WebRequestMethods.Http.Get;
                            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                            Stream httpResponseStream = httpResponse.GetResponseStream();

                            int bufferSize = 1024;
                            byte[] buffer = new byte[bufferSize];
                            int bytesRead = 0;

                            Monitor.Enter(path);
                            try
                            {
                                using (FileStream fileStream = File.Create(path))
                                {
                                    while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                                    {
                                        fileStream.Write(buffer, 0, bytesRead);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                            Monitor.Exit(path); 
                            return true;
                        }

                    }
                }
                    return false;      
            }
            catch(WebException  )
            {
                return false;
            }
            

        }
    }
    }
 