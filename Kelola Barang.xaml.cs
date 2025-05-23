using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class Kelola_Barang : Page
    {
        private readonly string connectionString;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataAdapter adapter;
        private DataTable dt;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };

        private const string CacheKey = "BarangData";

        public Kelola_Barang(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();
           
        }

        private void EnsureIndexes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) 
            {
                conn.Open();

                var indexScript = @"
                    IF OBJECT_ID('dbo.BarangMuseum', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Barang_KoleksiID' AND object_id = OBJECT_ID('dbo.BarangMuseum'))
                        CREATE NONCLUSTERED INDEX idx_Barang_KoleksiID ON dbo.BarangMuseum(KoleksiID);
                END
                ";
                using (SqlCommand cmd = new SqlCommand(indexScript, conn)) 
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadData()
        {
            DataTable dt = _cache.Get(CacheKey) as DataTable;
            if (dt != null) 
            {
                dataGridBarang.ItemsSource = dt.DefaultView;

            }
            else
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {

                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BarangMuseum", conn);
                        DataTable newDt = new DataTable();
                        adapter.Fill(newDt);
                        dataGridBarang.ItemsSource = newDt.DefaultView;
                        _cache.Set(CacheKey, newDt, _policy);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data: " + ex.Message);
                }
            }


        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogBarang();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dialog.BarangID) || string.IsNullOrWhiteSpace(dialog.NamaBarang) || string.IsNullOrWhiteSpace(dialog.Deskripsi) || string.IsNullOrWhiteSpace(dialog.KoleksiID) || string.IsNullOrWhiteSpace(dialog.TahunPembuatan) || string.IsNullOrWhiteSpace(dialog.AsalBarang))
                    {
                        MessageBox.Show("Jenis Koleksi dan Deskripsi harus diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int KoleksiIdInt;
                    if (!int.TryParse(dialog.KoleksiID, out KoleksiIdInt))
                    {
                        MessageBox.Show("KoleksiID harus berupa angka yang valid", "Validasi gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (dialog.TahunPembuatan.Length != 4 || !dialog.TahunPembuatan.All(char.IsDigit))
                    {
                        MessageBox.Show("Tahun Pembuatan harus terdiri dari tepat 4 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

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
                            _cache.Remove(CacheKey);
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
            string asalBarangID = row["BarangID"]?.ToString();
            if (string.IsNullOrWhiteSpace(asalBarangID))
            {
                MessageBox.Show("BarangID data yang dipilih tidak valid", "Kesalahan Data", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (string.IsNullOrWhiteSpace(dialog.NamaBarang) || string.IsNullOrWhiteSpace(dialog.Deskripsi) ||
                    string.IsNullOrWhiteSpace(dialog.KoleksiID) || string.IsNullOrWhiteSpace(dialog.TahunPembuatan) ||
                    string.IsNullOrWhiteSpace(dialog.AsalBarang))
                {
                    MessageBox.Show("Semua kolom harus diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }



                if (dialog.TahunPembuatan.Length != 4 || !dialog.TahunPembuatan.All(char.IsDigit))
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
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException SqlEx)
                {
                    if (SqlEx.Number == 50003)
                    {
                        MessageBox.Show("Data barang tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid", "Kesalahan Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Gagal menambah data" + SqlEx.Message, "Kesalahan database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat mengubah data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (string.IsNullOrWhiteSpace(barangIdToDelete))
            {
                MessageBox.Show("BarangID dari data yang dipilih tidak valid.", "Kesalahan Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
                    MessageBox.Show("Barang berhasil dihapus!");
                    _cache.Remove(CacheKey);
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50004)
                    {
                        MessageBox.Show("Data barang tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Hapus", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Gagal menghapus data: " + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat menghapus data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
