using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Kelola_Pegawai.xaml
    /// </summary>
    public partial class Kelola_Pegawai : Page
    {
        private string baseConnectionString = "Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password";

        public Kelola_Pegawai()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(baseConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Karyawan", connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridPegawai.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data: " + ex.Message);
                }
            }
        }

        private void dataGridPegawai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPegawai.SelectedItem is DataRowView row)
            {
                txtNIPP.Text = row["NIPP"].ToString();
                txtNama.Text = row["Nama"].ToString();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(baseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Karyawan (NIPP, NamaKaryawan) VALUES (@NIPP, @NamaKaryawan)";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@NIPP", txtNIPP.Text);
                    cmd.Parameters.AddWithValue("@NamaKaryawan", txtNama.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil ditambahkan");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambahkan data: " + ex.Message);
                }
            }

        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(baseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Karyawan SET NamaKaryawan = @NamaKaryawan WHERE NIPP = @NIPP";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@NIPP", txtNIPP.Text);
                    cmd.Parameters.AddWithValue("@NamaKaryawan", txtNama.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil diubah");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengedit data: " + ex.Message);
                }
            }

        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(baseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Karyawan WHERE NIPP = @NIPP";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@NIPP", txtNIPP.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil dihapus");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
                }
            }

        }
    }
}
