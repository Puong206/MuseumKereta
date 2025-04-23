using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for Kelola_Pegawai.xaml
    /// </summary>
    public partial class Kelola_Pegawai : Page
    {
        SqlConnection conn = new SqlConnection("Data Source=OLIPIA\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password");
        SqlDataAdapter adapter;
        DataTable dt;
        public Kelola_Pegawai()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                conn.Open();
                adapter = new SqlDataAdapter("SELECT NIPP, NamaKaryawan FROM Karyawan", conn);
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
            string NIPP = txtNIPP.Text.Trim();
            string NamaKaryawan = txtNama.Text.Trim();

            if (NIPP.Length != 5 || !int.TryParse(NIPP, out _))
            {
                MessageBox.Show("NIPP harus 5 digit angka.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NamaKaryawan))
            {
                MessageBox.Show("Nama tidak boleh kosong.");
                return;
            }

            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Karyawan (NIPP, NamaKaryawan) VALUES (@NIPP, @NamaKaryawan)", conn))
                {
                    cmd.Parameters.AddWithValue("@NIPP", NIPP);
                    cmd.Parameters.AddWithValue("@nama", NamaKaryawan);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();

                MessageBox.Show("Pegawai berhasil ditambahkan.");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menambah data: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataGridPegawai.SelectedItem is DataRowView row)) // Replaced 'is not' with '!(...)'
            {
                MessageBox.Show("Pilih pegawai yang ingin diedit.");
                return;
            }

            string NIPP = txtNIPP.Text.Trim();
            string NamaKaryawan = txtNama.Text.Trim();

            if (string.IsNullOrWhiteSpace(NamaKaryawan))
            {
                MessageBox.Show("Nama tidak boleh kosong.");
                return;
            }

            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Karyawan SET NamaKaryawan = @NamaKaryawan WHERE NIPP = @NIPP", conn))
                {
                    cmd.Parameters.AddWithValue("@NIPP", NIPP);
                    cmd.Parameters.AddWithValue("@NamaKaryawan", NamaKaryawan);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();

                MessageBox.Show("Pegawai berhasil diperbarui.");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message);
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
            NavigationService?.Navigate(new Page1());
        }
    }
}
