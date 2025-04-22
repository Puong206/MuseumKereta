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
    /// Interaction logic for LoginnWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string connectionString = "Data Source=LAPTOP-HDNQCJHP\\WILDAN_ZAUHAIR;Initial Catalog=OrganisasiMahasiswa;Integrated Security=True";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;


        }
    }
}
