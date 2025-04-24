using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
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

        public Kelola_Koleksi(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
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

        private int selectedId;
        private string selectedJenis;
        private string selectedDeskripsi;

        private void dataGridKoleksi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem is DataRowView row)
            {
                selectedJenis = row["JenisKoleksi"].ToString();
                selectedDeskripsi = row["Deskripsi"].ToString();
                selectedId = Convert.ToInt32(row["KoleksiID"]);
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    conn.Open();
                    cmd = new SqlCommand("INSERT INTO Koleksi (JenisKoleksi, Deskripsi) VALUES (@jenis, @deskripsi)", conn);
                    cmd.Parameters.AddWithValue("@jenis", dialog.JenisKoleksi);
                    cmd.Parameters.AddWithValue("@deskripsi", dialog.Deskripsi);
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
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(hiddenId.Text))
            //{
                //MessageBox.Show("Pilih koleksi yang ingin diedit.");
                //return;
            //}

            var dialog = new InputDialog();
            dialog.JenisTextBox.Text = txtJenisKoleksi.Text;
            dialog.DeskripsiTextBox.Text = txtDeskripsi.Text;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    conn.Open();
                    cmd = new SqlCommand("UPDATE Koleksi SET JenisKoleksi = @jenis, Deskripsi = @deskripsi WHERE KoleksiID = @id", conn);
                    cmd.Parameters.AddWithValue("@jenis", dialog.JenisKoleksi);
                    cmd.Parameters.AddWithValue("@deskripsi", dialog.Deskripsi);
                    cmd.Parameters.AddWithValue("@id", int.Parse(hiddenId.Text));
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
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
