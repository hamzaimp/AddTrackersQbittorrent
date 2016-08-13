using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AddTrackers
{
    class Rest
    {
        public static string Get(string add)
        {
            Uri uri = new Uri(Settings.Url+add);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            String reponse = String.Empty;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    
                    Stream dataStream = response.GetResponseStream();
                    if (dataStream != null)
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        reponse = reader.ReadToEnd();
                        reader.Close();
                    }
                    dataStream?.Close();
                }
            }
            catch (Exception e)
            {

                throw new Exception($"Caught exception - {e}");
            }
            return reponse;
        }
        public static void AddTrackers(bool recursive = true)
        {
            string url = $"{Settings.Url}/command/addTrackers";
            
            //Iterate through each torrent
            foreach (var hash in Settings.Hashes)
            {
                //POST all tracker URLs chained together (%0A=& I think) in one request.
                string track = $"hash={hash}&urls=" + String.Join("%0A", Settings.Trackers.Select(x => x.Trim()));
                Console.WriteLine(Post(url,track));
            }


            
        }

        public static Dictionary<string,string> GetTorrentInfoJsons()
        {
            Program.GetHashes();
            return Settings.Hashes.ToDictionary(hash => hash,hash=>Get($"/query/propertiesFiles/{hash.Trim()}"));
        }

        //This would be a lot better/cleaner with RestSharp... just sayin
        public static string Post(string url,string postData)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            myHttpWebRequest.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = data.Length;
            myHttpWebRequest.CookieContainer = new CookieContainer(1);
            myHttpWebRequest.CookieContainer.SetCookies(new Uri(url), Settings.Cookie);
            myHttpWebRequest.Host = Settings.Host;
            myHttpWebRequest.UserAgent = "Fiddler";

            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse) myHttpWebRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();

            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();

            return pageContent;
        }

        public static string Login()
        {
            string url = $"{Settings.Url}/login";
            string postData = $"username={Settings.Username}&password={Settings.Password}";

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            myHttpWebRequest.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = data.Length;
            myHttpWebRequest.Host = Settings.Host;
            myHttpWebRequest.UserAgent = "Fiddler";
            

            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            

            Stream responseStream = myHttpWebResponse.GetResponseStream();
            Console.WriteLine(myHttpWebResponse.ContentType);
            string cookie = myHttpWebResponse.Headers.Get("Set-Cookie");
            Console.WriteLine($"Cookie = {cookie}");
            Regex cookieReg = new Regex(@"\W{14,}");
            Settings.Cookie = cookieReg.Match(cookie).Value;
            
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = String.Empty;
            
            while (!myStreamReader.EndOfStream)
            {
                pageContent += myStreamReader.ReadLine();
                Console.WriteLine(pageContent);
            }
            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();

            return pageContent;
        }

    }

}
