using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MuseumApp
{
    public partial class InputDialog : Window
    {
        public string JenisKoleksi { get; private set; }
        public string Deskripsi { get; private set; }

        // Constructor untuk mode "Tambah"
        public InputDialog()
        {
            InitializeComponent();
            JenisTextBox.Focus();
        }

        // Constructor untuk mode "Edit"
        public InputDialog(string jenisAwal, string deskripsiAwal)
        {
            InitializeComponent();

            // Mengisi field dengan data yang ada
            JenisTextBox.Text = jenisAwal;
            DeskripsiTextBox.Text = deskripsiAwal;
            JenisTextBox.Focus();
        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            string jenisKoleksi = JenisTextBox.Text.Trim();
            string deskripsi = DeskripsiTextBox.Text.Trim();

            // Validasi input sebelum menutup dialog
            Regex regex = new Regex("^[a-zA-Z0-9 ]+$");
            if (string.IsNullOrWhiteSpace(jenisKoleksi) || !regex.IsMatch(jenisKoleksi))
            {
                CustomMessageBox.ShowWarning("Jenis Koleksi harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal");
                return; // Jangan tutup dialog jika validasi gagal
            }
            if (string.IsNullOrWhiteSpace(deskripsi))
            {
                CustomMessageBox.ShowWarning("Deskripsi tidak boleh kosong.", "Peringatan");
                return; // Jangan tutup dialog jika validasi gagal
            }

            // Jika validasi berhasil, set properti dan tutup dialog
            this.JenisKoleksi = jenisKoleksi;
            this.Deskripsi = deskripsi;
            this.DialogResult = true;
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Cukup set DialogResult, window akan tertutup otomatis
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Logika untuk memungkinkan window di-drag
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }

}
