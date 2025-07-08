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
        //private string baseconnectionString = "Data Source=LAPTOP-DP8JTMS7\\PUONG206;Initial Catalog=MuseumKeretaApi;";  //DB Arya
        //private string baseconnectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;";  //DB Olip
        //private string baseconnectionString = "Data Source=LAPTOP-HDNQCJHP\\WILDAN_ZAUHAIR;Initial Catalog=MuseumKeretaApi;";  //DB Welly

        private readonly Koneksi koneksi = new Koneksi();

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
                string baseConnectionString = koneksi.GetConnectionString();

                if (string.IsNullOrEmpty(baseConnectionString))
                {
                    ShowAlert("Connection string tidak ditemukan. Pastikan koneksi database sudah diatur.");
                    return;
                }

                string userConnectionString = $"{baseConnectionString}User ID={username};Password={password};";
                // Coba buka koneksi untuk memvalidasi login
                using (SqlConnection conn = new SqlConnection(userConnectionString))
                {
                    conn.Open();
                }
                MainWindow mainWindow = new MainWindow(userConnectionString);
                mainWindow.Show();
                this.Close();

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
            // Cek jika tombol yang ditekan adalah Enter
            if (e.Key == Key.Enter)
            {
                // Panggil metode klik tombol login secara manual
                Button_Click(sender, e);

                // Tandai bahwa event ini sudah ditangani agar tidak diproses lebih lanjut
                e.Handled = true;
            }
            // Cek jika tombol yang ditekan adalah Escape
            else if (e.Key == Key.Escape)
            {
                if (CustomMessageBox.ShowYesNo("Apakah Anda yakin ingin keluar dari aplikasi?",
                    "Konfirmasi Keluar"))
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Sinkronkan isi TextBox dengan PasswordBox setiap kali password diketik
            RevealedPasswordTextBox.Text = passwordTextBox.Password;
            HideAlert();
        }

        private void ViewPassword_Checked(object sender, RoutedEventArgs e)
        {
            // Saat tombol "mata" dicentang (mode lihat password)
            passwordTextBox.Visibility = Visibility.Collapsed;
            RevealedPasswordTextBox.Visibility = Visibility.Visible;
            RevealedPasswordTextBox.Focus(); // Pindahkan fokus ke TextBox
        }

        private void ViewPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // Saat tombol "mata" tidak dicentang (mode password tersembunyi)
            RevealedPasswordTextBox.Visibility = Visibility.Collapsed;
            passwordTextBox.Visibility = Visibility.Visible;
            passwordTextBox.Focus(); // Pindahkan fokus ke PasswordBox
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