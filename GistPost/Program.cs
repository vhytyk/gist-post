using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace GistPost
{
    internal class Program
    {
        #region Response classes
       
        [DataContract]
        public class GitResponse
        {
            [DataMember]
            public string id { get; set; }
        }
        #endregion

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }
       

        private static string PostOnGistAndGetLink(string text)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "GistPost");
                client.Headers.Add("Host", "api.github.com");
                client.Encoding = Encoding.UTF8;
                string json =
                    new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GistPost.gist.json"))
                        .ReadToEnd().Replace("[TextToReplace]", CleanForJson(text));
                string response = client.UploadString("https://api.github.com/gists", json);
                return response;
            }
        }

        class MyApplicationContext : ApplicationContext
        {
            //Component declarations
            private NotifyIcon TrayIcon;

            public MyApplicationContext()
            {
                Application.ApplicationExit += OnApplicationExit;
                InitializeComponent();
                TrayIcon.Visible = true;
                string textToPost = Clipboard.GetText();
                string response = PostOnGistAndGetLink(textToPost);
                var serializer = new DataContractJsonSerializer(typeof(GitResponse));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
                {
                    var responseObject = (GitResponse) serializer.ReadObject(stream);
                    Clipboard.SetText("https://gist.github.com/anonymous/" + responseObject.id);

                    TrayIcon.ShowBalloonTip(2000);
                    new Thread(() =>
                    {
                        Thread.Sleep(2000);
                        Application.Exit();
                    }).Start();
                }
            }

            private void InitializeComponent()
            {
                TrayIcon = new NotifyIcon();

                TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
                TrayIcon.BalloonTipText = "Gist link is copied to clipboard. You are free to paste it now.";
                TrayIcon.BalloonTipTitle = "Gist clipboard";
                TrayIcon.Icon = SystemIcons.Application;
            }

            private void OnApplicationExit(object sender, EventArgs e)
            {
                TrayIcon.Visible = false;
            }

        }

        private static string CleanForJson(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String t;

            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ')
                        {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
