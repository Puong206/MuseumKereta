using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MuseumApp
{
    public partial class LaporanKoleksi : Window
    {
        private readonly string connectionString;
        public LaporanKoleksi(string connectionString)
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

                string query = "SELECT KoleksiID, JenisKoleksi, Deskripsi FROM Koleksi ORDER BY JenisKoleksi ASC;";
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }

                ReportDataSource rds = new ReportDataSource("DataSetKoleksi", dt);

                ReportViewer.LocalReport.DataSources.Clear();
                ReportViewer.LocalReport.DataSources.Add(rds);

                ReportViewer.LocalReport.ReportPath = @"A:\Kuliah\Semester 4\PABD\Project\MuseumApp\KoleksiReport.rdlc";
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
