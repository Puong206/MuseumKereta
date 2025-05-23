using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace MuseumApp
{
    public partial class Kelola_Perawatan : Page
    {
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dt;
        string connectionString;

        public Kelola_Perawatan(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            conn = new SqlConnection(connectionString);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                conn.Open();
                adapter = new SqlDataAdapter("SELECT * FROM Perawatan", conn);
                dt = new DataTable();
                adapter.Fill(dt);
                dataGridPerawatan.ItemsSource = dt.DefaultView;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void dataGridPerawatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPerawatan.SelectedItem is DataRowView selectedRow)
            {
                if (BtnEdit !=null)
                {
                    BtnEdit.IsEnabled = true;
                }
                if (BtnHapus != null)
                {
                    BtnHapus.IsEnabled = true;
                }
            }
            else
            {
                if (BtnEdit != null)
                {
                    BtnEdit.IsEnabled = false;
                }
                if (BtnHapus != null)
                {
                    BtnHapus.IsEnabled = false;
                }
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPerawatan();
            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(dialog.IDBarang) || dialog.IDBarang.Length != 5 || !dialog.IDBarang.All(char.IsDigit))
                {
                    MessageBox.Show("BarangID harus terdiri dari tepat 5 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dialog.NIPP) || dialog.NIPP.Length != 5 || !dialog.NIPP.All(char.IsDigit))
                {
                    MessageBox.Show("NIPP harus terdiri dari tepat 5 digit angka.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddPerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BarangID", (object)dialog.IDBarang);
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", (object)dialog.Catatan);
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);

                            SqlParameter outputIdParam = new SqlParameter("@PerawatanIDIdentity", SqlDbType.Int);
                            outputIdParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputIdParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            int generatedID = (int)outputIdParam.Value;

                            MessageBox.Show("Data berhasil diperbarui.");
                        }
                    }


                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambah data: " + ex.Message);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan diedit.");
                return;
            }

            int perawatanId = Convert.ToInt32(row["PerawatanID"]);
            var dialog = new InputDialogPerawatan(
                row["BarangID"].ToString(),
                Convert.ToDateTime(row["TanggalPerawatan"]),
                row["JenisPerawatan"].ToString(),
                row["Catatan"].ToString(),
                row["NIPP"].ToString()
            );

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) 
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdatePerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PerawatanID", perawatanId);
                            cmd.Parameters.AddWithValue("@BarangID", (object)dialog.IDBarang); 
                            cmd.Parameters.AddWithValue("@TanggalPerawatan", dialog.TanggalPerawatan);
                            cmd.Parameters.AddWithValue("@JenisPerawatan", dialog.JenisPerawatan);
                            cmd.Parameters.AddWithValue("@Catatan", (object)dialog.Catatan); 
                            cmd.Parameters.AddWithValue("@NIPP", dialog.NIPP);


                            conn.Open();
                            cmd.ExecuteNonQuery();
                            
                        }
                    }

                    MessageBox.Show("Data berhasil diperbarui.");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message);
                }
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPerawatan.SelectedItem is DataRowView row))
            {
                MessageBox.Show("Pilih data yang akan dihapus.");
                return;
            }

            int perawatanId = Convert.ToInt32(row["PerawatanID"]);

            if (MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) 
                    {
                        using (SqlCommand cmd = new SqlCommand("DeletePerawatan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PerawatanID", perawatanId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Data berhasil dihapus.");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
