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
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
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
    }

}
