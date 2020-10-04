using System;
using System.IO;
using Newtonsoft.Json;

namespace SmModManager.Core.Models
{

    public class ModDescriptionModel
    {

        [JsonProperty("version")] public string Version { get; private set; }
        [JsonProperty("localId")] public string Id { get; private set; }

        [JsonProperty("name")] public string Name { get; private set; }

        [JsonProperty("description")] public string Description { get; private set; }

        [JsonProperty("location")] public string Location { get; private set; }

        [JsonProperty("fileId")] public uint WorkshopId { get; private set; }

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