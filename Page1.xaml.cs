using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private string connectionString = "Data Source=LAPTOP-DP8JTMS7\\PUONG206;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password";
        public Page1(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
        }

        private void ButtonKoleksi_Click(object sender, RoutedEventArgs e)
        {
            Kelola_Koleksi kelolaKoleksi = new Kelola_Koleksi(connectionString);
            this.NavigationService.Navigate(kelolaKoleksi);
        }

        private void ButtonBarang_Click(object sender, RoutedEventArgs e)
        {
            Kelola_Barang kelolaBarang = new Kelola_Barang(connectionString);
            this.NavigationService.Navigate(kelolaBarang);
        }

        private void ButtonPegawai_Click(object sender, RoutedEventArgs e)
        {
            Kelola_Pegawai kelolaPegawai = new Kelola_Pegawai(connectionString);
            this.NavigationService.Navigate(kelolaPegawai);
        }

        private void ButtonPerawatan_Click(object sender, RoutedEventArgs e)
        {
            Kelola_Perawatan kelolaPerawatan = new Kelola_Perawatan(connectionString);
            this.NavigationService.Navigate(kelolaPerawatan);
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Apakah Anda yakin ingin logout?", "Konfirmasi Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window parentWindow = Window.GetWindow(this);

                LoginWindow loginWindow= new LoginWindow();

                loginWindow.Show();

                if (parentWindow != null)
                {
                    parentWindow.Close();
                }
            }
        }

        private void ButtonReportExport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void LaporanBarang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Buat instance dari Window LaporanBarang
                var reportWindow = new LaporanBarang(this.connectionString);

                // (Opsional tapi direkomendasikan) Set window utama sebagai pemilik window laporan
                reportWindow.Owner = Window.GetWindow(this);

                // Tampilkan window laporan.
                // Gunakan .Show() agar tidak memblokir window utama,
                // atau .ShowDialog() jika Anda ingin window utama tidak bisa diakses sampai window laporan ditutup.
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal membuka jendela laporan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaporanPerawatan_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new LaporanPerawatan(this.connectionString);

            // (Opsional tapi direkomendasikan) Set window utama sebagai pemilik window laporan
            reportWindow.Owner = Window.GetWindow(this);

            // Tampilkan window laporan.
            // Gunakan .Show() agar tidak memblokir window utama,
            // atau .ShowDialog() jika Anda ingin window utama tidak bisa diakses sampai window laporan ditutup.
            reportWindow.Show();
            
        }

        private void LaporanPegawai_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new LaporanPegawai(this.connectionString);

            // (Opsional tapi direkomendasikan) Set window utama sebagai pemilik window laporan
            reportWindow.Owner = Window.GetWindow(this);

            // Tampilkan window laporan.
            // Gunakan .Show() agar tidak memblokir window utama,
            // atau .ShowDialog() jika Anda ingin window utama tidak bisa diakses sampai window laporan ditutup.
            reportWindow.Show();
        }

        private void LaporanKoleksi_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new LaporanKoleksi(this.connectionString);

            // (Opsional tapi direkomendasikan) Set window utama sebagai pemilik window laporan
            reportWindow.Owner = Window.GetWindow(this);

            // Tampilkan window laporan.
            // Gunakan .Show() agar tidak memblokir window utama,
            // atau .ShowDialog() jika Anda ingin window utama tidak bisa diakses sampai window laporan ditutup.
            reportWindow.Show();
        }
    }
    }

