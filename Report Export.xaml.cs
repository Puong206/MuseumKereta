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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Reporting.WinForms;



namespace MuseumApp
{
    /// <summary>
    /// Interaction logic for Report_Export.xaml
    /// </summary>
    public partial class ReportExport : Page
    {
        //private readonly string connectionString;
        public ReportExport()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            ReportViewer.ProcessingMode = ProcessingMode.Local;

            
            LoadBarangReport();
        }

        private void LoadBarangReport()
        {

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
