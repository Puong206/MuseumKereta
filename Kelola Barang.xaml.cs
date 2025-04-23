using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class Kelola_Barang : Page
    {
        private readonly string connectionString;
        private SqlConnection conn;

        public Kelola_Barang(string connStr)
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
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BarangMuseum", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridBarang.ItemsSource = dt.DefaultView;
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

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogBarang();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO BarangMuseum (BarangID, NamaBarang, Deskripsi, KoleksiID, TahunPembuatan, AsalBarang) 
                        VALUES (@id, @nama, @deskripsi, @koleksiID, @tahun, @asal)", conn);
                    cmd.Parameters.AddWithValue("@id", dialog.BarangID);
                    cmd.Parameters.AddWithValue("@nama", dialog.NamaBarang);
                    cmd.Parameters.AddWithValue("@deskripsi", dialog.Deskripsi);
                    if (string.IsNullOrWhiteSpace(dialog.KoleksiID))
                    {
                        cmd.Parameters.AddWithValue("@koleksiID", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@koleksiID", dialog.KoleksiID);
                    }
                    cmd.Parameters.AddWithValue("@tahun", dialog.TahunPembuatan);
                    cmd.Parameters.AddWithValue("@asal", dialog.AsalBarang);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambah data: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null) return;

            var dialog = new InputDialogBarang(
                row["BarangID"].ToString(),
                row["NamaBarang"].ToString(),
                row["Deskripsi"].ToString(),
                row["KoleksiID"] == DBNull.Value ? "" : row["KoleksiID"].ToString(),
                row["TahunPembuatan"].ToString(),
                row["AsalBarang"].ToString());

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE BarangMuseum 
                        SET NamaBarang = @nama, Deskripsi = @deskripsi, KoleksiID = @koleksiID, 
                            TahunPembuatan = @tahun, AsalBarang = @asal 
                        WHERE BarangID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", dialog.BarangID);
                    cmd.Parameters.AddWithValue("@nama", dialog.NamaBarang);
                    cmd.Parameters.AddWithValue("@deskripsi", dialog.Deskripsi);
                    if (string.IsNullOrWhiteSpace(dialog.KoleksiID))
                    {
                        cmd.Parameters.AddWithValue("@koleksiID", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@koleksiID", dialog.KoleksiID);
                    }
                    cmd.Parameters.AddWithValue("@tahun", dialog.TahunPembuatan);
                    cmd.Parameters.AddWithValue("@asal", dialog.AsalBarang);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengubah data: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                }
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null) return;

            string id = row["BarangID"].ToString();
            if (MessageBox.Show($"Yakin ingin menghapus BarangID {id}?", "Konfirmasi", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM BarangMuseum WHERE BarangID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                }
            }
        }

        private void dataGridBarang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
