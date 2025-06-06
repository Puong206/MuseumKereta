using System;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
    public partial class InputDialogBarang : Window
    {
        public string BarangID { get; private set; }
        public string NamaBarang { get; private set; }
        public string Deskripsi { get; private set; }
        public string TahunPembuatan { get; private set; }
        public string AsalBarang { get; private set; }
        public string KoleksiID { get; private set; }

        

      

        public InputDialogBarang()
        {
            InitializeComponent();
            IDBarangTextBox.Focus();
        }

        public InputDialogBarang(string barangID, string namaBarang, string deskripsi, string koleksiID, string tahun, string asal)
        {
            InitializeComponent();
            IDBarangTextBox.Text = barangID;
            NamaBarangTextBox.Text = namaBarang;
            DeskripsiTextBox.Text = deskripsi;
            IDKoleksiTextBox.Text = koleksiID;
            TahunPembuatanTextBox.Text = tahun;
            AsalBarangTextBox.Text = asal;

            IDBarangTextBox.IsEnabled = false;
            NamaBarangTextBox.Focus();
        }



        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            this.BarangID = IDBarangTextBox.Text;
            this.NamaBarang = NamaBarangTextBox.Text;
            this.Deskripsi = DeskripsiTextBox.Text;
            this.KoleksiID = IDKoleksiTextBox.Text;
            this.TahunPembuatan = TahunPembuatanTextBox.Text;
            this.AsalBarang = AsalBarangTextBox.Text;

            this.DialogResult = true;
        }

        private void Batal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
