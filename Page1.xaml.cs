using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private string connectionString = "Data Source=OLIPIA\\\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password";
        public Page1()
        {
            InitializeComponent();
        }

        private void ButtonKoleksi_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.GantiHalaman(new Kelola_Koleksi());
            }
        }

        private void ButtonBarang_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.GantiHalaman(new Kelola_Barang());
            }
        }

        private void ButtonPegawai_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.GantiHalaman(new Kelola_Pegawai());
            }
        }

        private void ButtonPerawatan_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.GantiHalaman(new Kelola_Perawatan());
            }
        }
    }
}
