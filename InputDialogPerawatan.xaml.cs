using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MuseumApp
{
    public partial class InputDialogPerawatan : Window
    {
        public string IDBarang => IDBarangTextBox.Text;
        public DateTime TanggalPerawatan => dpTanggalPerawatan.SelectedDate ?? DateTime.Today;
        public string JenisPerawatan => JenisPerawatanTextBox.Text;
        public string Catatan => CatatanTextBox.Text;
        public string NIPP => NIPPTextBox.Text;
        public InputDialogPerawatan(string idBarang = "", DateTime? tanggalPerawatan = null, string jenis = "", string catatan = "", string nipp = "")
        {
            InitializeComponent();
            IDBarangTextBox.Text = idBarang;
            dpTanggalPerawatan.SelectedDate = tanggalPerawatan ?? DateTime.Today;
            JenisPerawatanTextBox.Text = jenis;
            CatatanTextBox.Text = catatan;
            NIPPTextBox.Text = nipp;

            IDBarangTextBox.Focus();
        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            // --- Validasi Input ---
            string idBarang = IDBarangTextBox.Text.Trim();
            string nipp = NIPPTextBox.Text.Trim();
            string jenisPerawatan = JenisPerawatanTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(idBarang) || idBarang.Length != 5 || !idBarang.All(char.IsDigit))
            {
                CustomMessageBox.ShowWarning("ID Barang harus diisi dan terdiri dari 5 digit angka.", "Validasi Gagal");
                return;
            }
            if (dpTanggalPerawatan.SelectedDate == null)
            {
                CustomMessageBox.ShowWarning("Tanggal Perawatan harus diisi.", "Validasi Gagal");
                return;
            }
            if (string.IsNullOrWhiteSpace(jenisPerawatan))
            {
                CustomMessageBox.ShowWarning("Jenis Perawatan tidak boleh kosong.", "Validasi Gagal");
                return;
            }
            if (string.IsNullOrWhiteSpace(nipp) || nipp.Length != 5 || !nipp.All(char.IsDigit))
            {
                CustomMessageBox.ShowWarning("NIPP harus diisi dan terdiri dari 5 digit angka.", "Validasi Gagal");
                return;
            }

            // Jika semua validasi lolos, tutup dialog
            DialogResult = true;
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
