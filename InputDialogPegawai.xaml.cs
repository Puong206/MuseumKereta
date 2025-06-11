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
            // Anda bisa menambahkan karakter lain jika diperlukan, misalnya tanda kutip.
            Regex regex = new Regex("[^a-zA-Z .]");

            // e.Handled = true akan MENCEGAH karakter ditampilkan jika tidak cocok dengan pola.
            e.Handled = regex.IsMatch(e.Text);
        }

        private void StatusKaryawanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            // Validasi dasar sebelum menutup
            if (string.IsNullOrWhiteSpace(NIPPTextBox.Text) && NIPPTextBox.IsEnabled)
            {
                MessageBox.Show("NIPP tidak boleh kosong.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(NamaPegawaiTextBox.Text))
            {
                MessageBox.Show("Nama Pegawai tidak boleh kosong.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (StatusKaryawanComboBox.SelectedItem == null)
            {
                MessageBox.Show("Status Karyawan harus dipilih.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
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
