using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Interaction logic for Kelola_Koleksi.xaml
    /// </summary>
    public partial class Kelola_Koleksi : Page
    {
        SqlConnection conn = new SqlConnection("Data Source=OLIPIA\\\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password");
        SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dt;

        TextBox txtJenisKoleksi = new TextBox();
        TextBox txtDeskripsi = new TextBox();
        TextBox hiddenId = new TextBox();
        // Dummy controls yang tidak ada di XAML
        public Kelola_Koleksi()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                conn.Open();
                adapter = new SqlDataAdapter("SELECT KoleksiID, JenisKoleksi, Deskripsi FROM Koleksi", conn);
                dt = new DataTable();
                adapter.Fill(dt);
                dataGridKoleksi.ItemsSource = dt.DefaultView;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void dataGridKoleksi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem is DataRowView row)
            {
                txtJenisKoleksi.Text = row["JenisKoleksi"].ToString();
                txtDeskripsi.Text = row["Deskripsi"].ToString();
                hiddenId.Text = row["KoleksiID"].ToString();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            string jenis = txtJenisKoleksi.Text.Trim();
            string deskripsi = txtDeskripsi.Text.Trim();

            if (string.IsNullOrWhiteSpace(jenis))
            {
                MessageBox.Show("Jenis Koleksi tidak boleh kosong.");
                return;
            }

            try
            {
                conn.Open();
                cmd = new SqlCommand("INSERT INTO Koleksi (JenisKoleksi, Deskripsi) VALUES (@jenis, @deskripsi)", conn);
                cmd.Parameters.AddWithValue("@jenis", jenis);
                cmd.Parameters.AddWithValue("@deskripsi", deskripsi);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Koleksi berhasil ditambahkan.");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambah data: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(hiddenId.Text))
            {
                MessageBox.Show("Pilih koleksi yang ingin diedit.");
                return;
            }

            string jenis = txtJenisKoleksi.Text.Trim();
            string deskripsi = txtDeskripsi.Text.Trim();
            int id = int.Parse(hiddenId.Text);

            try
            {
                conn.Open();
                cmd = new SqlCommand("UPDATE Koleksi SET JenisKoleksi = @jenis, Deskripsi = @deskripsi WHERE KoleksiID = @id", conn);
                cmd.Parameters.AddWithValue("@jenis", jenis);
                cmd.Parameters.AddWithValue("@deskripsi", deskripsi);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Koleksi berhasil diperbarui.");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message);
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(hiddenId.Text))
            {
                MessageBox.Show("Pilih koleksi yang ingin dihapus.");
                return;
            }

            int id = int.Parse(hiddenId.Text);
            var result = MessageBox.Show($"Yakin ingin menghapus koleksi dengan ID {id}?", "Konfirmasi", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                conn.Open();
                cmd = new SqlCommand("DELETE FROM Koleksi WHERE KoleksiID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Koleksi berhasil dihapus.");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message);
            }
        }
        private void ClearForm()
        {
            txtJenisKoleksi.Text = "";
            txtDeskripsi.Text = "";
            hiddenId.Text = "";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
        }
    }
}
