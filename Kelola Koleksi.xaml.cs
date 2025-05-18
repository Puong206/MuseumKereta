using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
    public partial class Kelola_Koleksi : Page
    {
        private string connectionString;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataAdapter adapter;
        private DataTable dt;

        private TextBox txtJenisKoleksi = new TextBox();
        private TextBox txtDeskripsi = new TextBox();
        private TextBox hiddenId = new TextBox();
        // Dummy controls yang tidak ada di XAML

        public Kelola_Koleksi(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            conn = new SqlConnection(connectionString);
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    adapter = new SqlDataAdapter("SELECT KoleksiID, JenisKoleksi, Deskripsi FROM Koleksi", conn);
                    dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridKoleksi.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data: " + ex.Message);
                }
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
                selectedId = Convert.ToInt32(row["KoleksiID"]);

                txtJenisKoleksi.Text = selectedJenis;
                txtDeskripsi.Text = selectedDeskripsi;
                hiddenId.Text = selectedId.ToString();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
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
            var dialog = new InputDialog();
            dialog.JenisTextBox.Text = selectedJenis;
            dialog.DeskripsiTextBox.Text = selectedDeskripsi;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateKoleksi", conn)) 
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@KoleksiID", selectedId);
                            cmd.Parameters.AddWithValue("@JenisKoleksi", (object)dialog.JenisKoleksi);
                            cmd.Parameters.AddWithValue("@Deskripsi", (object)dialog.Deskripsi);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Koleksi berhasil diperbaharui");
                        }
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memperbaharui Koleksi" + ex.Message);
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
                        MessageBox.Show("Koleksi berhasil dihapus");
                    }
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message);
            }

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
