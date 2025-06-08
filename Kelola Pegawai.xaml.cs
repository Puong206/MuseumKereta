using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
        //private SqlConnection conn;
        private SqlDataAdapter adapter;
        //private DataTable dt;
        public Kelola_Pegawai(string connStr)
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
                IF OBJECT_ID('dbo.Karyawan', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_NamaKaryawan' AND object_id = OBJECT_ID('dbo.Karyawan'))
                        CREATE NONCLUSTERED INDEX idx_NamaKaryawan ON  dbo.Karyawan(NamaKaryawan);
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
                        conn.Open();

                        adapter = new SqlDataAdapter("SELECT NIPP, NamaKaryawan, statusKaryawan FROM Karyawan", conn);

                        dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridPegawai.ItemsSource = dt.DefaultView;
                        _cache.Set(CacheKey, dt, _policy);
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat data: " + ex.Message);
                    }
                    

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
                txtNIPP.Text = selectedNIPP;
                txtNama.Text = row["NamaKaryawan"]?.ToString();
                txtNIPP.IsEnabled = false;

                if (BtnEditPegawai != null)
                {
                    BtnEditPegawai.IsEnabled = true;
                }

            }
            else
            {
                selectedNIPP = string.Empty;
                txtNIPP.Text = string.Empty;
                txtNama.Text = string.Empty;
                txtNIPP.IsEnabled = false;

                if (BtnEditPegawai != null)
                {
                    BtnEditPegawai.IsEnabled = false;
                }
            }
        }

        

        private void BtnTambahPegawai_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPegawai();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                string NIPP = dialog.NIPP.Trim();
                string Nama = dialog.NamaKaryawan.Trim();
                string Status = dialog.StatusKaryawan.Trim();


                if (NIPP.Length != 5 || !int.TryParse(NIPP, out _))
                {
                    MessageBox.Show("NIPP harus 5 digit angka.", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    
                    return;
                }

                if (string.IsNullOrWhiteSpace(Nama) || string.IsNullOrWhiteSpace(Status))
                {
                    MessageBox.Show("Semua kolom harus diisi.", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddKaryawan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@NIPP", NIPP);
                            cmd.Parameters.AddWithValue("@NamaKaryawan", Nama);
                            cmd.Parameters.AddWithValue("@statuskaryawan", Status);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Karyawan berhasil ditambahkan");
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

        private void BtnEditPegawai_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = dataGridPegawai.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Pilih pegawai yang ingin diedit.");
                return;
            }

            if (string.IsNullOrEmpty(selectedNIPP))
            {
                MessageBox.Show("NIPP pegawai tidak valid", "Kesalahan ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            
            string currentNama = selectedRow["NamaKaryawan"].ToString();
            string currentStatus = selectedRow["statusKaryawan"].ToString();

            var dialog = new InputDialogPegawai(selectedNIPP, currentNama, currentStatus);
            dialog.Owner = Window.GetWindow(this);
            dialog.DisableNIPPInput();

            if (dialog.ShowDialog() == true)
            {
                string namaBaru = dialog.NamaKaryawan.Trim();
                string statusBaru = dialog.StatusKaryawan.Trim();

                if (string.IsNullOrWhiteSpace(namaBaru))
                {
                    MessageBox.Show("Nama Karyawan tidak boleh kosong.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(statusBaru)) 
                {
                    MessageBox.Show("Status Karyawan harus dipilih.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateKaryawan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@NIPP", selectedNIPP);
                            cmd.Parameters.AddWithValue("@NamaKaryawan", (object)namaBaru);
                            cmd.Parameters.AddWithValue("@statusKaryawan", (object)statusBaru);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Data karyawan berhasil diperbarui");
                            _cache.Remove(CacheKey);
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50007) 
                    {
                        MessageBox.Show("Data pegawai tidak ditemukan: " + sqlEx.Message);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memperbarui data: " + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message);
                }
            }
        }


        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
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
                    MessageBox.Show("error saat menganalisis query: " + ex.Message, "error analisis", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (statisticResult.Length > 0 ) 
                {
                    MessageBox.Show(statisticResult.ToString(), "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Information);                   
                }
                else
                {
                    MessageBox.Show("tidak ada informasi statistik yang diterima", "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            string queryToAnalyze = "SELECT NIPP, NamaKaryawan, statusKaryawan FROM Karyawan";
            AnalyzeQuery(queryToAnalyze);

        }
    }
}
