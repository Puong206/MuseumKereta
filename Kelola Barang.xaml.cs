using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
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
        private string selectedNamaBarang;
        private string selectedDeskripsi;
        private string selectedKoleksiId;
        private string selectedTahunPembuatan;
        private string selectedAsalBarang;

        public Kelola_Barang(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();
            // Menonaktifkan tombol Edit dan Hapus saat pertama kali dimuat
            BtnEdit.IsEnabled = false;
            BtnHapus.IsEnabled = false;
        }

        private void EnsureIndexes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Skrip untuk memastikan indeks ada untuk performa query yang lebih baik
                    var indexScript = @"
                    IF OBJECT_ID('dbo.BarangMuseum', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Barang_KoleksiID' AND object_id = OBJECT_ID('dbo.BarangMuseum'))
                            CREATE NONCLUSTERED INDEX idx_Barang_KoleksiID ON dbo.BarangMuseum(KoleksiID);
                    END";
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
            // Coba ambil data dari cache terlebih dahulu
            DataTable dt = _cache.Get(CacheKey) as DataTable;
            if (dt != null)
            {
                dataGridBarang.ItemsSource = dt.DefaultView;
            }
            else
            {
                // Jika tidak ada di cache, ambil dari database
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BarangMuseum", conn);
                        DataTable newDt = new DataTable();
                        adapter.Fill(newDt);
                        dataGridBarang.ItemsSource = newDt.DefaultView;

                        // Simpan data ke cache
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
                string barangID = dialog.BarangID.Trim();
                string namaBarang = dialog.NamaBarang.Trim();
                string deskripsi = dialog.Deskripsi.Trim();
                string tahunPembuatan = dialog.TahunPembuatan.Trim();
                string asalBarang = dialog.AsalBarang.Trim();
                string koleksiIdStr = dialog.KoleksiID.Trim();

                // Validasi Input
                Regex regex = new Regex("^[a-zA-Z0-9 ]+$");

                if (string.IsNullOrWhiteSpace(barangID) || string.IsNullOrWhiteSpace(namaBarang) || string.IsNullOrWhiteSpace(koleksiIdStr))
                {
                    CustomMessageBox.ShowWarning("BarangID, Nama Barang, dan KoleksiID wajib diisi.", "Input Kosong");
                    return;
                }
                if (barangID.Length != 5 || !barangID.All(char.IsDigit))
                {
                    CustomMessageBox.ShowWarning("BarangID harus terdiri dari 5 digit angka.", "Validasi Gagal");
                    return;
                }
                if (!regex.IsMatch(namaBarang)) { CustomMessageBox.ShowWarning("Nama Barang hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal"); return; }
                if (string.IsNullOrWhiteSpace(deskripsi)) { CustomMessageBox.ShowWarning("Deskripsi tidak boleh kosong.", "Validasi Gagal"); return; }
                if (!int.TryParse(koleksiIdStr, out int koleksiIdInt)) { CustomMessageBox.ShowWarning("KoleksiID harus berupa angka yang valid.", "Validasi Gagal"); return; }
                if (tahunPembuatan.Length != 4 || !int.TryParse(tahunPembuatan, out int tahunPembuatanInt)) { CustomMessageBox.ShowWarning("Tahun Pembuatan harus terdiri dari 4 digit angka.", "Validasi Gagal"); return; }
                if (tahunPembuatanInt > DateTime.Now.Year)
                {
                    CustomMessageBox.ShowWarning($"Tahun Pembuatan tidak boleh lebih dari tahun sekarang ({DateTime.Now.Year}).", "Tahun Tidak Valid");
                    return;
                }
                if (string.IsNullOrWhiteSpace(asalBarang) || !regex.IsMatch(asalBarang)) { CustomMessageBox.ShowWarning("Asal Barang harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal"); return; }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddBarang", conn)) // Pastikan nama Stored Procedure benar
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", barangID);
                            cmd.Parameters.AddWithValue("@NamaBarang", namaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", koleksiIdInt);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", tahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", asalBarang);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Barang berhasil ditambahkan");
                            _cache.Remove(CacheKey); // Hapus cache agar data baru dimuat
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627) // primary Key violation
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: BarangID '{barangID}' sudah terdaftar.", "Data Duplikat");
                    }
                    else if (sqlEx.Number == 547) // foreign Key atau check Constraint violation
                    {
                        if (sqlEx.Message.Contains("FK_"))
                        {
                            CustomMessageBox.ShowError($"Gagal menyimpan: KoleksiID '{dialog.KoleksiID}' tidak ditemukan.", "Referensi Salah");
                        }
                        else
                        {
                            CustomMessageBox.ShowError("Gagal menyimpan: Data yang dimasukkan tidak sesuai format yang ditentukan di database.", "Format Salah");
                        }
                    }
                    else if (sqlEx.Number == 50003) // error dari SP
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: KoleksiID '{dialog.KoleksiID}' tidak ditemukan.", "Referensi Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal menambah data: " + sqlEx.Message, "Kesalahan Database");
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan umum: " + ex.Message, "Kesalahan");
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Pastikan sebuah baris telah dipilih
            if (string.IsNullOrEmpty(selectedBarangId))
            {
                CustomMessageBox.ShowWarning("Pilih barang yang ingin diedit.", "Peringatan");
                return;
            }

            // Buka dialog edit dengan data yang sudah ada
            var dialog = new InputDialogBarang(selectedBarangId, selectedNamaBarang, selectedDeskripsi, selectedKoleksiId, selectedTahunPembuatan, selectedAsalBarang);

            if (dialog.ShowDialog() == true)
            {
                // Ambil nilai BARU dari dialog
                string namaBarang = dialog.NamaBarang.Trim();
                string deskripsi = dialog.Deskripsi.Trim();
                string tahunPembuatan = dialog.TahunPembuatan.Trim();
                string asalBarang = dialog.AsalBarang.Trim();
                string koleksiIdStr = dialog.KoleksiID.Trim();

                // Validasi input baru
                Regex regex = new Regex("^[a-zA-Z0-9 ]+$");
                if (string.IsNullOrWhiteSpace(namaBarang) || string.IsNullOrWhiteSpace(koleksiIdStr))
                {
                    CustomMessageBox.ShowWarning("Nama Barang dan KoleksiID wajib diisi.", "Input Kosong");
                    return;
                }
                if (!regex.IsMatch(namaBarang)) { CustomMessageBox.ShowWarning("Nama Barang hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal"); return; }
                if (string.IsNullOrWhiteSpace(deskripsi)) { CustomMessageBox.ShowWarning("Deskripsi tidak boleh kosong.", "Validasi Gagal"); return; }
                if (!int.TryParse(koleksiIdStr, out int koleksiIdInt)) { CustomMessageBox.ShowWarning("KoleksiID harus berupa angka yang valid.", "Validasi Gagal"); return; }
                if (tahunPembuatan.Length != 4 || !int.TryParse(tahunPembuatan, out int tahunPembuatanInt)) { CustomMessageBox.ShowWarning("Tahun Pembuatan harus terdiri dari 4 digit angka.", "Validasi Gagal"); return; }
                if (tahunPembuatanInt > DateTime.Now.Year)
                {
                    CustomMessageBox.ShowWarning($"Tahun Pembuatan tidak boleh lebih dari tahun sekarang ({DateTime.Now.Year}).", "Tahun Tidak Valid");
                    return;
                }
                if (string.IsNullOrWhiteSpace(asalBarang) || !regex.IsMatch(asalBarang)) { CustomMessageBox.ShowWarning("Asal Barang harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal"); return; }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Ganti "UpdateBarang" dengan nama Stored Procedure Anda yang sesuai
                        using (SqlCommand cmd = new SqlCommand("UpdateBarang", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", selectedBarangId); // ID barang yang akan diupdate
                            cmd.Parameters.AddWithValue("@NamaBarang", namaBarang);
                            cmd.Parameters.AddWithValue("@Deskripsi", deskripsi);
                            cmd.Parameters.AddWithValue("@KoleksiID", koleksiIdInt);
                            cmd.Parameters.AddWithValue("@TahunPembuatan", tahunPembuatan);
                            cmd.Parameters.AddWithValue("@AsalBarang", asalBarang);

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            CustomMessageBox.ShowSuccess("Barang berhasil diperbarui.", "Sukses");
                            _cache.Remove(CacheKey); // Hapus cache
                            LoadData();
                            LoadData();
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601) // Unique constraint violation
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: Jenis Koleksi '{selectedKoleksiId}' sudah terdaftar.", "Data Duplikat");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal memperbarui Koleksi: " + sqlEx.Message, "Kesalahan Database");
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga: " + ex.Message, "Kesalahan Umum");
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
                    _cache.Remove("PerawatanData");
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