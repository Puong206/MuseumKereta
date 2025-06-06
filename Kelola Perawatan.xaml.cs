using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;


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
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dt;
        

        public Kelola_Perawatan(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            conn = new SqlConnection(connectionString);
            EnsureIndexes();
            LoadData();
        }

        private void EnsureIndexes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) 
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT('dbo.Perawatan', 'U') IS NOT NULL
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
                        MessageBox.Show("Gagal memuat data: " + ex.Message);
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
                    MessageBox.Show("Kesalahan", "error data", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (string.IsNullOrWhiteSpace(dialog.IDBarang) || dialog.IDBarang.Length != 5 || !dialog.IDBarang.All(char.IsDigit))
                {
                    MessageBox.Show("BarangID harus terdiri dari tepat 5 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dialog.NIPP) || dialog.NIPP.Length != 5 || !dialog.NIPP.All(char.IsDigit))
                {
                    MessageBox.Show("NIPP harus terdiri dari tepat 5 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dialog.TanggalPerawatan == default(DateTime))
                {
                    MessageBox.Show("Tanggal Perawatan harus diisi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dialog.JenisPerawatan))
                {
                    MessageBox.Show("Jenis Perawatan harus diisi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

                            MessageBox.Show("Data berhasil diperbarui.");
                            _cache.Remove(CacheKey);
                        }
                    }


                    LoadData();
                }
                catch (SqlException sqlEx) 
                {
                    if (sqlEx.Number == 50009) 
                    {
                        MessageBox.Show("BarangID tidak ditemukan di database. Pastikan BarangID valid.", "Kesalahan Tambah", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (sqlEx.Number == 50010) 
                    {
                        MessageBox.Show("NIPP tidak ditemukan di database. Pastikan NIPP karyawan valid.", "Kesalahan Tambah", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Gagal menambah data: " + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat menambah data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan diedit.");
                return;
            }

            if (selectedPerawatanId <= 0)
            {
                MessageBox.Show("ID perawatan tidak valid", "kesalahan ID", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                            MessageBox.Show("Data berhasil diperbarui.");
                            _cache.Remove(CacheKey);
                        }
                    }

                    
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50011)
                    {
                        MessageBox.Show("BarangID tidak ditemukan di database. Pastikan BarangID valid.", "Kesalahan Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (sqlEx.Number == 50012)
                    {
                        MessageBox.Show("NIPP tidak ditemukan di database. Pastikan NIPP karyawan valid.", "Kesalahan Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (sqlEx.Number == 50013)
                    {
                        MessageBox.Show("Data perawatan tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memperbarui data: " + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat memperbarui data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan dihapus.");
                return;
            }

            if (selectedPerawatanId <= 0)
            {
                MessageBox.Show("ID perawatan tidak valid. Silakan pilih baris yang benar.", "Kesalahan ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            

            if (MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                            MessageBox.Show("Data berhasil dihapus.");
                            _cache.Remove(CacheKey);
                        }
                    }

                    
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50006)
                    {
                        MessageBox.Show("Data perawatan tidak ditemukan. Mungkin sudah dihapus atau ID tidak valid.", "Kesalahan Hapus", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
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
                    MessageBox.Show("error saat menganalisis query" + ex.Message, "error analisis", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


            }

            if (statisticsResult.Length > 0)
            {
                MessageBox.Show(statisticsResult.ToString(), "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("tidak ada informasi statistik yang diterima", "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            string queryToAnalyze = "SELECT * FROM Perawatan";
            AnalyzeQuery(queryToAnalyze);
        }
    }
}
