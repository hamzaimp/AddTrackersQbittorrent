using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AddTrackers
{
    

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Config
    {
        
        public ConfigServer Server { get; set; }


        [XmlElement("TrackersFilePath")]
        public string TrackerPath { get; set; }

        public bool Valid => AreFieldsValid(Server.Url, Server.Password, Server.Username, TrackerPath);

        public override string ToString()
        {
            return $"Config settings: Valid? {Valid}. Tracker File Path: {TrackerPath??"NULL"}. Server: {Server?.ToString()??"NULL"}";
        }

        private bool AreFieldsValid(params string[] fields)
        {
            return fields.All(x => !string.IsNullOrWhiteSpace(x));
        }


    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class ConfigServer
    {

        private string _url;

        /// <remarks/>
        [XmlAttribute("url")]
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                string tmp = value;
                if (value.Trim().StartsWith("localhost")) tmp = "http://" + tmp;
                if(!Uri.IsWellFormedUriString(tmp,UriKind.RelativeOrAbsolute))throw new UriFormatException($"{value} is not a valid URL");
                _url = value;
            }
        }

        /// <remarks/>
        [XmlAttribute("username")]
        public string Username { get; set; }

        /// <remarks/>
        [XmlAttribute("password")]
        public string Password { get; set; }

        public override string ToString()
        {
            return $"Url: {Url??"NULL"}, Username: {Username??"NULL"}";
        }
    }

  



}
