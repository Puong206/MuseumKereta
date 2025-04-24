using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MuseumApp
{
    public partial class Kelola_Pegawai : Page
    {
        private string connectionString;
        private SqlConnection conn;
        private SqlDataAdapter adapter;
        private DataTable dt;
        public Kelola_Pegawai(string connStr)
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
                adapter = new SqlDataAdapter("SELECT NIPP, NamaKaryawan, statusKaryawan FROM Karyawan", conn);
                dt = new DataTable();
                adapter.Fill(dt);
                dataGridPegawai.ItemsSource = dt.DefaultView;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void dataGridPegawai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridPegawai.SelectedItem is DataRowView row)
            {
                txtNIPP.Text = row["NIPP"].ToString();
                txtNama.Text = row["NamaKaryawan"].ToString();
                txtNIPP.IsEnabled = false;
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPegawai.SelectedItem is DataRowView row)
            {
                txtNIPP.Text = row["NIPP"].ToString();
                txtNama.Text = row["NamaKaryawan"].ToString();
                txtNIPP.IsEnabled = false;
            }
        }

        private void BtnTambahPegawai_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialogPegawai();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                string NIPP = dialog.NIPP.Trim();
                string Nama = dialog.NamaKaryawan;
                string Status = dialog.StatusKaryawan;


                if (NIPP.Length != 5 || !int.TryParse(NIPP, out _))
                {
                    MessageBox.Show("NIPP harus 5 digit angka.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Nama) || string.IsNullOrWhiteSpace(Status))
                {
                    MessageBox.Show("Semua kolom harus diisi.");
                    return;
                }

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Karyawan (NIPP, NamaKaryawan, statusKaryawan) VALUES (@NIPP, @Nama, @Status)", conn))
                    {
                        cmd.Parameters.AddWithValue("@NIPP", NIPP);
                        cmd.Parameters.AddWithValue("@Nama", Nama);
                        cmd.Parameters.AddWithValue("@Status", Status);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();

                    MessageBox.Show("Pegawai berhasil ditambahkan.");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambah data: " + ex.Message);
                }
            }
        }

        private void BtnEditPegawai_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = dataGridPegawai.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Pilih pegawai yang ingin diedit.");
                return;
            }

            string currentNIPP = selectedRow["NIPP"].ToString();
            string currentNama = selectedRow["NamaKaryawan"].ToString();
            string currentStatus = selectedRow["statusKaryawan"].ToString();

            var dialog = new InputDialogPegawai(currentNIPP, currentNama, currentStatus);
            dialog.Owner = Window.GetWindow(this);
            dialog.DisableNIPPInput();

            if (dialog.ShowDialog() == true)
            {
                string namaBaru = dialog.NamaKaryawan;
                string statusBaru = dialog.StatusKaryawan;

                if (string.IsNullOrWhiteSpace(namaBaru) || string.IsNullOrWhiteSpace(statusBaru))
                {
                    MessageBox.Show("Nama dan status tidak boleh kosong.");
                    return;
                }

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE Karyawan SET NamaKaryawan = @Nama, statusKaryawan = @Status WHERE NIPP = @NIPP", conn))
                    {
                        cmd.Parameters.AddWithValue("@Nama", namaBaru);
                        cmd.Parameters.AddWithValue("@Status", statusBaru);
                        cmd.Parameters.AddWithValue("@NIPP", currentNIPP);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();

                    MessageBox.Show("Pegawai berhasil diperbarui.");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message);
                }
            }
        }


        private void ClearForm()
        {
            txtNIPP.Text = "";
            txtNama.Text = "";
            txtNIPP.IsEnabled = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
