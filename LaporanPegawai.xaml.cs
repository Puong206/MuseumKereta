using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for LaporanPegawai.xaml
    /// </summary>
    public partial class LaporanPegawai : Window
    {
        private readonly string connectionString;
        public LaporanPegawai(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupReportViewer();
        }

        private void SetupReportViewer()
        {
            try
            {
                ReportViewer.ProcessingMode = ProcessingMode.Local;

                string query = "SELECT NIPP, NamaKaryawan, statusKaryawan FROM Karyawan ORDER BY NamaKaryawan ASC;";
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }

                ReportDataSource rds = new ReportDataSource("DataSetPegawai", dt);

                ReportViewer.LocalReport.DataSources.Clear();
                ReportViewer.LocalReport.DataSources.Add(rds);

                ReportViewer.LocalReport.ReportPath = @"C:\Project PABD\PegawaiReport.rdlc";
                ReportViewer.RefreshReport();
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Gagal memuat laporan koleksi: {ex.Message}", "Error Laporan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void BtnKembali_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }
    }
}
