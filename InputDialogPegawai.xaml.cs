using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuseumApp
{
    public partial class InputDialogPegawai : Window
    {
        public string NIPP => NIPPTextBox.Text;
        public string NamaKaryawan => NamaPegawaiTextBox.Text;
        public string StatusKaryawan => (StatusKaryawanComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        public InputDialogPegawai(string nipp = "", string nama = "", string status = "")
        {
            InitializeComponent();
            NIPPTextBox.Text = nipp;
            NamaPegawaiTextBox.Text = nama;

            foreach (ComboBoxItem item in StatusKaryawanComboBox.Items)
            {
                if (item.Content.ToString() == status)
                {
                    StatusKaryawanComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        public void DisableNIPPInput()
        {
            NIPPTextBox.IsEnabled = false;
        }

        private void NIPPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NamaPegawaiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void StatusKaryawanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

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

        private void StatusKaryawanComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
