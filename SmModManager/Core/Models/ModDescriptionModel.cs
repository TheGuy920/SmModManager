using System;
using System.IO;
using Newtonsoft.Json;

namespace SmModManager.Core.Models
{

    public class ModDescriptionModel
    {

        [JsonProperty("localId")]
        public Guid Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("version")]
        public uint Version { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("fileId")]
        public uint WorkshopId { get; private set; }

        public static ModDescriptionModel Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ModDescriptionModel>(json);
        }

    }

}