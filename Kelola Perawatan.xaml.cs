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
using System.Data;
using System.Data.SqlClient;


namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for Kelola_Perawatan.xaml
    /// </summary>
    public partial class Kelola_Perawatan : Page
    {
        SqlConnection conn = new SqlConnection("Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password");
        SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dt;

        public Kelola_Perawatan()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                conn.Open();
                adapter = new SqlDataAdapter("SELECT * FROM Perawatan", conn);
                dt = new DataTable();
                adapter.Fill(dt);
                dataGridPerawatan.ItemsSource = dt.DefaultView;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void dataGridPerawatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
