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
        public string IDBarang => txtIDBarang.Text;
        public DateTime TanggalPerawatan => (dpTanggalPerawatan.SelectedDate ?? DateTime.Now).Date;
        public string JenisPerawatan => txtJenisPerawatan.Text;
        public string Catatan => txtCatatan.Text;
        public string NIPP => txtNIPP.Text;
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
            if (string.IsNullOrWhiteSpace(IDBarang) || dpTanggalPerawatan.SelectedDate == null ||
                string.IsNullOrWhiteSpace(JenisPerawatan) || string.IsNullOrWhiteSpace(Catatan) || string.IsNullOrWhiteSpace(NIPP))
            {
                MessageBox.Show("Harap lengkapi semua data.");
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
    }
}
