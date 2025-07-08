using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class ImportData : Page
    {
        private readonly string connectionString;
        private string selectedFilePath = string.Empty;
        private DataTable previewDataTable = null;
        public ImportData(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            PopulateEntityTypeComboBox();
        }

        private void PopulateEntityTypeComboBox()
        {
            CmbEntityType.Items.Add("Koleksi");
            CmbEntityType.Items.Add("Barang");
            CmbEntityType.Items.Add("Pegawai");
            CmbEntityType.Items.Add("Perawatan");
            CmbEntityType.SelectedIndex = 0;
        }

        private void BtnPilihFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.OK) // Fix: Compare with DialogResult.OK instead of 'true'
            {
                selectedFilePath = openFileDialog.FileName;
                TxtFilePath.Text = selectedFilePath;
                PreviewData(selectedFilePath);
            }
        }

        private void PreviewData(string filepath)
        {
            try
            {
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

                    previewDataTable = new DataTable();

                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (var cell in headerRow.Cells)
                        {
                            previewDataTable.Columns.Add(cell.ToString());
                        }
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow dataRow = sheet.GetRow(i);
                        if (dataRow == null) continue;

                        DataRow newRow = previewDataTable.NewRow();
                        for (int j = 0; j < headerRow.LastCellNum; j++)
                        {
                            ICell cell = dataRow.GetCell(j);
                            newRow[j] = cell?.ToString() ?? string.Empty;
                        }
                        previewDataTable.Rows.Add(newRow);
                    }
                }

                PreviewDataGrid.ItemsSource = previewDataTable.DefaultView;
                BtnImport.IsEnabled = true;
                StatusText.Text = $"{previewDataTable.Rows.Count} baris ditemukan dan siap diimpor.";

            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError("Gagal Membaca file excel: ", "Error");
                BtnImport.IsEnabled = false;
            }
        }

        private void BtnMulaiImport_Click(object sender, RoutedEventArgs e)
        {
            if (previewDataTable == null || previewDataTable.Rows.Count == 0)
            {
                CustomMessageBox.ShowWarning("Tidak ada data untuk diimpor.", "Peringatan");
                return;
            }

            bool confirm = CustomMessageBox.ShowYesNo(
                $"Anda akan mengimpor {previewDataTable.Rows.Count} baris data. Lanjutkan?");

            if (!confirm) return;

            string entityType = CmbEntityType.SelectedItem.ToString();
            bool success = ImportDataToDatabase(entityType);

            if (success)
            {
                CustomMessageBox.ShowSuccess("Proses impor selesai.", "Sukses");
                TxtFilePath.Text = "";
                PreviewDataGrid.ItemsSource = null;
                BtnImport.IsEnabled = false;
                StatusText.Text = "Pilih jenis data dan file excel.";
            }
        }

        private bool ImportDataToDatabase(string entityType)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    int successCount = 0;
                    int failedCount = 0;

                    foreach (DataRow row in previewDataTable.Rows)
                    {
                        switch (entityType)
                        {
                            case "Koleksi":
                                if (ImportRowKoleksi(row, conn, transaction)) successCount++;
                                else failedCount++;
                                break;
                            case "Barang":
                                if (ImportRowBarang(row, conn, transaction)) successCount++;
                                else failedCount++;
                                break;
                            case "Pegawai":
                                if (ImportRowPegawai(row, conn, transaction)) successCount++;
                                else failedCount++;
                                break;
                            case "Perawatan":
                                if (ImportRowPerawatan(row, conn, transaction)) successCount++;
                                else failedCount++;
                                break;
                            default:
                                transaction.Rollback();
                                CustomMessageBox.ShowError("Jenis entitas tidak valid.", "Error");
                                return false;
                        }
                    }

                    if (failedCount > 0)
                    {
                        transaction.Rollback();
                        CustomMessageBox.ShowError($"Impor dibatalkan karena {failedCount} baris data tidak valid. Tidak ada data yang disimpan.", "Impor gagal");
                        return false;
                    }
                    else
                    {
                        transaction.Commit();
                        StatusText.Text = $"Impor berhasil: {successCount} baris data disimpan.";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    CustomMessageBox.ShowError("Terjadi kesalahan fatal saat mengimpor data ke database: " + ex.Message, "Error");
                    return false;
                }
            }
        }

        private bool ImportRowKoleksi(DataRow row, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                string jenis = row["JenisKoleksi"].ToString();
                string deskripsi = row["Deskripsi"].ToString();
                if (string.IsNullOrWhiteSpace(jenis)) return false;

                using (SqlCommand cmd = new SqlCommand("AddKoleksi", conn, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@JenisKoleksi", jenis);
                    cmd.Parameters.AddWithValue("@Deskripsi", deskripsi);
                    var p = cmd.Parameters.Add("@IDKoleksiIdentity", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        private bool ImportRowBarang(DataRow row, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                if (row["BarangID"].ToString().Length > 10 ||
                    string.IsNullOrWhiteSpace(row["NamaBarang"].ToString()) ||
                    !int.TryParse(row["TahunPembuatan"].ToString(), out _) ||
                    row["TahunPembuatan"].ToString().Length != 4)
                {
                    return false;
                }

                using (SqlCommand cmd = new SqlCommand("AddBarang", conn, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BarangID", row["BarangID"]);
                    cmd.Parameters.AddWithValue("@NamaBarang", row["NamaBarang"]);
                    cmd.Parameters.AddWithValue("@Deskripsi", row["Deskripsi"]);
                    cmd.Parameters.AddWithValue("@KoleksiID", int.Parse(row["KoleksiID"].ToString()));
                    cmd.Parameters.AddWithValue("@TahunPembuatan", row["TahunPembuatan"]);
                    cmd.Parameters.AddWithValue("@AsalBarang", row["AsalBarang"]);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        private bool ImportRowPegawai(DataRow row, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                string nipp = row["NIPP"].ToString();
                if (nipp.Length != 5 || !nipp.All(char.IsDigit)) return false;

                using (SqlCommand cmd = new SqlCommand("AddKaryawan", conn, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NIPP", nipp);
                    cmd.Parameters.AddWithValue("@NamaKaryawan", row["NamaKaryawan"]);
                    cmd.Parameters.AddWithValue("@statuskaryawan", row["statuskaryawan"]);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        private bool ImportRowPerawatan(DataRow row, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                if (!DateTime.TryParse(row["TanggalPerawatan"].ToString(), out _)) return false;

                using (SqlCommand cmd = new SqlCommand("AddPerawatan", conn, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BarangID", row["BarangID"]);
                    cmd.Parameters.AddWithValue("@TanggalPerawatan", DateTime.Parse(row["TanggalPerawatan"].ToString()));
                    cmd.Parameters.AddWithValue("@JenisPerawatan", row["JenisPerawatan"]);
                    cmd.Parameters.AddWithValue("@Catatan", row["Catatan"]);
                    cmd.Parameters.AddWithValue("@NIPP", row["NIPP"]);
                    var p = cmd.Parameters.Add("@PerawatanIDIdentity", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }
    }
}
