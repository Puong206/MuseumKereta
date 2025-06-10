using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MuseumApp
{
    public partial class DashboardHomePage : Page
    {
        private readonly string connectionString;

        public DashboardHomePage(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllDashboardData();
            DateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID"));

            DashboardCalendar.SelectedDate = DateTime.Today;
            DashboardCalendar.DisplayDate = DateTime.Today;
        }

        private void LoadAllDashboardData()
        {
            string query = @"
                SELECT 'Stats' AS DataType, 
                       (SELECT COUNT(*) FROM BarangMuseum) AS TotalBarang,
                       (SELECT COUNT(*) FROM Koleksi) AS TotalKoleksi,
                       (SELECT COUNT(*) FROM Karyawan) AS TotalPegawai,
                       (SELECT COUNT(*) FROM Perawatan) AS TotalPerawatan,
                       NULL AS Data1, NULL AS Data2, NULL AS Data3;

                SELECT TOP 5 'Recent' AS DataType,
                       p.JenisPerawatan AS Data1,
                       b.NamaBarang AS Data2,
                       p.TanggalPerawatan AS Data3
                FROM Perawatan p
                LEFT JOIN BarangMuseum b ON p.BarangID = b.BarangID
                ORDER BY p.TanggalPerawatan DESC;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Proses hasil pertama (Statistik)
                            if (reader.Read())
                            {
                                TotalBarangText.Text = reader["TotalBarang"].ToString();
                                TotalKoleksiText.Text = reader["TotalKoleksi"].ToString();
                                TotalPegawaiText.Text = reader["TotalPegawai"].ToString();
                                TotalPerawatanText.Text = reader["TotalPerawatan"].ToString();
                            }

                            // Proses hasil kedua (Aktivitas Terbaru)
                            reader.NextResult();

                            // UBAH: Buat list untuk menampung objek anonim
                            var recentActivities = new List<object>();
                            while (reader.Read())
                            {
                                // UBAH: Buat objek anonim baru dengan properti yang sesuai
                                recentActivities.Add(new
                                {
                                    Icon = "\uE945", // Ikon service/repair
                                    IconBackground = new SolidColorBrush(Color.FromRgb(0, 121, 107)), // Teal
                                    Title = reader["Data1"].ToString(),
                                    Subtitle = "Barang: " + (reader["Data2"] == DBNull.Value ? "Umum" : reader["Data2"].ToString()),
                                    DateInfo = Convert.ToDateTime(reader["Data3"]).ToString("dd MMM yyyy")
                                });
                            }
                            RecentActivityList.ItemsSource = recentActivities;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data dashboard: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DashboardCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DashboardCalendar.SelectedDate.HasValue)
            {
                LoadScheduleForDate(DashboardCalendar.SelectedDate.Value);
            }
        }

        private void LoadScheduleForDate(DateTime selectedDate)
        {
            ScheduleTitleText.Text = "Jadwal untuk " + selectedDate.ToString("dd MMMM yyyy");

            string query = @"
                SELECT p.JenisPerawatan, b.NamaBarang
                FROM Perawatan p
                LEFT JOIN BarangMuseum b ON p.BarangID = b.BarangID
                WHERE CAST(p.TanggalPerawatan AS DATE) = @SelectedDate
                ORDER BY p.TanggalPerawatan ASC;";

            // UBAH: Buat list untuk menampung objek anonim
            var scheduleItems = new List<object>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // UBAH: Buat objek anonim baru
                                scheduleItems.Add(new
                                {
                                    Icon = "\uE787", // Ikon kalender
                                    IconBackground = new SolidColorBrush(Color.FromRgb(242, 105, 36)), // Oranye
                                    Title = reader["JenisPerawatan"].ToString(),
                                    DateInfo = "Barang: " + (reader["NamaBarang"] == DBNull.Value ? "Umum" : reader["NamaBarang"].ToString())
                                });
                            }
                        }
                    }
                }

                if (scheduleItems.Count == 0)
                {
                    // UBAH: Buat objek anonim untuk pesan "tidak ada jadwal"
                    scheduleItems.Add(new
                    {
                        Icon = "\uE8F5", // Ikon checklist
                        IconBackground = Brushes.Gray,
                        Title = "Tidak Ada Jadwal",
                        DateInfo = "Tidak ada perawatan tercatat untuk tanggal ini."
                    });
                }

                ScheduleList.ItemsSource = scheduleItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data jadwal: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metode-metode helper untuk membuat DataTemplate bisa dihapus jika Anda sudah meletakkannya di XAML
        // dan metode untuk tombol pintasan cepat juga bisa dihapus karena UI-nya sudah diganti kalender.
    }
}