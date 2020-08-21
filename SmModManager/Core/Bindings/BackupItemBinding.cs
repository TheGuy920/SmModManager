using System;
using SmModManager.Core.Models;

namespace SmModManager.Core.Bindings
{

    public class BackupItemBinding
    {

        public string Name { get; private set; }

        public string WorldName { get; private set; }

        public string Path { get; private set; }

        public DateTime Time { get; private set; }

        public static BackupItemBinding Create(string path)
        {
            var description = BackupDescriptionModel.Load(path);
            return new BackupItemBinding
            {
                Name = description.Name,
                WorldName = description.WorldName,
                Path = path,
                Time = description.Time
            };
        }

    }

}