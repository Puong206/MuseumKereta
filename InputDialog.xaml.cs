using System.Windows;

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

        private void JenisTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void DeskripsiTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            JenisKoleksi = JenisTextBox.Text;
            Deskripsi = DeskripsiTextBox.Text;

            if (string.IsNullOrWhiteSpace(JenisKoleksi))
            {
                MessageBox.Show("Jenis koleksi tidak boleh kosong.");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

}
