using System.ComponentModel;

namespace SmModManager.Core.Bindings
{

    public class ModListItemBinding : INotifyPropertyChanged
    {

        private int _currentImage;
        private bool _isSpinning;

        private bool _isVisible;

        public bool IsLoading
        {
            get => _isSpinning;
            set
            {
                _isSpinning = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public int CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        public string Name { get; private set; }

        public string Id { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(
            string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


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