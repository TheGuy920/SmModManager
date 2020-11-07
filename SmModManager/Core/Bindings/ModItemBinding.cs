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
        public string ModIdLocation { get; private set; }
        public int ModId { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }

        public string Path { get; private set; }

        public string Url { get; private set; }

        public static ModItemBinding Create(string path)
        {
            var previewPath = string.Empty;
            var description = ModDescriptionModel.Load(System.IO.Path.Combine(path, "description.json"));
            if (File.Exists(System.IO.Path.Combine(path, "preview.png")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.png");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.WorkshopId + ".png");
                if (!File.Exists(previewTempPath))
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            else if (File.Exists(System.IO.Path.Combine(path, "preview.jpg")))
            {
                previewPath = System.IO.Path.Combine(path, "preview.jpg");
                var previewTempPath = System.IO.Path.Combine(Constants.CachePath, description.WorkshopId + ".jpg");
                if (!File.Exists(previewTempPath))
                    File.Copy(previewPath, previewTempPath);
                previewPath = previewTempPath;
            }
            var binding = new ModItemBinding
            {
                Preview = BitmapFrame.Create(new Uri(previewPath)),
                ImagePath = previewPath,
                Name = description.Name,
                Description = description.Description,
                Path = path
            };
            string Url;
            if (description.Location == null)
            {
                binding.ModIdLocation = "12ws" + description.WorkshopId;
                Url = "https://steamcommunity.com/sharedfiles/filedetails/?id=";
            }
            else
            {
                var site = description.Location;
                binding.ModIdLocation = site + description.WorkshopId;
                Url = site;
            }
            if (description.WorkshopId != 0 && description.Location == null)
            {
                binding.Url = Url + description.WorkshopId;
                binding.ModId = (int)description.WorkshopId;
            }
            else if (description.WorkshopId != 0)
            {
                var numbers = "0123456789";
                var subStrStartIndex = numbers.IndexOf(Url[0]);
                Url = Url.Remove(0, subStrStartIndex + 1);
                binding.Url = Url;
                binding.ModId = (int)description.WorkshopId;
            }
            else
            {
                var numbers = "0123456789";
                var subStrStartIndex = numbers.IndexOf(Url[0]);
                Url = Url.Remove(0, subStrStartIndex + 1);
                binding.Url = Url;
                var folderName = path.Split("\\")[^1];
                var newName = "";
                foreach (var character in folderName)
                    if (numbers.Contains(character))
                        newName += character;
                if (newName.Length < 5)
                {
                    var rnd = new Random();
                    newName += rnd.Next(999999, 999999999).ToString();
                }
                if (newName != folderName)
                    try
                    {
                        Directory.Move(path, System.IO.Path.Combine(path.Replace(folderName, ""), newName));
                    }
                    catch
                    {
                        var rnd = new Random();
                        newName = newName.Substring(2);
                        newName += rnd.Next(9, 99).ToString();
                        Directory.Move(path, System.IO.Path.Combine(path.Replace(folderName, ""), newName));
                    }
                if (newName.Length > 9)
                {
                    newName = newName.Substring(1);
                    Directory.Move(path, System.IO.Path.Combine(path.Replace(folderName, ""), newName));
                }
                binding.ModId = int.Parse(newName);
            }
            if (binding.Url.Contains("smmods.com"))
                binding.Url = binding.Url.Split("/version/")[0];
            return binding;
        }

    }

}