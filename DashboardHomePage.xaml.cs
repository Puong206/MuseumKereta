using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace MuseumApp
{
    public partial class DashboardHomePage : Page
    {
        private readonly string connectionString;
        public SeriesCollection PieChartSeries { get; set; }
        // Kelas internal sederhana untuk menampung data daftar
        private class ListItem
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string DateInfo { get; set; }
            public string Icon { get; set; }
            public Brush IconBackground { get; set; }
        }

        public DashboardHomePage(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;

            PieChartSeries = new SeriesCollection();
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllDashboardData();
            DateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID"));
        }

        private void LoadAllDashboardData()
        {
            // Query efisien untuk mengambil semua data dalam satu kali jalan
            string query = @"
                -- 1. Statistik Utama
                SELECT 'Stats' AS DataType, 
                       (SELECT COUNT(*) FROM BarangMuseum) AS TotalBarang,
                       (SELECT COUNT(*) FROM Koleksi) AS TotalKoleksi,
                       (SELECT COUNT(*) FROM Karyawan) AS TotalPegawai,
                       (SELECT COUNT(*) FROM Perawatan) AS TotalPerawatan,
                       NULL AS Data1, NULL AS Data2, NULL AS Data3;

                -- 2. 5 Aktivitas Perawatan Terbaru
                SELECT TOP 5 'Recent' AS DataType,
                       p.JenisPerawatan AS Data1,
                       b.NamaBarang AS Data2,
                       p.TanggalPerawatan AS Data3
                FROM Perawatan p LEFT JOIN BarangMuseum b ON p.BarangID = b.BarangID
                ORDER BY p.TanggalPerawatan DESC;

                -- 3. 5 Jadwal Perawatan Terdekat
                SELECT TOP 5 'Upcoming' AS DataType,
                       p.JenisPerawatan AS Data1,
                       b.NamaBarang AS Data2,
                       p.TanggalPerawatan AS Data3
                FROM Perawatan p LEFT JOIN BarangMuseum b ON p.BarangID = b.BarangID
                WHERE p.TanggalPerawatan >= GETDATE()
                ORDER BY p.TanggalPerawatan ASC;

                -- 4. 5 Barang Baru Ditambahkan
                -- Asumsi tabel BarangMuseum memiliki kolom tanggal penambahan, misal 'TanggalDitambahkan'
                -- Jika tidak ada, kita urutkan berdasarkan BarangID terbaru sebagai gantinya
                SELECT TOP 5 'NewItems' as DataType,
                       NamaBarang as Data1,
                       AsalBarang as Data2,
                       TahunPembuatan as Data3
                FROM BarangMuseum
                ORDER BY BarangID DESC; -- Ganti dengan 'TanggalDitambahkan DESC' jika ada

                -- 5. BARU: Data untuk Pie Chart (Top 5 Tahun + Lainnya)
                WITH YearCounts AS (
                    SELECT
                        TahunPembuatan,
                        COUNT(*) AS ItemCount,
                        ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC, TahunPembuatan DESC) as rn
                    FROM BarangMuseum
                    WHERE TahunPembuatan IS NOT NULL
                    GROUP BY TahunPembuatan
                )
                SELECT 'PieChart' as DataType,
                    CASE WHEN rn <= 5 THEN CAST(TahunPembuatan AS VARCHAR(20)) ELSE 'Lainnya' END AS Data1,
                    SUM(ItemCount) AS Data2
                FROM YearCounts
                GROUP BY CASE WHEN rn <= 5 THEN CAST(TahunPembuatan AS VARCHAR(20)) ELSE 'Lainnya' END;
            ";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 1. Proses Statistik
                            if (reader.Read())
                            {
                                TotalBarangText.Text = reader["TotalBarang"].ToString();
                                TotalKoleksiText.Text = reader["TotalKoleksi"].ToString();
                                TotalPegawaiText.Text = reader["TotalPegawai"].ToString();
                                TotalPerawatanText.Text = reader["TotalPerawatan"].ToString();
                            }

                            // 2. Proses Aktivitas Terbaru
                            reader.NextResult();
                            var recentActivities = new List<ListItem>();
                            while (reader.Read())
                            {
                                recentActivities.Add(new ListItem
                                {
                                    Icon = "\uE945", // Ikon service/repair
                                    IconBackground = new SolidColorBrush(Color.FromRgb(0, 121, 107)), // Teal
                                    Title = reader["Data1"].ToString(),
                                    Subtitle = "Barang: " + (reader["Data2"] == DBNull.Value ? "Umum" : reader["Data2"].ToString()),
                                    DateInfo = Convert.ToDateTime(reader["Data3"]).ToString("dd MMM yyyy")
                                });
                            }
                            RecentActivityList.ItemsSource = recentActivities;

                            // 3. Proses Jadwal Terdekat
                            reader.NextResult();
                            var upcomingSchedules = new List<ListItem>();
                            while (reader.Read())
                            {
                                upcomingSchedules.Add(new ListItem
                                {
                                    Icon = "\uE787", // Ikon kalender
                                    IconBackground = new SolidColorBrush(Color.FromRgb(242, 105, 36)), // Oranye
                                    Title = reader["Data1"].ToString(),
                                    DateInfo = "Barang: " + (reader["Data2"] == DBNull.Value ? "Umum" : reader["Data2"].ToString()) + " - " + Convert.ToDateTime(reader["Data3"]).ToString("dd MMM yyyy")
                                });
                            }
                            UpcomingScheduleList.ItemsSource = upcomingSchedules;

                            // 4. Proses Barang Baru
                            reader.NextResult();
                            var newItems = new List<ListItem>();
                            while (reader.Read())
                            {
                                newItems.Add(new ListItem
                                {
                                    Icon = "\uE7B8", // Ikon box/paket
                                    IconBackground = new SolidColorBrush(Color.FromRgb(45, 43, 112)), // Biru
                                    Title = reader["Data1"].ToString(),
                                    DateInfo = "Asal: " + reader["Data2"].ToString() + " (Thn: " + reader["Data3"].ToString() + ")"
                                });
                            }
                            NewItemsList.ItemsSource = newItems;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError("Gagal memuat data dashboard: ", "Error");
            }
        }

        // Event handler kalender tidak lagi relevan dalam layout ini, bisa dihapus
        // private void DashboardCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) { ... }
        // private void LoadScheduleForDate(DateTime selectedDate) { ... }
    }
}