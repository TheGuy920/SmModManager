using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SmModManager.Core.Models;

namespace SmModManager.Core.Bindings
{

    public class ErrorDataBinding
    {


        public string Name { get; private set; }
        public CheckBox Box { get; private set; }
        public string Function { get; private set; }

        public static ErrorDataBinding Create(string Name, string func)
        {
            var binding = new ErrorDataBinding
            {
                Name = Name,
                Function = func,
                Box = new CheckBox()
            };
            return binding;
        }

    }

}