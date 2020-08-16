using System.IO;
using System.Windows;
using SmModManager.Core.Bindings;

namespace SmModManager.Graphics
{

    public partial class PgManage
    {

        public PgManage()
        {
            InitializeComponent();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Directory.Exists(Path.Combine(path, "Survival")))
                    CompatibleModsList.Items.Add(ModItemBinding.Create(path));
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                AllModsList.Items.Add(ModItemBinding.Create(path));
            // TODO: Add current mods
        }

        private void InjectMod(object sender, RoutedEventArgs args)
        {
            // TODO: Inject mod
        }

        private void ArchiveMod(object sender, RoutedEventArgs args)
        {
            // TODO: Archive mod
        }

    }

}