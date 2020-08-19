using System;
using System.IO;
using System.Windows.Media.Imaging;
using SmModManager.Core.Models;

namespace SmModManager.Core.Bindings
{

    public class ModItemBinding
    {

        public string Name { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public BitmapFrame Preview { get; set; }

        public static ModItemBinding Create(string path)
        {
            
            var previewPath = string.Empty;
            var description = ModDescriptionModel.Load(System.IO.Path.Combine(path, "description.json"));
            if (File.Exists(System.IO.Path.Combine(path, "preview.png")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.png");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.Id + ".png");
                if (!File.Exists(previewTempPath))
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            else if (File.Exists(System.IO.Path.Combine(path, "preview.jpg")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.jpg");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.Id + ".jpg");
                if (!File.Exists(previewTempPath)) 
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            if (string.IsNullOrEmpty(previewPath))
            {
                // TODO: Add placeholder image
            }
            var binding = new ModItemBinding();
            if (description.WorkshopId != 0)
                binding.Url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + description.WorkshopId;
            binding.Preview = BitmapFrame.Create(new Uri(previewPath));
            binding.Path = path;
            binding.Name = description.Name;
            return binding;
        }

    }

}