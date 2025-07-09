using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MuseumApp
{
    public partial class InputDialog : Window
    {
        public string JenisKoleksi { get; private set; }
        public string Deskripsi { get; private set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string jenisAwal, string deskripsiAwal)
        {
            InitializeComponent();

            
            JenisTextBox.Text = jenisAwal;
            DeskripsiTextBox.Text = deskripsiAwal;
        }

        private void JenisTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void DeskripsiTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            string jenisKoleksi = JenisTextBox.Text.Trim();
            string deskripsi = DeskripsiTextBox.Text.Trim();
            Regex regex = new Regex("^[a-zA-Z0-9 ]+$");
            if (string.IsNullOrWhiteSpace(jenisKoleksi) || !regex.IsMatch(jenisKoleksi))
            {
                CustomMessageBox.ShowWarning("Jenis Koleksi harus diisi dan hanya boleh berisi huruf, angka, dan spasi.", "Validasi Gagal");
                return;
            }
            if (string.IsNullOrWhiteSpace(deskripsi))
            {
                CustomMessageBox.ShowWarning("Deskripsi tidak boleh kosong.", "Peringatan");
                return;
            }

            this.JenisKoleksi = jenisKoleksi;
            this.Deskripsi = deskripsi;


            this.DialogResult = true;
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
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
