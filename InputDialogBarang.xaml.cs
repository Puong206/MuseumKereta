using System.Windows;

namespace MuseumApp
{
    public partial class InputDialogBarang : Window
    {
        public string BarangID => IDBarangTextBox.Text;
        public string NamaBarang => NamaBarangTextBox.Text;
        public string Deskripsi => DeskripsiTextBox.Text;
        public string TahunPembuatan => TahunPembuatanTextBox.Text;
        public string AsalBarang => AsalBarangTextBox.Text;
        public string KoleksiID => IDKoleksiTextBox.Text;
        public InputDialogBarang()
        {
            InitializeComponent();
        }

        public InputDialogBarang(string barangID, string namaBarang, string deskripsi, string koleksiID, string tahun, string asal)
            : this()
        {
            IDBarangTextBox.Text = barangID;
            txtNamaBarang.Text = namaBarang;
            DeskripsiTextBox.Text = deskripsi;
            IDKoleksiTextBox.Text = koleksiID;
            TahunPembuatanTextBox.Text = tahun;
            AsalBarangTextBox.Text = asal;
        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
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
