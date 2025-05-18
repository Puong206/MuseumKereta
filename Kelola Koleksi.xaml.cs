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
            using (SqlConnection conn = new SqlConnection(connectionString))
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

                txtJenisKoleksi.Text = selectedJenis;
                txtDeskripsi.Text = selectedDeskripsi;
                hiddenId.Text = selectedId.ToString();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("AddKoleksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@KoleksiID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@JenisKoleksi", dialog.JenisKoleksi);
                            cmd.Parameters.AddWithValue("@Deskripsi", dialog.Deskripsi);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Koleksi berhasil ditambahkan.");
                        }
                        
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal menambah data: " + ex.Message);
                    }
                }
                
                
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem == null)
            {
                MessageBox.Show("Pilih koleksi yang ingin diedit");
                return;
            }
            var dialog = new InputDialog();
            dialog.JenisTextBox.Text = txtJenisKoleksi.Text;
            dialog.DeskripsiTextBox.Text = txtDeskripsi.Text;

            if (dialog.ShowDialog() == true)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = ("UPDATE Koleksi SET JenisKoleksi = @jenis, Deskripsi = @deskripsi WHERE KoleksiID = @id");

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@jenis", dialog.JenisKoleksi);
                            cmd.Parameters.AddWithValue("@deskripsi", dialog.Deskripsi);
                            cmd.Parameters.AddWithValue("@id", int.Parse(hiddenId.Text));
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Koleksi berhasil diperbarui.");
                        }
                        LoadData();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memperbarui data: " + ex.Message);
                    }
                }
                
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem == null)
            {
                MessageBox.Show("Pilih koleksi yang ingin dihapus.");
                return;
            }

            var result = MessageBox.Show($"Yakin ingin menghapus koleksi dengan ID {selectedId}?", "Konfirmasi", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Koleksi WHERE KoleksiID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Koleksi berhasil dihapus.");
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
                }
            }
            
            
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
