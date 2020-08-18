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

        public BitmapFrame Preview { get; set; }

        public static ModItemBinding Create(string path)
        {
            var previewPath = string.Empty;
            var description = ModDescriptionModel.Load(System.IO.Path.Combine(path, "description.json"));
            if (File.Exists(System.IO.Path.Combine(path, "preview.png")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.png");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.GameId + ".png");
                if (!File.Exists(previewTempPath))
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            else if (File.Exists(System.IO.Path.Combine(path, "preview.jpg")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.jpg");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.GameId + ".jpg");
                if (!File.Exists(previewTempPath)) 
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            if (string.IsNullOrEmpty(previewPath))
            {
                // TODO: Add placeholder image
            }
            var preview = BitmapFrame.Create(new Uri(previewPath));
            return new ModItemBinding
            {
                Name = description.Name,
                Path = path,
                Preview = preview
            };
        }

    }

}