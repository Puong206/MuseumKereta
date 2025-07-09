using System.Linq;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuseumApp
{
    public partial class InputDialogPegawai : Window
    {
        public string NIPP => NIPPTextBox.Text;
        public string NamaKaryawan => NamaPegawaiTextBox.Text;
        public string StatusKaryawan => (StatusKaryawanComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        public InputDialogPegawai(string nipp = "", string nama = "", string status = "")
        {
            InitializeComponent();
            NIPPTextBox.Text = nipp;
            NamaPegawaiTextBox.Text = nama;

            // Jika dalam mode edit, nonaktifkan input NIPP
            if (!string.IsNullOrEmpty(nipp))
            {
                NIPPTextBox.IsEnabled = false;
                NamaPegawaiTextBox.Focus();
            }
            else
            {
                NIPPTextBox.Focus();
            }

            // Pilih item ComboBox yang sesuai
            if (!string.IsNullOrEmpty(status))
            {
                foreach (ComboBoxItem item in StatusKaryawanComboBox.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        StatusKaryawanComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                // Set default untuk mode tambah jika belum ada yang terpilih
                if (StatusKaryawanComboBox.SelectedIndex == -1)
                {
                    StatusKaryawanComboBox.SelectedIndex = 0;
                }
            }
        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            // --- Validasi Input ---
            string nipp = NIPPTextBox.Text.Trim();
            string nama = NamaPegawaiTextBox.Text.Trim();

            // Validasi NIPP (hanya jika dalam mode 'Tambah')
            if (NIPPTextBox.IsEnabled && (string.IsNullOrWhiteSpace(nipp) || nipp.Length != 5 || !nipp.All(char.IsDigit)))
            {
                CustomMessageBox.ShowWarning("NIPP harus diisi dan terdiri dari 5 digit angka.", "Validasi Gagal");
                return;
            }

            // Validasi Nama
            Regex regex = new Regex("^[a-zA-Z .']+$"); // Izinkan huruf, spasi, titik, dan apostrof
            if (string.IsNullOrWhiteSpace(nama) || !regex.IsMatch(nama))
            {
                CustomMessageBox.ShowWarning("Nama Pegawai harus diisi dan hanya boleh berisi huruf dan spasi.", "Validasi Gagal");
                return;
            }

            // Validasi Status
            if (StatusKaryawanComboBox.SelectedItem == null)
            {
                CustomMessageBox.ShowWarning("Status Karyawan harus dipilih.", "Validasi Gagal");
                return;
            }

            // Jika semua validasi lolos, tutup dialog
            this.DialogResult = true;
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void StatusKaryawanComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
