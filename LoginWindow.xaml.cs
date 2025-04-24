using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for LoginnWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string baseconnectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void Frame_Navigated_1(object sender, NavigationEventArgs e)
        {

        }

        private void Frame_Navigated_2(object sender, NavigationEventArgs e)
        {

        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = passwordTextBox.Password;

            try
            {
                string connectionString = baseconnectionString + $"User ID={username};Password={password}";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Login berhasil!");

                    // Tampilkan Page1 dalam window baru
                    NavigationWindow navWindow = new NavigationWindow();
                    navWindow.Navigate(new Page1(connectionString));
                    navWindow.Show();

                    this.Close(); // Tutup LoginWindow
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 18456)
                {
                    MessageBox.Show("Login gagal: Username atau password salah.");
                }
                else
                {
                    MessageBox.Show("Kesalahan database: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Add logic here if needed, or leave empty if no specific action is required.
        }
    }
}
