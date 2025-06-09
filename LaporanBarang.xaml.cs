using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MuseumApp
{
    public partial class LaporanBarang : Window
    {
        private readonly string connectionString;

        public LaporanBarang(string connStr)
        {
            InitializeComponent();
            this.connectionString = connStr;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupReportViewer();
        }

        private void SetupReportViewer()
        {
            try
            {
                string query = @"SELECT    
                                    b.BarangID,    
                                    b.NamaBarang,    
                                    b.Deskripsi,    
                                    b.TahunPembuatan,    
                                    b.AsalBarang,    
                                    k.JenisKoleksi    
                                FROM    
                                    BarangMuseum AS b    
                                INNER JOIN    
                                    Koleksi AS k ON b.KoleksiID = k.KoleksiID    
                                ORDER BY    
                                    b.NamaBarang ASC;";

                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }

                ReportDataSource rds = new ReportDataSource("DataSetBarang", dt);

                ReportViewer.LocalReport.DataSources.Clear();
                ReportViewer.LocalReport.DataSources.Add(rds);
                ReportViewer.LocalReport.ReportPath = @"C:\Project PABD\BarangReport.rdlc";
                ReportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat laporan: {ex.Message}", "Error Laporan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnKembali_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }
    }
}