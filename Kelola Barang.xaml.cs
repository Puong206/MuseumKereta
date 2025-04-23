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
    public partial class Kelola_Barang : Page
    {
        private string connectionString;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataAdapter adapter;
        private DataTable dt;

        // Dummy controls yang tidak ada di XAML
        TextBox txtBarangID = new TextBox();
        TextBox txtNamaBarang = new TextBox();
        TextBox txtTahunPembuatan = new TextBox();
        TextBox txtAsalBarang = new TextBox();
        TextBox txtDeskripsi = new TextBox();
        ComboBox cbKoleksiID = new ComboBox(); // jika kamu pakai relasi Koleksi

        public Kelola_Barang(string connStr)
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
            finally
            {
                conn.Close();
            }
        }

        private void BtnTambah_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialogBarang();
            if (inputDialog.ShowDialog() == true)
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO BarangMuseum VALUES (@id, @nama, @deskripsi, @koleksiID, @tahun, @asal)", conn);
                    cmd.Parameters.AddWithValue("@id", inputDialog.BarangID);
                    cmd.Parameters.AddWithValue("@nama", inputDialog.NamaBarang);
                    cmd.Parameters.AddWithValue("@deskripsi", inputDialog.Deskripsi);
                    cmd.Parameters.AddWithValue("@koleksiID", inputDialog.KoleksiID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@tahun", inputDialog.TahunPembuatan);
                    cmd.Parameters.AddWithValue("@asal", inputDialog.AsalBarang);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridBarang.SelectedItem is DataRowView row)
            {
                var inputDialog = new InputDialogBarang(
                    row["BarangID"].ToString(),
                    row["NamaBarang"].ToString(),
                    row["Deskripsi"].ToString(),
                    row["KoleksiID"] == DBNull.Value ? null : row["KoleksiID"].ToString(),
                    row["TahunPembuatan"].ToString(),
                    row["AsalBarang"].ToString());

                if (inputDialog.ShowDialog() == true)
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE BarangMuseum SET NamaBarang=@nama, Deskripsi=@deskripsi, KoleksiID=@koleksiID, TahunPembuatan=@tahun, AsalBarang=@asal WHERE BarangID=@id", conn);
                        cmd.Parameters.AddWithValue("@id", inputDialog.BarangID);
                        cmd.Parameters.AddWithValue("@nama", inputDialog.NamaBarang);
                        cmd.Parameters.AddWithValue("@deskripsi", inputDialog.Deskripsi);
                        cmd.Parameters.AddWithValue("@koleksiID", inputDialog.KoleksiID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@tahun", inputDialog.TahunPembuatan);
                        cmd.Parameters.AddWithValue("@asal", inputDialog.AsalBarang);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                        LoadData();
                    }
                }
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
            NavigationService.Navigate(new Page1(connectionString));
        }
    }
}
