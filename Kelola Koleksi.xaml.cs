using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for Kelola_Koleksi.xaml
    /// </summary>
    public partial class Kelola_Koleksi : Page
    {
        private string connectionString;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataAdapter adapter;
        private DataTable dt;

        private TextBox txtJenisKoleksi = new TextBox();
        private TextBox txtDeskripsi = new TextBox();
        private TextBox hiddenId = new TextBox();
        // Dummy controls yang tidak ada di XAML

        public Kelola_Koleksi()
        {
            InitializeComponent();
            connectionString = connectionStr;
            conn = new SqlConnection(connectionString);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
            finally
            {
                conn.Close();
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
            string jenis = Microsoft.VisualBasic.Interaction.InputBox("Masukkan Jenis Koleksi:", "Tambah Koleksi", "");
            string deskripsi = Microsoft.VisualBasic.Interaction.InputBox("Masukkan Deskripsi:", "Tambah Koleksi", "");

            if (string.IsNullOrWhiteSpace(jenis)) return;

            try
            {
                conn.Open();
                cmd = new SqlCommand("INSERT INTO Koleksi (JenisKoleksi, Deskripsi) VALUES (@jenis, @deskripsi)", conn);
                cmd.Parameters.AddWithValue("@jenis", jenis);
                cmd.Parameters.AddWithValue("@deskripsi", deskripsi);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Koleksi berhasil ditambahkan.");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambah data: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(hiddenId.Text))
            {
                MessageBox.Show("Pilih koleksi yang ingin diedit.");
                return;
            }

            string jenis = Microsoft.VisualBasic.Interaction.InputBox("Edit Jenis Koleksi:", "Edit Koleksi", txtJenisKoleksi.Text);
            string deskripsi = Microsoft.VisualBasic.Interaction.InputBox("Edit Deskripsi:", "Edit Koleksi", txtDeskripsi.Text);
            int id = int.Parse(hiddenId.Text);

            try
            {
                conn.Open();
                cmd = new SqlCommand("UPDATE Koleksi SET JenisKoleksi = @jenis, Deskripsi = @deskripsi WHERE KoleksiID = @id", conn);
                cmd.Parameters.AddWithValue("@jenis", jenis);
                cmd.Parameters.AddWithValue("@deskripsi", deskripsi);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Koleksi berhasil diperbarui.");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message);
            }
            finally
            {
                conn.Close();
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
                MessageBox.Show("Koleksi berhasil dihapus.");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GantiKonten(new Page1());
        }
    }
}
