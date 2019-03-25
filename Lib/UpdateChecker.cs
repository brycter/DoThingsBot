using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DoThingsBot.Lib {
    public class GitLabTagData {
        public string tag_name = "";
        public DateTime created_at = DateTime.MinValue;

    }

    public static class UpdateChecker {
        private static string json = "";

        public static void CheckForUpdate() {
            //new Newtonsoft.Json.Serialization.Action(FetchGitlabData).BeginInvoke(new AsyncCallback(OnGitlabFetchComplete), null);
        }

        public static void FetchGitlabData() {

            // no tls 1.2 in dotnet 3.5???
            try {
                var url = string.Format(@"https://gitlab.com/api/v4/projects/9782839/releases");

                Util.WriteToChat("Fetching");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10000;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            json = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private static void OnGitlabFetchComplete(IAsyncResult result) {
            try {
                if (!string.IsNullOrEmpty(json)) {
                    try {
                        var tags = JsonConvert.DeserializeObject<GitLabTagData[]>(json);

                        foreach (var tag in tags) {
                            var released = Util.GetFriendlyTimeDifference(DateTime.UtcNow - tag.created_at);
                            Util.WriteToChat(string.Format("{0}: {1}", tag.tag_name.Replace("release-",""), released));
                        }
                    }
                    catch (Exception ex) { Util.LogException(ex); }
                }
            }
            catch (Exception ex) {
                Util.LogException(ex);
            }
        }
    }
}
