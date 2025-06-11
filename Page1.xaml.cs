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
        private readonly string connectionString;
        public Page1(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            Loaded += Page1_Loaded;
        }

        private void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainContentFrame.Content == null)
            {
                // Memanggil metode klik tombol dashboard untuk konsistensi
                ButtonDashboard_Click(null, null);
            }
        }

        private void ButtonKoleksi_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Kelola_Koleksi(connectionString));
        }

        private void ButtonBarang_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Kelola_Barang(connectionString));
        }

        private void ButtonPegawai_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Kelola_Pegawai(connectionString));
        }

        private void ButtonPerawatan_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Kelola_Perawatan(connectionString));
        }

        

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Apakah Anda yakin ingin logout?", "Konfirmasi Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window parentWindow = Window.GetWindow(this);
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                parentWindow?.Close();
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

        private void ButtonImportData_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new ImportData(connectionString));
        }

        private void ButtonDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContentFrame.Navigate(new DashboardHomePage(this.connectionString));
                ButtonDashboard.Focus(); // Memberi highlight pada tombol yang aktif
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat halaman dashboard: " + ex.Message, "Error Navigasi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

