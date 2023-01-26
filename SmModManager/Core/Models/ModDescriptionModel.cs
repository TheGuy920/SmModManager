using System;
using System.IO;
using Newtonsoft.Json;

namespace SmModManager.Core.Models
{

    public class ModDescriptionModel
    {

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("localId")] public string Id { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("location")] public string Location { get; set; }

        [JsonProperty("fileId")] public uint WorkshopId { get; set; }

        public static ModDescriptionModel Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ModDescriptionModel>(json);
            }
            catch
            {
                throw new Exception("Error Loading The Specified Path: " + path);
            }
        }

    }

}