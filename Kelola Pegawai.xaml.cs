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
    /// Interaction logic for Kelola_Pegawai.xaml
    /// </summary>
    public partial class Kelola_Pegawai : Page
    {
        private string baseConnectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password";

        public Kelola_Pegawai()
        {
            InitializeComponent();
        }

        private void dataGridPegawai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
