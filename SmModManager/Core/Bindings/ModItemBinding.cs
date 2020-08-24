using System;
using System.IO;
using System.Windows.Media.Imaging;
using SmModManager.Core.Models;

namespace SmModManager.Core.Bindings
{

    public class ModItemBinding
    {

        public BitmapFrame Preview { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Path { get; private set; }

        public string Url { get; private set; }

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
            var binding = new ModItemBinding();
            binding.Preview = BitmapFrame.Create(new Uri(previewPath));
            binding.Name = description.Name;
            binding.Description = description.Description;
            binding.Path = path;
            if (description.WorkshopId != 0)
                binding.Url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + description.WorkshopId;
            return binding;
        }

    }

}