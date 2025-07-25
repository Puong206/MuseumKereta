﻿using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
    public partial class LaporanPerawatan : Page
    {
        private readonly string connectionString;
        public LaporanPerawatan(string connStr)
        {
            InitializeComponent();
            this.connectionString = connStr;
            SetupReportViewer();
        }

        private void SetupReportViewer()
        {
            try
            {
                string query = @"SELECT
                    p.PerawatanID,
                    p.TanggalPerawatan,
                    b.BarangID,
                    b.NamaBarang, 
                    kar.NIPP,
                    kar.NamaKaryawan,
                    p.JenisPerawatan,
                    p.Catatan
                FROM
                    Perawatan AS p
                LEFT JOIN
                    BarangMuseum AS b ON p.BarangID = b.BarangID -- LEFT JOIN agar perawatan tanpa barang tetap muncul
                LEFT JOIN
                    Karyawan AS kar ON p.NIPP = kar.NIPP -- LEFT JOIN untuk keamanan jika NIPP bisa NULL
                ORDER BY
                    p.TanggalPerawatan DESC;";

                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }

                ReportDataSource rds = new ReportDataSource("DataSetPerawatan", dt);

                ReportViewer.LocalReport.DataSources.Clear();
                ReportViewer.LocalReport.DataSources.Add(rds);
                ReportViewer.LocalReport.ReportPath = "PerawatanReport.rdlc";
                ReportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError($"Gagal memuat laporan: {ex.Message}", "Error Laporan");

            }
        }
        private void BtnKembali_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }
    }
}
