using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class LoginWindow : Window
    {
        //private string baseconnectionString = "Data Source=LAPTOP-DP8JTMS7\\PUONG206;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Arya
        //private string baseconnectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Olip
        private string baseconnectionString = "Data Source=LAPTOP-HDNQCJHP\\WILDAN_ZAUHAIR;Initial Catalog=MuseumKeretaApi;Integrated Security=True;";  //DB Welly


        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
        }

        private void ShowAlert(string message)
        {
            AlertTextBlock.Text = message;
            AlertTextBlock.Visibility = Visibility.Visible;
        }

        private void HideAlert()
        {
            AlertTextBlock.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HideAlert(); // Sembunyikan alert sebelumnya jika ada

            string username = UsernameTextBox.Text;
            string password = passwordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowAlert("Username dan password tidak boleh kosong.");
                return;
            }

            try
            {
                // Bangun connection string lengkap dengan kredensial pengguna
                // PENTING: Pendekatan ini hanya untuk SQL Server Authentication.
                // Jika Anda menggunakan Integrated Security (Windows Auth), logika login akan berbeda.
                string connectionString = baseconnectionString + $"User ID={username};Password={password}";

                // Coba buka koneksi untuk memvalidasi login
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MainWindow mainWindow = new MainWindow(connectionString);
                    mainWindow.Show();
                    this.Close();
                }
                
            }
            catch (SqlException ex)
            {
                // Error 18456 secara spesifik adalah "Login failed for user..."
                if (ex.Number == 18456)
                {
                    ShowAlert("Login gagal: Username atau password salah.");
                }
                else
                {
                    ShowAlert("Kesalahan database: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Terjadi kesalahan: " + ex.Message);
            }
        }

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

        // Metode-metode kosong ini bisa dihapus jika tidak digunakan
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e) 
        {
            HideAlert();
        }
        private void Frame_Navigated_2(object sender, NavigationEventArgs e) 
        {
            HideAlert();
        }
    }
}