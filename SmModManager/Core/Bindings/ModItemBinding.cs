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
            var preview = default(BitmapFrame);
            if (File.Exists(System.IO.Path.Combine(path, "preview.png")))
                preview = BitmapFrame.Create(new Uri(System.IO.Path.Combine(path, "preview.png")));
            else if (File.Exists(System.IO.Path.Combine(path, "preview.jpg")))
                preview = BitmapFrame.Create(new Uri(System.IO.Path.Combine(path, "preview.jpg")));
            return new ModItemBinding
            {
                Name = ModDescriptionModel.Load(System.IO.Path.Combine(path, "description.json")).Name,
                Path = path,
                Preview = preview
            };
        }

    }

}