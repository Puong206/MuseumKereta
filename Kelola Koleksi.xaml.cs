﻿using System.IO;

using Microsoft.Win32;

using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Caching;
using System.Text;
using System.Collections.Generic;



namespace MuseumApp
{
    public partial class Kelola_Koleksi : Page
    {
        private readonly string connectionString;
        
        private SqlDataAdapter adapter;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "KoleksiData";

        private TextBox txtJenisKoleksi = new TextBox();
        private TextBox txtDeskripsi = new TextBox();
        private TextBox hiddenId = new TextBox();
        

        public Kelola_Koleksi(string connStr)
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
                    IF OBJECT_ID('dbo.Koleksi', 'U') IS NOT NULL
                    BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_JenisKoleksi' AND object_id = OBJECT_ID('dbo.Koleksi'))
                        CREATE UNIQUE NONCLUSTERED INDEX idx_JenisKoleksi ON dbo.Koleksi(JenisKoleksi);
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
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {

                        conn.Open();
                        adapter = new SqlDataAdapter("SELECT KoleksiID, JenisKoleksi, Deskripsi FROM Koleksi", conn);
                        dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridKoleksi.ItemsSource = dt.DefaultView;
                        _cache.Set(CacheKey, dt, _policy);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data: " + ex.Message);
                }
                
            }
            else
            {
                dataGridKoleksi.ItemsSource = dt.DefaultView;
            }
        }

        private int selectedId;
        private string selectedJenis;
        private string selectedDeskripsi;

        private void dataGridKoleksi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem is DataRowView row)
            {
                selectedJenis = row["JenisKoleksi"].ToString();
                selectedDeskripsi = row["Deskripsi"].ToString();
                int.TryParse(row["KoleksiID"]?.ToString(), out selectedId);

                bool isItemSelected = selectedId > 0;
                if (BtnEdit != null) BtnEdit.IsEnabled = isItemSelected;
                if (BtnHapus != null) BtnHapus.IsEnabled = isItemSelected;

                
            }
            else
            {
                selectedId = 0;
                if (BtnEdit != null) BtnEdit.IsEnabled = false;
                if (BtnHapus != null) BtnHapus.IsEnabled = false;
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dialog.JenisKoleksi) || string.IsNullOrWhiteSpace(dialog.Deskripsi))
                    {
                        MessageBox.Show("Jenis Koleksi dan Deskripsi harus diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddKoleksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@JenisKoleksi", dialog.JenisKoleksi);
                            cmd.Parameters.AddWithValue("@Deskripsi", dialog.Deskripsi);
                            SqlParameter outputIdParam = new SqlParameter("@IDKoleksiIdentity", SqlDbType.Int);
                            outputIdParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputIdParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            int generatedID = (int)outputIdParam.Value;
                            MessageBox.Show("Koleksi berhasil ditambahkan");
                            _cache.Remove(CacheKey);
                        }
                    }

                    LoadData();
                } catch (Exception ex)
                {
                    MessageBox.Show("Gagal menammbah Koleksi" + ex.Message);
                }
                
              
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem == null)
            {
                MessageBox.Show("Pilih koleksi yang ingin diedit");
                return;
            }

            if (selectedId <= 0)
            {
                MessageBox.Show("ID Koleksi tidak valid", "kesalahan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new InputDialog();
            if (dialog.JenisTextBox != null) dialog.JenisTextBox.Text = selectedJenis;
            if (dialog.DeskripsiTextBox != null) dialog.DeskripsiTextBox.Text = selectedDeskripsi;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dialog.JenisKoleksi) || string.IsNullOrWhiteSpace(dialog.Deskripsi))
                    {
                        MessageBox.Show("Jenis Koleksi dan Deskripsi harus diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateKoleksi", conn)) 
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@KoleksiID", selectedId);
                            cmd.Parameters.AddWithValue("@JenisKoleksi", dialog.JenisKoleksi.Trim());
                            cmd.Parameters.AddWithValue("@Deskripsi", dialog.Deskripsi.Trim());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0) 
                            {
                                MessageBox.Show("Koleksi berhasil diperbarui.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                                _cache.Remove("KoleksiData");

                                _cache.Remove("BarangData");

                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Data koleksi tidak ditemukan atau tidak ada perubahan.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                             
                        }
                    }
                    
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 50001)
                    {
                        MessageBox.Show("Data koleksi tidak ditemukan", "Kesalahan Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memperbaharui Koleksi" + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Terjadi kesalahan tak terduga saat memperbaharui Koleksi: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
                }


            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridKoleksi.SelectedItem == null)
            {
                MessageBox.Show("Pilih koleksi yang ingin dihapus.");
                return;
            }

            if (selectedId <= 0)
            {
                MessageBox.Show("ID koleksi tidak valid. Silakan pilih baris yang benar.", "Kesalahan ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Yakin ingin menghapus koleksi dengan ID {selectedId}?", "Konfirmasi", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DeleteKoleksi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@KoleksiID", selectedId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Koleksi berhasil dihapus.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        _cache.Remove("KoleksiData");

                        _cache.Remove("BarangData");
                        LoadData();
                       
                         
                    }
                }
                
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50002)
                {
                    MessageBox.Show("Data koleksi tidak ditemukan.", "Kesalahan Hapus", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Gagal menghapus data: " + sqlEx.Message, "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Terjadi kesalahan tak terduga saat menghapus data: " + ex.Message, "Kesalahan Umum", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateHome();
        }

        private void AnalyzeQuery (string sqlQuery)
        {
            StringBuilder statisticsResult = new StringBuilder();

            using (var conn = new SqlConnection(connectionString)) 
            {
                conn.InfoMessage += (s, e) =>
                {
                    statisticsResult.AppendLine(e.Message);
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
                    MessageBox.Show("error saat enganalisis query" + ex.Message, "error analisis",MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (statisticsResult.Length > 0)
            {
                MessageBox.Show(statisticsResult.ToString(), "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("tidak ada informasi statistik yang diterima", "STATISTICS INFO", MessageBoxButton.OK, MessageBoxImage.Warning );
            }
        }

        private void BtnAnalisis_Click(object sender, RoutedEventArgs e)
        {
            string queryToAnalyze = "SELECT * FROM Koleksi WHERE JenisKoleksi = 'tiket';";
            AnalyzeQuery(queryToAnalyze);
        }
    }
} 
