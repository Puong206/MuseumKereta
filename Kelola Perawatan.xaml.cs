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
        private int selectedPerawatanId;
        //SqlConnection conn;
        //SqlCommand cmd;
        SqlDataAdapter adapter;
        //DataTable dt;
        

        public Kelola_Perawatan(string connStr)
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

        private void LoadData()
        {
            DataTable dt = _cache.Get(CacheKey) as DataTable;

            if (dt == null)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            adapter = new SqlDataAdapter("SELECT * FROM Perawatan", conn);
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
                
                
            }
            else
            {
                dataGridPerawatan.ItemsSource = dt.DefaultView;
            }
        }

        private void dataGridPerawatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPerawatan.SelectedItem is DataRowView selectedRow)
            {
                if (int.TryParse(selectedRow["PerawatanID"]?.ToString(), out int id))
                {
                    selectedPerawatanId = id;
                }
                else
                {
                    CustomMessageBox.ShowError("Kesalahan", "Error Data");
                    selectedPerawatanId = 0;
                }
                if (BtnEdit !=null)
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

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPerawatan();
            if (dialog.ShowDialog() == true)
            {
                string idBarang = dialog.IDBarang.Trim();
                string nipp = dialog.NIPP.Trim();
                string jenisPerawatan = dialog.JenisPerawatan.Trim();
                Regex regex = new Regex("^[a-zA-Z0-9 ]+$");

                if (idBarang.Length != 5 || !idBarang.All(char.IsDigit)) { CustomMessageBox.ShowWarning("BarangID harus terdiri dari 5 digit angka.", "Validasi Gagal"); return; }
                if (nipp.Length != 5 || !nipp.All(char.IsDigit)) { CustomMessageBox.ShowWarning("NIPP harus terdiri dari 5 digit angka.", "Validasi Gagal"); return; }
                if (dialog.TanggalPerawatan == default(DateTime)) { CustomMessageBox.ShowWarning("Tanggal Perawatan harus diisi.", "Validasi Gagal"); return; }
                if (string.IsNullOrWhiteSpace(jenisPerawatan) || !regex.IsMatch(jenisPerawatan)) { CustomMessageBox.ShowWarning("Jenis Perawatan harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal"); return; }


                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddPerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", (object)dialog.IDBarang);
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", (object)dialog.Catatan);
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);

                            SqlParameter outputIdParam = new SqlParameter("@PerawatanIDIdentity", SqlDbType.Int);
                            outputIdParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputIdParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            int generatedID = (int)outputIdParam.Value;

                            CustomMessageBox.ShowSuccess("Data berhasil ditambah.");
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
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                CustomMessageBox.ShowWarning("Pilih data yang akan diedit.");
                return;
            }

            if (selectedPerawatanId <= 0)
            {
                CustomMessageBox.ShowWarning("ID perawatan tidak valid", "kesalahan ID");
                return;
            }

            int perawatanId = Convert.ToInt32(row["PerawatanID"]);
            var dialog = new InputDialogPerawatan(
                row["BarangID"].ToString(),
                Convert.ToDateTime(row["TanggalPerawatan"]),
                row["JenisPerawatan"].ToString(),
                row["Catatan"].ToString(),
                row["NIPP"].ToString()
            );

            if (dialog.ShowDialog() == true)
            {

                string idBarang = dialog.IDBarang.Trim();
                string nipp = dialog.NIPP.Trim();
                string jenisPerawatan = dialog.JenisPerawatan.Trim();
                Regex regex = new Regex("^[a-zA-Z0-9 ]+$");

                if (idBarang.Length != 5 || !idBarang.All(char.IsDigit))
                {
                    CustomMessageBox.ShowWarning("BarangID harus terdiri dari 5 digit angka.", "Validasi Gagal");
                    return;
                }
                if (nipp.Length != 5 || !nipp.All(char.IsDigit))
                {
                    CustomMessageBox.ShowWarning("NIPP harus terdiri dari 5 digit angka.", "Validasi Gagal");
                    return;
                }
                if (dialog.TanggalPerawatan == default(DateTime))
                {
                    CustomMessageBox.ShowWarning("Tanggal Perawatan harus diisi.", "Validasi Gagal");
                    return;
                }
                if (string.IsNullOrWhiteSpace(jenisPerawatan) || !regex.IsMatch(jenisPerawatan))
                {
                    CustomMessageBox.ShowWarning("Jenis Perawatan harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal");
                    return;
                }



                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) 
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdatePerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PerawatanID", perawatanId);
                            cmd.Parameters.AddWithValue("@BarangID", (object)dialog.IDBarang); 
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", (object)dialog.Catatan); 
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);


                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Data berhasil diperbarui.");
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
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                CustomMessageBox.ShowWarning("Pilih data yang akan dihapus.");
                return;
            }

            if (selectedPerawatanId <= 0)
            {
                CustomMessageBox.ShowWarning("ID perawatan tidak valid. Silakan pilih baris yang benar.", "Kesalahan ID");
                return;
            }

            

            if (CustomMessageBox.ShowYesNo("Yakin ingin menghapus data ini?", "Konfirmasi"))
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
