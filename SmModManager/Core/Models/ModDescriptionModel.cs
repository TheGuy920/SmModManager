using System;
using System.IO;
using Newtonsoft.Json;

namespace SmModManager.Core.Models
{

    public class ModDescriptionModel
    {

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fileId")]
        public uint WorkshopId { get; set; }

        [JsonProperty("localId")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public uint Version { get; set; }

        public static ModDescriptionModel Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ModDescriptionModel>(json);
        }

    }

}