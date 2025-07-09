using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

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

            // Jika status tidak kosong (mode edit), pilih item yang sesuai
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
                // Set default untuk mode tambah
                StatusKaryawanComboBox.SelectedIndex = 0;
            }
        }

        public void DisableNIPPInput()
        {
            NIPPTextBox.IsEnabled = false;
        }

        private void NIPPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NamaPegawaiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NamaPegawaiTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex ini mengizinkan huruf (a-z, A-Z), spasi, dan titik.
            //Regex regex = new Regex("[^a-zA-Z .]");
            //e.Handled = regex.IsMatch(e.Text);
        }

        private void StatusKaryawanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            if (NIPPTextBox.IsEnabled && string.IsNullOrWhiteSpace(NIPPTextBox.Text) && string.IsNullOrWhiteSpace(NamaPegawaiTextBox.Text))
            {
                CustomMessageBox.ShowWarning("NIPP dan Nama Karyawan wajib diisi.", "Peringatan");
                return;
            }

            // Cek NIPP satu per satu (hanya untuk add)
            if (NIPPTextBox.IsEnabled && string.IsNullOrWhiteSpace(NIPPTextBox.Text))
            {
                CustomMessageBox.ShowWarning("NIPP tidak boleh kosong.", "Peringatan");
                return;
            }

            // Cek Nama Karyawan
            if (string.IsNullOrWhiteSpace(NamaPegawaiTextBox.Text))
            {
                CustomMessageBox.ShowWarning("Nama Pegawai tidak boleh kosong.", "Peringatan");
                return;
            }

            // Cek Status
            if (StatusKaryawanComboBox.SelectedItem == null)
            {
                CustomMessageBox.ShowWarning("Status Karyawan harus dipilih.", "Peringatan");
                return;
            }

            // Set properti DialogResult menjadi true HANYA jika validasi lolos.
            // Logika untuk menyimpan properti tidak lagi diperlukan di sini karena sudah ada public getter.
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
