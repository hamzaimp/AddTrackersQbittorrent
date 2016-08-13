using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.Text;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;


namespace AddTrackers
{

    public class Settings
    {
        public static Config Config;
        public static string Url => Config.Server.Url.Trim('/');

        public static string Username => Config.Server.Username;
        
        public static string Password => Config.Server.Password;

        public static List<string> Trackers
            => File.ReadAllLines("trackers.txt").Select(x => x.Trim()).ToList();

        public static string Cookie { get; set; }

        public static List<string> Hashes { get; set; }

        public static string Host => Url.Replace("http://", String.Empty);
    }
    
    class Program
    {
        
        
        static void Main(string[] args)
        {
            if(!File.Exists("Config.xml"))throw new FileNotFoundException($"Config.xml was not found. This file must be placed in the directory of this program");
            using (StreamReader reader = new StreamReader("Config.xml"))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                try
                {
                    Settings.Config = ser.Deserialize(reader) as Config;
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidOperationException($"Unable to read Config.xml. It is likely it is incorrectly formatted. Exception - {e}");
                }
            }
            if(!Settings.Config?.Valid??false)throw new ArgumentException($"Config.xml was not valid - {Settings.Config?.ToString()??"NULL"}");
            Console.WriteLine($"Config Settings:\n{Settings.Config}");
            Console.WriteLine(Rest.Login());
            GetHashes();
            Rest.AddTrackers();
            Console.ReadLine();
        }


        

        

        
        
        public static void GetHashes()
        {
            //Each torrent is represented by a hash you get by querying the API.
            //For any POST requests, you need the hash.
            string url = "/query/torrents";
            string output = Rest.Get(url);
            Settings.Hashes = TryGetValue("hash",output);
            
        }

        private static List<string> TryGetValue(string name, string output)
        {
            var json = JsonObject.ParseArray(output);
            List<string> values = new List<string>();
            foreach (var s in json)
            {
                string value;
                if (!s.TryGetValue(name, out value)) continue;
                Console.WriteLine($"Got value {value}");
                values.Add(value);
            }
            return values;
        }


        /// <summary>
        /// Something I was testing out, don't use it at all
        /// </summary
        public static void DeleteCompletedTorrents()
        {
            var info = Rest.GetTorrentInfoJsons();
            Dictionary<string, float> hashAndProgress = new Dictionary<string, float>();
            foreach (var kevVal in info)
            {
                Console.WriteLine($"Json info - {kevVal.Value}");
                //                var json = JsonObject.ParseArray(torrent);

                if (!TryGetNameAndProgress(kevVal, ref hashAndProgress))
                {
                    Console.WriteLine($"Failed to get progress for {kevVal.Value}");
                }

            }
            var completed = hashAndProgress.Where(kv => Math.Abs(kv.Value - 1) < .0003).Select(x => x.Key);
            if (completed.Any())
            {
                string hashes = "hashes=" + String.Join("|", completed);
                Console.WriteLine("POST " + hashes);
            }
            else
            {
                Console.WriteLine("No completed torrents, try again later.");
            }

        }
        /// <summary>
        /// Also don't use this
        /// </summary>
        /// <param name="hashJson"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool TryGetNameAndProgress(KeyValuePair<string,string> hashJson , ref Dictionary<string, float> dict)
        {
            string json = hashJson.Value;
            var name = TryGetValue("name", json).FirstOrDefault();
//            kv = new KeyValuePair<string, float>();
            if (name == null)
            {
                Console.WriteLine($"No name found in {json}. Will not remove");
                return false;
            }
            Regex nameReg = new Regex(@"\\\\");
            if (nameReg.IsMatch(name))
            {
                name = nameReg.Split(name).First();
            }
            var progressString = TryGetValue("progress", json).FirstOrDefault();
            if (progressString == null)
            {
                Console.WriteLine($"Could not get progress from {json}");
                return false;
            }
            float prog;
            if (!float.TryParse(progressString.Trim(':'), out prog))
            {
                Console.WriteLine($"Could not parse progress {progressString} into float.");
                return false;
            }

            Console.WriteLine($"Adding {name} to dict. Hash = {hashJson.Key}Progress = {prog}");
            dict[hashJson.Key] = prog;
            return true;
        }
      




    }
}
