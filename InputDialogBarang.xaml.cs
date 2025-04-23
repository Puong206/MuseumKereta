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
    public partial class InputDialogBarang : Window
    {
        public string BarangID => IDBarangTextBox.Text;
        public string NamaBarang => NamaBarangTextBox.Text;
        public string Deskripsi => DeskripsiBarang.Text;
        public string TahunPembuatan => TahunPembuatanBarang.Text;
        public string AsalBarang => AsalBarangED.Text;
        public string KoleksiID => cbKoleksiID.SelectedValue?.ToString();
        public InputDialogBarang()
        {
            InitializeComponent();
        }
    }
}
