using System;
using System.IO;
using System.Windows;
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
        public string NewFileName { get; set; }
        public int StartUpX { get; set; }
        public int StartUpY { get; set; }
        public bool DevMode { get; set; }
        public bool VerMode { get; set; }
        public bool WindowMode { get; set; }
        public WindowState StartupMode { get; set; }
        public bool LatestDownloadComplete { get; set; }
        public bool HasFormattedMods { get; set; }
        public bool HasTakenTutorial { get; set; }
        public bool DontAskMeToReValidate { get; set; }

        public UpdateBehaviorOptions UpdatePreference { get; set; } = UpdateBehaviorOptions.RemindForUpdates;
        public LanguageOptions AppLanguage { get; set; } = LanguageOptions.English;

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
        public static Configuration GetConfiguration;
        public Configuration()
        {
            GetConfiguration = this;
        }
        public static Configuration Load()
        {
            try
            {
                if (!File.Exists(Source))
                    return new Configuration();
                var stream = new FileStream(Source, FileMode.Open);
                var result = (Configuration)Serializer.Deserialize(stream);
                stream.Close();
                result.HasTakenTutorial = true;
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }

}