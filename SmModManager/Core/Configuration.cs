using System;
using System.Diagnostics;
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
        public bool HasTakenTutorial { get; set; }

        public UpdateBehaviorOptions UpdatePreference { get; set; } = UpdateBehaviorOptions.RemindForUpdates;

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
            try
            {
                if (!File.Exists(Source))
                    return new Configuration();
                File.OpenRead(Source);
                return (Configuration)Serializer.Deserialize(new FileStream(Source, FileMode.Open));
            }
            catch(Exception e)
            {
                throw e;
            }
        }

    }

}