using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", dialog.BarangID);
                            cmd.Parameters.AddWithValue("@NamaBarang", dialog.NamaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", dialog.Deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", dialog.KoleksiID);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", dialog.TahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", dialog.AsalBarang);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Barang berhasil ditabahkan");
                        }
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambah data: " + ex.Message);
                }
                
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Pilih data barang yang akan diedit");
                return;
            }

            var dialog = new InputDialogBarang(
                row["BarangID"].ToString(),
                row["NamaBarang"].ToString(),
                row["Deskripsi"].ToString(),
                row["KoleksiID"].ToString(),
                row["TahunPembuatan"].ToString(),
                row["AsalBarang"].ToString()) ;

            if (dialog.ShowDialog() == true)
            {

                if (string.IsNullOrWhiteSpace(dialog.TahunPembuatan) || dialog.TahunPembuatan.Length != 4 || !dialog.TahunPembuatan.All(char.IsDigit))
                {
                    MessageBox.Show("Tahun Pembuatan harus terdiri dari tepat 4 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int koleksiIdInt;
                if (string.IsNullOrWhiteSpace(dialog.KoleksiID) || !int.TryParse(dialog.KoleksiID, out koleksiIdInt))
                {
                    MessageBox.Show("KoleksiID harus diisi dengan angka yang valid.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) 
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", row["BarangID"].ToString());
                            cmd.Parameters.AddWithValue("@NamaBarang", (object)dialog.NamaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", (object)dialog.Deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", koleksiIdInt);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", (object)dialog.TahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", (object)(dialog.AsalBarang));

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Data barang berhasil diperbaharui");
                        }
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengubah data: " + ex.Message);
                }
                
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Pilih data barang yang akan dihapus.");
                return;
            }
            string barangIdToDelete = row["BarangID"].ToString();

            if (MessageBox.Show($"Yakin ingin menghapus BarangID {barangIdToDelete}?", "Konfirmasi", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("DeleteBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", barangIdToDelete);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadData();
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
            if (dataGridBarang.SelectedItem is DataRowView)
            {
                if (BtnEdit != null)
                {
                    BtnEdit.IsEnabled = true;
                }
                if (BtnHapus != null)
                {
                    BtnHapus.IsEnabled = true;
                }
            }
            else
            {
                if (BtnEdit != null)
                {
                    BtnEdit.IsEnabled = false;
                }
                if (BtnHapus != null)
                {
                    BtnHapus.IsEnabled = false;
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
