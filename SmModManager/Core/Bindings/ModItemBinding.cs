using System;
using System.IO;
using System.Windows.Media.Imaging;
using SmModManager.Core.Models;

namespace SmModManager.Core.Bindings
{

    public class ModItemBinding
    {

        public string Name { get; set; }
        public BitmapFrame Preview { get; set; }

        public static ModItemBinding Create(string path)
        {
            BitmapFrame preview = null;
            if (File.Exists(Path.Combine(path, "preview.png"))) // TODO: Needs improvement
                preview = BitmapFrame.Create(new Uri(Path.Combine(path, "preview.png")));
            else if (File.Exists(Path.Combine(path, "preview.jpg")))
                preview = BitmapFrame.Create(new Uri(Path.Combine(path, "preview.jpg")));
            return new ModItemBinding
            {
                Name = ModDescriptionModel.Load(Path.Combine(path, "description.json")).Name,
                Preview = preview
            };
        }

    }

}