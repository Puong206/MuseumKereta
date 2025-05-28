using System;
using System.Windows;
using System.Windows.Controls;

namespace MuseumApp
{
    public partial class InputDialogBarang : UserControl
    {
        public event EventHandler SaveClicked;
        public event EventHandler CancelClicked;

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
            LoadDataForEdit(barangID, namaBarang, deskripsi, koleksiID, tahun, asal);
        }

        public void LoadDataForEdit(string barangID, string namaBarang, string deskripsi, string koleksiID, string tahun, string asal)
        {
            IDBarangTextBox.Text = barangID;
            NamaBarangTextBox.Text = namaBarang;
            DeskripsiTextBox.Text = deskripsi;
            IDKoleksiTextBox.Text = koleksiID;
            TahunPembuatanTextBox.Text = tahun;
            AsalBarangTextBox.Text = asal;
            IDBarangTextBox.IsEnabled = false;
        }

        public void ClearForm()
        {
            IDBarangTextBox.Text = string.Empty;
            NamaBarangTextBox.Text = string.Empty;
            DeskripsiTextBox.Text = string.Empty;
            IDKoleksiTextBox.Text = string.Empty;
            TahunPembuatanTextBox.Text = string.Empty;
            AsalBarangTextBox.Text = string.Empty;
            IDBarangTextBox.IsEnabled = true;
        }

        public void EnableBarangIDInput()
        {
            IDBarangTextBox.IsEnabled = true;
        }

        public void DisableBarangIDInput()
        {
            IDBarangTextBox.IsEnabled = false;
        }

        private void SimpanButton_Click(object sender, RoutedEventArgs e)
        {
            SaveClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BatalButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
