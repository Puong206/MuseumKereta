using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;


namespace MuseumApp
{
    public partial class Kelola_Perawatan : Page
    {
        private readonly string connectionString;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy()
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "PerawatanData";

        // Variabel untuk menyimpan data dari baris yang dipilih
        private int selectedPerawatanId;
        private string selectedBarangId;
        private DateTime selectedTanggalPerawatan;
        private string selectedJenisPerawatan;
        private string selectedCatatan;
        private string selectedNipp;

        public Kelola_Perawatan(string connStr)
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
                    IF OBJECT_ID('dbo.Perawatan', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_TanggalPerawatan' AND object_id = OBJECT_ID('dbo.Perawatan'))
                            CREATE NONCLUSTERED INDEX idx_TanggalPerawatan ON dbo.Perawatan(TanggalPerawatan);
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
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Perawatan", conn);
                        dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridPerawatan.ItemsSource = dt.DefaultView;
                        _cache.Set(CacheKey, dt, _policy);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Gagal memuat data: " + ex.Message);
                }
            }
            else
            {
                dataGridPerawatan.ItemsSource = dt.DefaultView;
            }
        }

        private void dataGridPerawatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPerawatan.SelectedItem is DataRowView row)
            {
                // Ambil semua data dari baris yang dipilih
                int.TryParse(row["PerawatanID"]?.ToString(), out selectedPerawatanId);
                selectedBarangId = row["BarangID"]?.ToString();
                DateTime.TryParse(row["TanggalPerawatan"]?.ToString(), out selectedTanggalPerawatan);
                selectedJenisPerawatan = row["JenisPerawatan"]?.ToString();
                selectedCatatan = row["Catatan"]?.ToString();
                selectedNipp = row["NIPP"]?.ToString();

                // Aktifkan tombol jika item valid dipilih
                bool isItemSelected = selectedPerawatanId > 0;
                BtnEdit.IsEnabled = isItemSelected;
                BtnHapus.IsEnabled = isItemSelected;
            }
            else
            {
                // Reset state dan nonaktifkan tombol jika tidak ada yang dipilih
                selectedPerawatanId = 0;
                BtnEdit.IsEnabled = false;
                BtnHapus.IsEnabled = false;
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPerawatan();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddPerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", dialog.IDBarang);
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", dialog.Catatan);
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);

                            conn.Open();
                            cmd.ExecuteNonQuery();

                            CustomMessageBox.ShowSuccess("Data perawatan berhasil ditambahkan.");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx) 
                {
                    if (sqlEx.Number == 50009) { CustomMessageBox.ShowError("BarangID tidak ditemukan di database.", "Referensi Salah"); }
                    else if (sqlEx.Number == 50010) { CustomMessageBox.ShowError("NIPP tidak ditemukan di database.", "Referensi Salah"); }
                    else if (sqlEx.Number == 547) { CustomMessageBox.ShowError("Gagal menyimpan: Format Jenis Perawatan tidak valid.", "Format Salah"); }
                    else { CustomMessageBox.ShowError("Gagal menambah data: " + sqlEx.Message, "Kesalahan Database"); }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga saat menambah data: " + ex.Message, "Kesalahan Umum");
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPerawatanId <= 0)
            {
                CustomMessageBox.ShowWarning("Pilih data perawatan yang akan diedit.", "Peringatan");
                return;
            }

            var dialog = new InputDialogPerawatan(
                selectedBarangId,
                selectedTanggalPerawatan,
                selectedJenisPerawatan,
                selectedCatatan,
                selectedNipp
            );

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdatePerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PerawatanID", selectedPerawatanId);
                            cmd.Parameters.AddWithValue("@BarangID", dialog.IDBarang);
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", dialog.Catatan);
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Data perawatan berhasil diperbarui.");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50011)
                    {
                        CustomMessageBox.ShowError("BarangID tidak ditemukan di database. Pastikan BarangID valid.", "Kesalahan Update");
                    }
                    else if (sqlEx.Number == 50012)
                    {
                        CustomMessageBox.ShowError("NIPP tidak ditemukan di database. Pastikan NIPP karyawan valid.", "Kesalahan Update");
                    }
                    else if (sqlEx.Number == 50013)
                    {
                        CustomMessageBox.ShowError("Data perawatan tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Update");
                    }
                    else if (sqlEx.Number == 547)
                    {
                        CustomMessageBox.ShowError("Gagal menyimpan: Format Jenis Perawatan tidak valid.", "Format Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal memperbarui data: " + sqlEx.Message, "Kesalahan Database");
                    }
                }
                catch (Exception ex) 
                {
                    CustomMessageBox.ShowError("Terjadi kesalahan tak terduga saat memperbarui data: " + ex.Message, "Kesalahan Umum");
                }
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPerawatanId <= 0)
            {
                CustomMessageBox.ShowWarning("Pilih data perawatan yang akan dihapus.", "Peringatan");
                return;
            }

            if (CustomMessageBox.ShowYesNo($"Yakin ingin menghapus data perawatan ID {selectedPerawatanId}?", "Konfirmasi Hapus"))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("DeletePerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PerawatanID", selectedPerawatanId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Data berhasil dihapus.");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50006)
                    {
                        CustomMessageBox.ShowError("Data perawatan tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Hapus");
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

        private void AnalyzeQuery(string sqlQuery)
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
                    var wrappedQuery =$@"
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
                    CustomMessageBox.ShowError("Error saat menganalisis query" + ex.Message, "Error Analisis");
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
            string queryToAnalyze = "SELECT * FROM dbo.Perawatan WHERE TanggalPerawatan BETWEEN '2025-01-01' AND '2025-06-30';";
            AnalyzeQuery(queryToAnalyze);
        }

        

    }


}
