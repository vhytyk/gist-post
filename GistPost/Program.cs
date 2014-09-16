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
using System.Windows.Forms;
using System.Reflection;

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
            string textToPost = Clipboard.GetText();
            string response = PostOnGistAndGetLink(textToPost);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GitResponse));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
            {
                GitResponse obj = (GitResponse)serializer.ReadObject(stream);
                Clipboard.SetText("https://gist.github.com/anonymous/" + obj.id);
                MessageBox.Show("Gist url copied to clipboard");
            }
            
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
