using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using System.Text.RegularExpressions;

namespace MuseumApp
{
    public partial class Kelola_Pegawai : Page
    {
        private readonly string connectionString;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy()
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "KaryawanData";

        private string selectedNIPP;
        private string selectedNama;
        private string selectedStatus;

        public Kelola_Pegawai(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();
            // Nonaktifkan tombol Edit dan Hapus saat pertama kali dimuat
            BtnEditPegawai.IsEnabled = false;
        }

        private void EnsureIndexes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var indexScript = @"
                    IF OBJECT_ID('dbo.Karyawan', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_NamaKaryawan' AND object_id = OBJECT_ID('dbo.Karyawan'))
                            CREATE NONCLUSTERED INDEX idx_NamaKaryawan ON dbo.Karyawan(NamaKaryawan);
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
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT NIPP, NamaKaryawan, statusKaryawan FROM Karyawan", conn);
                        dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridPegawai.ItemsSource = dt.DefaultView;
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
                dataGridPegawai.ItemsSource = dt.DefaultView;
            }
        }

        private void dataGridPegawai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPegawai.SelectedItem is DataRowView row)
            {
                selectedNIPP = row["NIPP"]?.ToString();
                selectedNama = row["NamaKaryawan"]?.ToString();
                selectedStatus = row["statusKaryawan"]?.ToString();

                // Mengisi TextBox di UI (jika masih digunakan)
                txtNIPP.Text = selectedNIPP;
                txtNama.Text = selectedNama;
                txtNIPP.IsEnabled = false; // NIPP tidak boleh diubah dari sini

                bool isItemSelected = !string.IsNullOrEmpty(selectedNIPP);
                BtnEditPegawai.IsEnabled = isItemSelected;
            }
            else
            {
                selectedNIPP = null;
                selectedNama = null;
                selectedStatus = null;

                txtNIPP.Text = string.Empty;
                txtNama.Text = string.Empty;

                BtnEditPegawai.IsEnabled = false;
            }
        }

        private void BtnTambahPegawai_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPegawai();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                string nipp = dialog.NIPP;
                string nama = dialog.NamaKaryawan;
                string status = dialog.StatusKaryawan;

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddKaryawan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@NIPP", nipp);
                            cmd.Parameters.AddWithValue("@NamaKaryawan", nama);
                            cmd.Parameters.AddWithValue("@statuskaryawan", status);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Karyawan berhasil ditambahkan.");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }

                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627) // Primary Key violation
                    {
                        CustomMessageBox.ShowError($"Gagal menyimpan: NIPP '{nipp}' sudah terdaftar.", "Data Duplikat");
                    }
                    else if (sqlEx.Number == 547) // Check Constraint violation
                    {
                        CustomMessageBox.ShowError("Gagal menyimpan: Data yang dimasukkan tidak sesuai format yang ditentukan (misal: nama atau status tidak valid).", "Format Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal menambah data: " + sqlEx.Message, "Kesalahan Database");
                    }
                }

                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Gagal menambah data: " + ex.Message);
                }
            }
        }

        private void BtnEditPegawai_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedNIPP))
            {
                CustomMessageBox.ShowWarning("Pilih pegawai yang ingin diedit.", "Peringatan");
                return;
            }

            var dialog = new InputDialogPegawai(selectedNIPP, selectedNama, selectedStatus);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                string namaBaru = dialog.NamaKaryawan;
                string statusBaru = dialog.StatusKaryawan;

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateKaryawan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@NIPP", selectedNIPP);
                            cmd.Parameters.AddWithValue("@NamaKaryawan", namaBaru);
                            cmd.Parameters.AddWithValue("@statusKaryawan", statusBaru);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            CustomMessageBox.ShowSuccess("Data karyawan berhasil diperbarui.");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50007) 
                    {
                        CustomMessageBox.ShowError("Data pegawai tidak ditemukan: " + sqlEx.Message);
                    }
                    else if (sqlEx.Number == 547) // Check Constraint violation
                    {
                        CustomMessageBox.ShowError("Gagal menyimpan: Format Nama atau Status tidak valid.", "Format Salah");
                    }
                    else
                    {
                        CustomMessageBox.ShowError("Gagal memperbarui data: " + sqlEx.Message, "Kesalahan Database");
                    }
                    
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowError("Gagal memperbarui data: " + ex.Message);
                }
            }
        }


        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }

        private void AnalyzeQuery(string sqlQuery)
        {
            StringBuilder statisticResult = new StringBuilder();   

            using (var  conn = new SqlConnection(connectionString)) 
            {
                conn.InfoMessage += (s, e) =>
                {
                    statisticResult.AppendLine(e.Message);
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
                    CustomMessageBox.ShowError("error saat menganalisis query: " + ex.Message, "error analisis");
                    return;
                }

                if (statisticResult.Length > 0 ) 
                {
                    CustomMessageBox.ShowInfo(statisticResult.ToString(), "STATISTICS INFO");                   
                }
                else
                {
                    CustomMessageBox.ShowWarning("tidak ada informasi statistik yang diterima", "STATISTICS INFO");
                }
            }
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            string queryToAnalyze = "SELECT * FROM dbo.Karyawan WHERE NamaKaryawan = 'azril';";
            AnalyzeQuery(queryToAnalyze);

        }

        
    }
}
