using System.IO;

using Microsoft.Win32;

using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Caching;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace MuseumApp
{
    public partial class Kelola_Koleksi : Page
    {
        private readonly string connectionString;
        
        private SqlDataAdapter adapter;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "KoleksiData";

        // Variabel untuk menyimpan data dari baris yang dipilih
        private int selectedId;
        private string selectedJenis;
        private string selectedDeskripsi;


        public Kelola_Koleksi(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();
            // Nonaktifkan tombol saat pertama kali dimuat
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
                    var indexScript = @"
                    IF OBJECT_ID('dbo.Koleksi', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_JenisKoleksi' AND object_id = OBJECT_ID('dbo.Koleksi'))
                            CREATE UNIQUE NONCLUSTERED INDEX idx_JenisKoleksi ON dbo.Koleksi(JenisKoleksi);
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
            DataTable dt = _cache.Get(CacheKey) as DataTable;
            if (dt == null)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {

                        conn.Open();
                        adapter = new SqlDataAdapter("SELECT KoleksiID, JenisKoleksi, Deskripsi FROM Koleksi", conn);
                        dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridKoleksi.ItemsSource = dt.DefaultView;
                        _cache.Set(CacheKey, dt, _policy);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowWarning("Gagal memuat data: " + ex.Message);
                }
                
            }
            else
            {
                dataGridKoleksi.ItemsSource = dt.DefaultView;
            }
        }

        private void dataGridKoleksi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem is DataRowView row)
            {
                // Ambil data dari baris yang dipilih
                selectedJenis = row["JenisKoleksi"].ToString();
                selectedDeskripsi = row["Deskripsi"].ToString();
                int.TryParse(row["KoleksiID"]?.ToString(), out selectedId);

                // Aktifkan tombol jika item valid dipilih
                bool isItemSelected = selectedId > 0;
                if (BtnEdit != null) BtnEdit.IsEnabled = isItemSelected;
                if (BtnHapus != null) BtnHapus.IsEnabled = isItemSelected;
            }
            else
            {
                // Reset state dan nonaktifkan tombol jika tidak ada yang dipilih
                selectedId = 0;
                if (BtnEdit != null) BtnEdit.IsEnabled = false;
                if (BtnHapus != null) BtnHapus.IsEnabled = false;
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true)
            {
                string jenisKoleksi = dialog.JenisKoleksi; // Ambil dari property, bukan textbox
                string deskripsi = dialog.Deskripsi;

                // Validasi sudah dilakukan di dalam dialog, namun tetap baik untuk double-check
                if (string.IsNullOrWhiteSpace(jenisKoleksi) || string.IsNullOrWhiteSpace(deskripsi))
                {
                    CustomMessageBox.ShowWarning("Data tidak boleh kosong.", "Peringatan");
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddKoleksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@JenisKoleksi", jenisKoleksi);
                            cmd.Parameters.AddWithValue("@Deskripsi", deskripsi);

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            CustomMessageBox.ShowSuccess("Koleksi berhasil ditambahkan");
                            _cache.Remove(CacheKey); // Hapus cache agar data baru dimuat
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601)
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: Jenis Koleksi '{jenisKoleksi}' sudah terdaftar.", "Data Duplikat");
                    }
                    else if (sqlEx.Number == 547)
                    {
                        CustomMessageBox.ShowError("Gagal menyimpan: Format Jenis Koleksi tidak valid. Hanya boleh huruf, angka, dan spasi.", "Format Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal menambah Koleksi: " + sqlEx.Message, "Kesalahan Database");
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
            if (selectedId <= 0)
            {
                CustomMessageBox.ShowWarning("Pilih koleksi yang valid untuk diedit.", "Peringatan");
                return;
            }

            // Panggil dialog dengan mengirim data yang sudah ada. Ini cara yang benar.
            var dialog = new InputDialog(selectedJenis, selectedDeskripsi);

            if (dialog.ShowDialog() == true)
            {
                string jenisKoleksi = dialog.JenisKoleksi;
                string deskripsi = dialog.Deskripsi;

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateKoleksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@KoleksiID", selectedId);
                            cmd.Parameters.AddWithValue("@JenisKoleksi", jenisKoleksi);
                            cmd.Parameters.AddWithValue("@Deskripsi", deskripsi);

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            CustomMessageBox.ShowSuccess("Koleksi berhasil diperbarui.", "Sukses");
                            _cache.Remove(CacheKey);
                            _cache.Remove("BarangData"); // Hapus juga cache barang jika ada keterkaitan
                            LoadData();
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50001)
                    {
                        CustomMessageBox.ShowError("Data koleksi tidak ditemukan", "Kesalahan Update");
                    }
                    else if (sqlEx.Number == 2601) // Error UNIQUE constraint
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: Jenis Koleksi '{jenisKoleksi}' sudah terdaftar.", "Data Duplikat");
                    }
                    else if (sqlEx.Number == 547) // Error CHECK constraint
                    {
                        CustomMessageBox.ShowError("Gagal menyimpan: Format Jenis Koleksi tidak valid.", "Format Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal memperbaharui Koleksi" + sqlEx.Message, "Kesalahan Database");
                    }
                }
                catch (Exception ex) 
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga saat memperbaharui Koleksi: " + ex.Message, "Kesalahan Umum");
                }


            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId <= 0)
            {
                CustomMessageBox.ShowWarning("Pilih koleksi yang ingin dihapus.", "Peringatan");
                return;
            }

            if (CustomMessageBox.ShowYesNo($"Yakin ingin menghapus koleksi '{selectedJenis}' (ID: {selectedId})?", "Konfirmasi Hapus"))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("DeleteKoleksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@KoleksiID", selectedId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Koleksi berhasil dihapus.", "Sukses");

                            _cache.Remove(CacheKey);
                            _cache.Remove("BarangData");
                            LoadData();
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50002)
                    {
                        CustomMessageBox.ShowError("Data koleksi tidak ditemukan.", "Kesalahan Hapus");
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }

        private void AnalyzeQuery (string sqlQuery)
        {
            StringBuilder statisticsResult = new StringBuilder();

            using (var conn = new SqlConnection(connectionString)) 
            {
                conn.InfoMessage += (s, e) =>
                {
                    statisticsResult.AppendLine(e.Message);
                };

                try
                {
                    conn.Open();
                    var wrappedQuery = $@"
                    SET STATISTICS IO ON;
                    SET STATISTICS TIME ON;
                    {sqlQuery};
                    SET STATISTICS IO OFF;
                    SET STATISTICS TIME OFF;";
                    using (var cmd = new SqlCommand(wrappedQuery, conn)) 
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("error saat enganalisis query" + ex.Message, "error analisis");
                    return;
                }
            }

            if (statisticsResult.Length > 0)
            {
                CustomMessageBox.ShowInfo(statisticsResult.ToString(), "STATISTICS INFO");
            }
            else
            {
                CustomMessageBox.ShowWarning("tidak ada informasi statistik yang diterima", "STATISTICS INFO");
            }
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            string queryToAnalyze = "SELECT * FROM Koleksi WHERE JenisKoleksi = 'tiket';";
            AnalyzeQuery(queryToAnalyze);
        }
    }
} 
