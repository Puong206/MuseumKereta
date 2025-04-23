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
    /// Interaction logic for Kelola_Barang.xaml
    /// </summary>
    public partial class Kelola_Barang : Page
    {
        SqlConnection conn = new SqlConnection("Data Source=OLIPIA\\\\OLIP;Initial Catalog=MuseumKeretaApi;User ID=username;Password=password");

        // Dummy controls yang tidak ada di XAML
        TextBox txtBarangID = new TextBox();
        TextBox txtNamaBarang = new TextBox();
        TextBox txtTahunPembuatan = new TextBox();
        TextBox txtAsalBarang = new TextBox();
        TextBox txtDeskripsi = new TextBox();
        ComboBox cbKoleksiID = new ComboBox(); // jika kamu pakai relasi Koleksi

        public Kelola_Barang()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM BarangMuseum", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridBarang.ItemsSource = dt.DefaultView;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO BarangMuseum VALUES (@id, @nama, @deskripsi, @koleksiID, @tahun, @asal)", conn);
                cmd.Parameters.AddWithValue("@id", txtBarangID.Text);
                cmd.Parameters.AddWithValue("@nama", txtNamaBarang.Text);
                cmd.Parameters.AddWithValue("@deskripsi", txtDeskripsi.Text);
                cmd.Parameters.AddWithValue("@koleksiID", cbKoleksiID.SelectedValue ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tahun", txtTahunPembuatan.Text);
                cmd.Parameters.AddWithValue("@asal", txtAsalBarang.Text);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE BarangMuseum SET NamaBarang=@nama, Deskripsi=@deskripsi, KoleksiID=@koleksiID, TahunPembuatan=@tahun, AsalBarang=@asal WHERE BarangID=@id", conn);
                cmd.Parameters.AddWithValue("@id", txtBarangID.Text);
                cmd.Parameters.AddWithValue("@nama", txtNamaBarang.Text);
                cmd.Parameters.AddWithValue("@deskripsi", txtDeskripsi.Text);
                cmd.Parameters.AddWithValue("@koleksiID", cbKoleksiID.SelectedValue ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tahun", txtTahunPembuatan.Text);
                cmd.Parameters.AddWithValue("@asal", txtAsalBarang.Text);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM BarangMuseum WHERE BarangID=@id", conn);
                cmd.Parameters.AddWithValue("@id", txtBarangID.Text);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
        }

        private void dataGridBarang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridBarang.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dataGridBarang.SelectedItem;
                txtBarangID.Text = row["BarangID"].ToString();
                txtNamaBarang.Text = row["NamaBarang"].ToString();
                txtTahunPembuatan.Text = row["TahunPembuatan"].ToString();
                txtAsalBarang.Text = row["AsalBarang"].ToString();
                txtDeskripsi.Text = row["Deskripsi"].ToString();
                cbKoleksiID.SelectedValue = row["KoleksiID"];
            }
        }

        private void ClearForm()
        {
            txtBarangID.Text = "";
            txtNamaBarang.Text = "";
            txtTahunPembuatan.Text = "";
            txtAsalBarang.Text = "";
            txtDeskripsi.Text = "";
            cbKoleksiID.SelectedIndex = -1;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
        }
    }
}
