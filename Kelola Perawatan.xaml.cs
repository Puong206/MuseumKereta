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
            if (dataGridPerawatan.SelectedItem is DataRowView row)
            {
                txtIDBarang.Text = row["BarangID"].ToString();
                dpTanggalPerawatan.SelectedDate = Convert.ToDateTime(row["TanggalPerawatan"]);
                txtJenisPerawatan.Text = row["JenisPerawatan"].ToString();
                txtCatatan.Text = row["Catatan"].ToString();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIDBarang.Text) ||
                dpTanggalPerawatan.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtJenisPerawatan.Text) ||
                string.IsNullOrWhiteSpace(txtCatatan.Text) ||
                string.IsNullOrWhiteSpace(txtNIPP.Text))
            {
                MessageBox.Show("Harap lengkapi semua data.");
                return;
            }

            try
            {
                conn.Open();
                cmd = new SqlCommand("INSERT INTO Perawatan (BarangID, TanggalPerawatan, JenisPerawatan, Catatan, NIPP) VALUES (@barangid, @tanggal, @jenis, @catatan, @nipp)", conn);
                cmd.Parameters.AddWithValue("@barangid", txtIDBarang.Text);
                cmd.Parameters.AddWithValue("@tanggal", dpTanggalPerawatan.SelectedDate.Value);
                cmd.Parameters.AddWithValue("@jenis", txtJenisPerawatan.Text);
                cmd.Parameters.AddWithValue("@catatan", txtCatatan.Text);
                cmd.Parameters.AddWithValue("@nipp", txtNIPP.Text);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Data berhasil ditambahkan.");
                LoadData();

                // Kosongkan input
                txtIDBarang.Clear();
                txtJenisPerawatan.Clear();
                txtCatatan.Clear();
                txtNIPP.Clear();
                dpTanggalPerawatan.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambah data: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan diedit.");
                return;
            }

            int perawatanId = Convert.ToInt32(row["PerawatanID"]);

            try
            {
                conn.Open();
                cmd = new SqlCommand("UPDATE Perawatan SET BarangID = @barangid, TanggalPerawatan = @tanggal, JenisPerawatan = @jenis, Catatan = @catatan WHERE PerawatanID = @id", conn);
                cmd.Parameters.AddWithValue("@id", perawatanId);
                cmd.Parameters.AddWithValue("@barangid", txtIDBarang.Text);
                cmd.Parameters.AddWithValue("@tanggal", dpTanggalPerawatan.SelectedDate.Value);
                cmd.Parameters.AddWithValue("@jenis", txtJenisPerawatan.Text);
                cmd.Parameters.AddWithValue("@catatan", txtCatatan.Text);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Data berhasil diperbarui.");
                LoadData();

                txtIDBarang.Clear();
                txtJenisPerawatan.Clear();
                txtCatatan.Clear();
                dpTanggalPerawatan.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message);
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan dihapus.");
                return;
            }

            int perawatanId = Convert.ToInt32(row["PerawatanID"]);

            try
            {
                conn.Open();
                cmd = new SqlCommand("DELETE FROM Perawatan WHERE PerawatanID = @id", conn);
                cmd.Parameters.AddWithValue("@id", perawatanId);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Data berhasil dihapus.");
                LoadData();

                txtIDBarang.Clear();
                txtJenisPerawatan.Clear();
                txtCatatan.Clear();
                dpTanggalPerawatan.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
        }
    }
}
