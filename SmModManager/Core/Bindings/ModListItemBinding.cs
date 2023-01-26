using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using SmModManager.Core.Models;
using SmModManager.Graphics;

namespace SmModManager.Core.Bindings
{

    public class ModListItemBinding : INotifyPropertyChanged
    {
        private bool _isSpinning;

        public bool IsLoading
        {
            get => _isSpinning;
            set
            {
                _isSpinning = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private int _currentImage;

        public int CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(
          string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name { get; private set; }

        public string Id { get; private set; }



        public static ModListItemBinding Create(string name, string id)
        {
            var binding = new ModListItemBinding
            {
                Name = name,
                Id = id
            };
            return binding;
        }
    }
}