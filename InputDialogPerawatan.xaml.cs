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
        public DateTime TanggalPerawatan => (dpTanggalPerawatan.SelectedDate ?? DateTime.Now).Date;
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
        }

        private void Simpan_Click(object sender, RoutedEventArgs e)
        {
            if (IDBarang.Length != 5 || !IDBarang.All(char.IsDigit))
            {
                CustomMessageBox.ShowWarning("ID Barang harus 5 digit");
                return;
            }

            if (string.IsNullOrWhiteSpace(IDBarang) || dpTanggalPerawatan.SelectedDate == null ||
                string.IsNullOrWhiteSpace(JenisPerawatan) || string.IsNullOrWhiteSpace(Catatan) || string.IsNullOrWhiteSpace(NIPP))
            {
                CustomMessageBox.ShowWarning("Harap lengkapi semua data.");
                return;
            }

            DialogResult = true;
            Close();
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
