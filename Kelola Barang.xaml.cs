using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class Kelola_Barang : Page
    {
        private readonly string connectionString;
        

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };

        private const string CacheKey = "BarangData";
        private string selectedBarangId;


        public Kelola_Barang(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();
           
        }

        private void EnsureIndexes()
        {
            try
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
            catch (Exception ex)
            {
                CustomMessageBox.ShowError("Gagal memastikan indeks: " + ex.Message, "Error Indeks");
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
                    CustomMessageBox.ShowWarning("Gagal memuat data: " + ex.Message);
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
                        CustomMessageBox.ShowWarning("Jenis Koleksi dan Deskripsi harus diisi!", "Peringatan");
                        return;
                    }

                    int KoleksiIdInt;
                    if (!int.TryParse(dialog.KoleksiID, out KoleksiIdInt))
                    {
                        CustomMessageBox.ShowWarning("KoleksiID harus berupa angka yang valid", "Validasi gagal");
                        return;
                    }

                    if (dialog.TahunPembuatan.Length != 4 || !dialog.TahunPembuatan.All(char.IsDigit))
                    {
                        CustomMessageBox.ShowWarning("Tahun Pembuatan harus terdiri dari tepat 4 digit angka.", "Validasi Gagal");
                        return;
                    }

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("Addbarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", dialog.BarangID);
                            cmd.Parameters.AddWithValue("@NamaBarang", dialog.NamaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", dialog.Deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", KoleksiIdInt);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", dialog.TahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", dialog.AsalBarang);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Barang berhasil ditabahkan");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowWarning("Gagal menambah data: " + ex.Message);
                }
                
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null)
            {
                CustomMessageBox.ShowWarning("Pilih data barang yang akan diedit");
                return;
            }
            string asalBarangID = row["BarangID"]?.ToString();
            if (string.IsNullOrWhiteSpace(asalBarangID))
            {
                CustomMessageBox.ShowError("BarangID data yang dipilih tidak valid", "Kesalahan Data");
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
                    CustomMessageBox.ShowWarning("Semua kolom harus diisi!", "Peringatan");
                    return;
                }



                if (dialog.TahunPembuatan.Length != 4 || !dialog.TahunPembuatan.All(char.IsDigit))
                {
                    CustomMessageBox.ShowWarning("Tahun Pembuatan harus terdiri dari tepat 4 digit angka.", "Validasi Gagal");
                    return;
                }

                int koleksiIdInt;
                if (string.IsNullOrWhiteSpace(dialog.KoleksiID) || !int.TryParse(dialog.KoleksiID, out koleksiIdInt))
                {
                    CustomMessageBox.ShowWarning("KoleksiID harus diisi dengan angka yang valid.", "Validasi Gagal");
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) 
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", selectedBarangId);
                            cmd.Parameters.AddWithValue("@NamaBarang", (object)dialog.NamaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", (object)dialog.Deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", koleksiIdInt);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", (object)dialog.TahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", (object)(dialog.AsalBarang));

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            CustomMessageBox.ShowSuccess("Data barang berhasil diperbaharui");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException SqlEx)
                {
                    if (SqlEx.Number == 50005)
                    {
                        CustomMessageBox.ShowError("Data barang tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid", "Kesalahan Update");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal menambah data" + SqlEx.Message, "Kesalahan database");
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga saat mengubah data: " + ex.Message, "Kesalahan Umum");
                }

            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null)
            {
                CustomMessageBox.ShowWarning("Pilih data barang yang akan dihapus.");
                return;
            }
            

            if (string.IsNullOrWhiteSpace(selectedBarangId))
            {
                CustomMessageBox.ShowError("BarangID dari data yang dipilih tidak valid.", "Kesalahan Data");
                return;
            }

            if (CustomMessageBox.ShowYesNo($"Yakin ingin menghapus BarangID {selectedBarangId}?", "Konfirmasi"))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("DeleteBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", selectedBarangId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CustomMessageBox.ShowSuccess("Barang berhasil dihapus!");
                    _cache.Remove(CacheKey);
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50006)
                    {
                        CustomMessageBox.ShowError("Data barang tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Hapus");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal menghapus data: " + sqlEx.Message, "Kesalahan Database");
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga saat menghapus data: " + ex.Message, "Kesalahan Umum");
                }
            }
        }

        private void dataGridBarang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridBarang.SelectedItem is DataRowView selectedRow)
            {
                selectedBarangId = selectedRow["BarangID"]?.ToString();
                bool isItemSelected = !string.IsNullOrEmpty(selectedBarangId);
                if (BtnEdit != null) BtnEdit.IsEnabled = isItemSelected;
                if (BtnHapus != null) BtnHapus.IsEnabled = isItemSelected;
            }
            else
            {
                selectedBarangId = null;
                if (BtnEdit != null) BtnEdit.IsEnabled = false;
                if (BtnHapus != null) BtnHapus.IsEnabled = false;   
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeQuery("SELECT * FROM BarangMuseum WHERE KoleksiID = 1;");
        }

        private void AnalyzeQuery(string query)
        {
            StringBuilder statisticsResult = new StringBuilder();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.InfoMessage += (s, e) => statisticsResult.AppendLine(e.Message);
                try
                {
                    conn.Open();
                    var wrappedQuery = $@"SET STATISTICS IO ON; SET STATISTICS TIME ON; {query}; SET STATISTICS IO OFF; SET STATISTICS TIME OFF;";
                    using (var cmd = new SqlCommand(wrappedQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowWarning("Error saat menganalisis kueri: " + ex.Message, "Error Analisis");
                    return;
                }
            }
            if (statisticsResult.Length > 0) CustomMessageBox.ShowInfo(statisticsResult.ToString(), "STATISTICS INFO");
            else CustomMessageBox.ShowWarning("Tidak ada informasi statistik yang diterima.", "STATISTICS INFO");
        }
    }
}