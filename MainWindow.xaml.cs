using System.Windows;
using System.Windows.Input;

namespace MuseumApp
{
    public partial class MainWindow : Window
    {
        //private string connectionString = "Data Source=LAPTOP-DP8JTMS7\\PUONG206;Initial Catalog=MuseumKeretaApi;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Arya
        //private string connectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Olip
        private string connectionString = "Data Source=LAPTOP-HDNQCJHP\\WILDAN_ZAUHAIR;Initial Catalog=MuseumKeretaApi;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Welly


        public MainWindow(string connStr)
        {
            InitializeComponent();
            connectionString = connStr; // Simpan connection string

            // Saat MainWindow terbuka, langsung tampilkan Page1 (dashboard)
            NavigateHome();
        }

        public void NavigateHome()
        {
            // Navigasi ke instance baru dari Page1
            MainFrame.Navigate(new Page1(connectionString));

            // PENTING: Hapus semua riwayat navigasi 'Back' agar pengguna
            // tidak bisa kembali dari dashboard ke halaman sebelumnya (misal, Kelola Koleksi)
            while (MainFrame.NavigationService.CanGoBack)
            {
                MainFrame.NavigationService.RemoveBackEntry();
            }
        }

        // Metode untuk menangani penekanan tombol Esc untuk keluar aplikasi
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Apakah Anda yakin ingin keluar dari aplikasi?",
                    "Konfirmasi Keluar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}