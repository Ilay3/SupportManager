using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SupportManager.Views
{
    public partial class AddEditWindow : Window
    {
        public AddEditWindow()
        {
            InitializeComponent();
        }

        // Обработчик для DragOver (чтобы указать, что можно перетащить файл)
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        // Обработчик для Drop (приём перетащенного файла)
        private void TextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    // Обновляем путь к файлу в DataContext
                    ((TextBox)sender).Text = files[0];
                }
                else
                {
                    MessageBox.Show("Только PDF-файлы поддерживаются.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик для вставки файла из буфера обмена (Ctrl+V)
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var clipboardText = Clipboard.GetText();
                if (File.Exists(clipboardText) && Path.GetExtension(clipboardText).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ((TextBox)sender).Text = clipboardText;
                }
                else
                {
                    MessageBox.Show("Только пути к PDF-файлам поддерживаются.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
