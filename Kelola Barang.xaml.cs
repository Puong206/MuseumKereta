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
        // private SqlConnection conn;
        // private SqlCommand cmd;
        // private SqlDataAdapter adapter;
        // private DataTable dt;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };

        private const string CacheKey = "BarangData";

        private bool isEditMode = false;
        private string currentEditBarangID = null;

        public Kelola_Barang(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            EnsureIndexes();
            LoadData();

            //if (BtnEdit != null) BtnEdit.IsEnabled = false;
            //if (BtnHapus != null) BtnHapus.IsEnabled = false;
            //if (BtnAnalisis != null) BtnAnalisis.IsEnabled = true;

            UpdateActionButtonState(false);
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
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Barang_NamaBarang' AND object_id = OBJECT_ID('dbo.BarangMuseum'))
                            CREATE NONCLUSTERED INDEX idx_Barang_NamaBarang ON dbo.BarangMuseum(NamaBarang); 
                    END";
                    using (SqlCommand cmd = new SqlCommand(indexScript, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memastikan indeks: " + ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            DataTable dt = _cache.Get(CacheKey) as DataTable;
            if (dt != null) 
            {
                dataGridBarang.ItemsSource = dt.DefaultView;
                ApplyInitialSort(dt.DefaultView);
            }
            else
            {
                try
                {
                    DataTable newDt = new DataTable();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {

                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BarangMuseum", conn);
                        adapter.Fill(newDt);
                    }
                    dataGridBarang.ItemsSource = newDt.DefaultView;
                    ApplyInitialSort(newDt.DefaultView);
                    _cache.Set(CacheKey, newDt, _policy);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data: " + ex.Message);
                }
            }
        }

        private void ApplyInitialSort(DataView view)
        {
            if (view.Table.Columns.Contains("NamaBarang"))
            {
                view.Sort = "NamaBarang ASC"; // Atur urutan awal berdasarkan NamaBarang
            }
            else if (view.Table.Columns.Contains("BarangID"))
            {
                view.Sort = "BarangID ASC";
            }
        }

        private void UpdateActionButtonState(bool isItemSelected)
        {
            if (BtnEdit != null)
            {
                BtnEdit.IsEnabled = isItemSelected;
            }
            if (BtnHapus != null)
            {
                BtnHapus.IsEnabled = isItemSelected;
            }
        }

        private void ShowPopup()
        {
            PopupOverlayGrid.Visibility = Visibility.Visible;
        }

        private void HidePopup()
        {
            PopupOverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = false;
            currentEditBarangID = null;
            BarangInputFormControl.ClearForm();
            ShowPopup();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridBarang.SelectedItem is DataRowView row)
            {
                isEditMode = true;
                currentEditBarangID = row["BarangID"]?.ToString(); // Simpan ID untuk proses update

                if (string.IsNullOrWhiteSpace(currentEditBarangID))
                {
                    MessageBox.Show("BarangID dari data yang dipilih tidak valid.", "Kesalahan Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Panggil metode LoadDataForEdit di UserControl
                BarangInputFormControl.LoadDataForEdit(
                    currentEditBarangID,
                    row["NamaBarang"]?.ToString() ?? "",
                    row["Deskripsi"]?.ToString() ?? "",
                    row["KoleksiID"]?.ToString() ?? "",
                    row["TahunPembuatan"]?.ToString() ?? "",
                    row["AsalBarang"]?.ToString() ?? ""
                );
                // BarangInputFormControl.DisableBarangIDInput(); // Sudah dihandle di LoadDataForEdit
                ShowPopup();
            }
            else
            {
                MessageBox.Show("Pilih data barang yang akan diedit.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BarangInputFormControl_SaveClicked(object sender, EventArgs e)
        {
            string barangID = BarangInputFormControl.BarangID;
            string namaBarang = BarangInputFormControl.NamaBarang;
            string deskripsi = BarangInputFormControl.Deskripsi;
            string koleksiID = BarangInputFormControl.KoleksiID;
            string tahunPembuatan = BarangInputFormControl.TahunPembuatan;
            string asalBarang = BarangInputFormControl.AsalBarang;

            if (string.IsNullOrWhiteSpace(barangID) && !!isEditMode)
            {
                MessageBox.Show("ID Barang harus diisi!.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(namaBarang))
            {
                MessageBox.Show("Nama Barang harus diisi!.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (tahunPembuatan.Length != 4 || !tahunPembuatan.All(char.IsDigit))
            {
                MessageBox.Show("Tahun Pembuatan harus terdiri dari 4 digit angka.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int koleksiIDInt;
            if (string.IsNullOrWhiteSpace(koleksiID) || !int.TryParse(koleksiID, out koleksiIDInt))
            {
                MessageBox.Show("ID Koleksi harus diisi!.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string storedProcedure = isEditMode ? "EditBarang" : "TambahBarang";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BarangID", isEditMode ? currentEditBarangID : barangID);
                        cmd.Parameters.AddWithValue("@NamaBarang", namaBarang);
                        cmd.Parameters.AddWithValue("@Deskripsi", string.IsNullOrWhiteSpace(deskripsi) ? (object)DBNull.Value : deskripsi);
                        cmd.Parameters.AddWithValue("@KoleksiID", koleksiIDInt);
                        cmd.Parameters.AddWithValue("@TahunPembuatan", tahunPembuatan);
                        cmd.Parameters.AddWithValue("@AsalBarang", string.IsNullOrWhiteSpace(asalBarang) ? (object)DBNull.Value : asalBarang);
                        
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show(isEditMode ? "Data barang berhasil diperbarui." : "Barang berhasil ditambahkan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                        _cache.Remove(CacheKey);  // Hapus cache setelah edit data
                    }
                }
                LoadData();
                HidePopup();
            }
            catch (SqlException sqlEx)
            {
                string userMessage = "Terjadi kesalahan database.";
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Pelanggaran Primary Key
                {
                    userMessage = $"Gagal {(isEditMode ? "memperbarui" : "menambah")} data: BarangID '{(isEditMode ? currentEditBarangID : barangID)}' sudah ada atau tidak valid.";
                }
                else if (sqlEx.Number == 547) // Pelanggaran Foreign Key
                {
                    userMessage = $"Gagal {(isEditMode ? "memperbarui" : "menambah")} data: KoleksiID '{koleksiID}' tidak ditemukan atau tidak valid.";
                }
                else if (sqlEx.Number == 50003 && isEditMode) // Custom error dari SP Update jika barang tidak ditemukan
                {
                    userMessage = "Data barang tidak ditemukan untuk diperbarui. Mungkin sudah dihapus atau ID tidak valid.";
                }
                else
                {
                    userMessage = $"Gagal {(isEditMode ? "memperbarui" : "menambah")} data: {sqlEx.Message}";
                }
                MessageBox.Show(userMessage, "Error Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal {(isEditMode ? "memperbarui" : "menambah")} data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BarangInputFormControl_CancelClicked(object sender, EventArgs e)
        {
            HidePopup();
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dataGridBarang.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Pilih data barang yang akan dihapus.");
                return;
            }
            string barangIdToDelete = row["BarangID"]?.ToString();

            if (string.IsNullOrWhiteSpace(barangIdToDelete))
            {
                MessageBox.Show("BarangID dari data yang dipilih tidak valid.", "Kesalahan Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Yakin ingin menghapus BarangID '{barangIdToDelete}'?", "Konfirmasi", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                            MessageBox.Show("Data barang berhasil dihapus!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                            _cache.Remove(CacheKey); // Hapus cache
                        }
                    }
                    LoadData();
                }
                catch (SqlException sqlEx)
                {
                    string userMessage = "Gagal menghapus data.";
                    if (sqlEx.Number == 50004) // Custom error dari SP Delete jika barang tidak ditemukan
                    {
                        userMessage = "Data barang tidak ditemukan untuk dihapus. Mungkin sudah dihapus atau ID tidak valid.";
                    }
                    else if (sqlEx.Number == 547) // Error jika barang direferensikan oleh tabel lain (misal, Perawatan)
                    {
                        userMessage = $"Gagal menghapus BarangID '{barangIdToDelete}'. Barang ini mungkin masih digunakan dalam data Perawatan atau data lainnya.";
                    }
                    else
                    {
                        userMessage = $"Gagal menghapus data: {sqlEx.Message}";
                    }
                    MessageBox.Show(userMessage, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat menghapus data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void dataGridBarang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateActionButtonState(dataGridBarang.SelectedItem != null);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            // BELOOOOOOOOMMMMM
            MessageBox.Show("Tombol Analisis Barang diklik! Fitur ini belum diimplementasikan.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
