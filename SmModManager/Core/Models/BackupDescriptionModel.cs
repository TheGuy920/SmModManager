using System;
using System.IO;
using Newtonsoft.Json;
using SmModManager.Core.Enums;

namespace SmModManager.Core.Models
{

    public class BackupDescriptionModel
    {

        [JsonProperty("type")] public BackupType Type { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("worldName")] public string WorldName { get; set; }

        [JsonProperty("data")] public byte[] Data { get; set; }

        [JsonProperty("time")] public DateTime Time { get; set; }

        public void Save(string outputPath)
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(outputPath, json);
        }

        public static BackupDescriptionModel Load(string inputFile)
        {
            var json = File.ReadAllText(inputFile);
            return JsonConvert.DeserializeObject<BackupDescriptionModel>(json);
        }

    }

}