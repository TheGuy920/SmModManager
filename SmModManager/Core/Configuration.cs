using System;
using System.IO;
using System.Xml.Serialization;
using SmModManager.Core.Enums;

namespace SmModManager.Core
{

    public class Configuration
    {

        private static readonly string Source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "configuration.smmm");
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Configuration));

        public string GameDataPath { get; set; }
        public string WorkshopPath { get; set; }
        public string UserDataPath { get; set; }

        public UpdatePreferenceOptions UpdatePreference { get; set; } = UpdatePreferenceOptions.RemindForUpdates;

        public void Save()
        {
            using var stream = new FileStream(Source, FileMode.Create);
            Serializer.Serialize(stream, this);
        }

        public void Reset()
        {
            if (File.Exists(Source))
                File.Delete(Source);
        }

        public static Configuration Load()
        {
            if (!File.Exists(Source))
                return new Configuration();
            using var stream = new FileStream(Source, FileMode.Open);
            return (Configuration)Serializer.Deserialize(stream);
        }

    }

}