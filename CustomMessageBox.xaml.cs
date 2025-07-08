using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace MuseumApp
{
    public partial class CustomMessageBox : Window
    {
        // Enum untuk menentukan jenis pesan (mempengaruhi ikon dan warna)
        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Error
        }

        // Enum untuk menentukan jenis tombol yang ditampilkan
        public enum MessageButtons
        {
            Ok,
            YesNo
        }

        public CustomMessageBox(string message, string title, MessageType type, MessageButtons buttons)
        {
            InitializeComponent();

            // Set teks judul dan pesan
            TitleText.Text = title;
            MessageText.Text = message;

            // Mengatur ikon dan warna berdasarkan jenis pesan
            switch (type)
            {
                case MessageType.Info:
                    IconBackground.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Biru
                    IconText.Text = "\uE946"; // Ikon Info
                    break;

                case MessageType.Success:
                    IconBackground.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Hijau
                    IconText.Text = "\uE8FB"; // Ikon Sukses
                    break;

                case MessageType.Warning:
                    IconBackground.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    IconText.Text = "\uE7BA"; // Ikon Warning
                    break;

                case MessageType.Error:
                    IconBackground.Background = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Merah
                    IconText.Text = "\uEA39"; // Ikon Error/Cancel
                    break;
            }

            switch (buttons)
            {
                case MessageButtons.Ok:
                    AddButton("OK", isPrimary: true, dialogResult: true);
                    break;

                case MessageButtons.YesNo:
                    AddButton("Tidak", isPrimary: false, dialogResult: false);
                    AddButton("Ya", isPrimary: true, dialogResult: true);
                    break;
            }
        }

        // Metode helper untuk mennambahkan tombol secara dinamis
        private void AddButton(string content, bool isPrimary, bool dialogResult)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = content,
                Style = (Style)FindResource(isPrimary ? "PrimaryButtonStyle" : "SecondaryButtonStyle")
            };

            button.Click += (o, e) =>
            {
                this.DialogResult = dialogResult;
                this.Close();
            };

            ButtonArea.Children.Add(button);
        }

        // Metode untuk memungkinkan window di-drag
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Metode Statis untuk Mempermudah Pemanggilan
        public static void ShowInfo(string message, string title = "Informasi")
        {
            new CustomMessageBox(message, title, MessageType.Info, MessageButtons.Ok).ShowDialog();
        }

        public static void ShowSuccess(string message, string title = "Sukses")
        {
            new CustomMessageBox(message, title, MessageType.Success, MessageButtons.Ok).ShowDialog();
        }

        public static void ShowWarning(string message, string title = "Peringatan")
        {
            new CustomMessageBox(message, title, MessageType.Warning, MessageButtons.Ok).ShowDialog();
        }

        public static void ShowError(string message, string title = "Error")
        {
            new CustomMessageBox(message, title, MessageType.Error, MessageButtons.Ok).ShowDialog();
        }

        public static bool ShowYesNo(string question, string title = "Konfirmasi")
        {
            var dialog = new CustomMessageBox(question, title, MessageType.Warning, MessageButtons.YesNo);
            return dialog.ShowDialog() ?? false;
        }
    }
}
