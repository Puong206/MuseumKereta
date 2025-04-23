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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new Page1();

        }
        public void GantiKonten(UserControl control)
        {
            MainContent.Content = control;
        }

        internal void GantiKonten(Kelola_Koleksi kelola_Koleksi)
        {
            GantiKonten((UserControl)kelola_Koleksi);
        }

        internal void GantiKonten(Kelola_Barang kelola_Barang)
        {
            GantiKonten((UserControl)kelola_Barang);
        }

        internal void GantiKonten(Kelola_Pegawai kelola_Pegawai)
        {
            GantiKonten((UserControl)kelola_Pegawai);
        }

        internal void GantiKonten(Kelola_Perawatan kelola_Perawatan)
        {
            GantiKonten((UserControl)kelola_Perawatan);
        }

    }
}
