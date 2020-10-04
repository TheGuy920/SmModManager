using System;
using System.IO;
using Newtonsoft.Json;

namespace SmModManager.Core.Models
{

    public class FileDetailsModel
    {

        //[JsonProperty("action")] public string Action { get; private set; }
        //[JsonProperty("contentMd5")] public string ContentMd5 { get; private set; }
        //[JsonProperty("contentSha1")] public string ContentSha1 { get; private set; }
        //[JsonProperty("contentType")] public string ContentType { get; private set; }
        [JsonProperty("fileId")] public string FileId { get; private set; }

        //[JsonProperty("fileName")] public string FileName { get; private set; }
        //[JsonProperty("size")] public uint Size { get; private set; }
        //[JsonProperty("uploadTimestamp")] public uint UploadTimestamp { get; private set; }
        public static FileDetailsModel Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<FileDetailsModel>(json);
            }
            catch
            {
                throw new Exception("Error Loading The Specified Path: " + path);
            }
        }

    }

}