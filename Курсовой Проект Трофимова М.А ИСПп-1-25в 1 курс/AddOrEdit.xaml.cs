using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс
{
    /// <summary>
    /// Логика взаимодействия для AddOrEdit.xaml
    /// </summary>
    public partial class AddOrEdit : Window
    {
        public AddOrEdit()
        {
            InitializeComponent();
            
        }
        public AddOrEdit(string name) : this()
        {
            if (!string.IsNullOrEmpty(Name)) logName.Text = Name; Name = name;
        }
        public AddOrEdit(string name, string fullPath) : this()
        {
            if (!string.IsNullOrEmpty(Name)) { logName.Text = Name; mainTree.Text = FullPath; Name = name; FullPath = fullPath; }
        }
        public AddOrEdit(string name, bool isRenameOnly = false)
        {
            InitializeComponent();
            logName.Text = name;
            mainTree.Visibility = isRenameOnly ? Visibility.Collapsed : Visibility.Visible;
        }
        public string FullPath { get; set; }
        public string Name { get; set; }
        private void show_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new();
                ofd.Filter = "RTF документ (*.rtf)|*.rtf";

                if (ofd.ShowDialog() == true)
                {
                    string fullPath = ofd.FileName; 
                    string name = System.IO.Path.GetFileName(fullPath);

                    FullPath = fullPath;
                    Name = name;

                    mainTree.Text = fullPath;
                    logName.Text = name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private async void AddPageBttn_Click(object sender, RoutedEventArgs e)
        {
            Name = logName.Text.Trim();
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(FullPath)) { MessageBox.Show("Выберите имя и путь"); return; }
            await App.repository.AddDocument(Name, FullPath);
            var window = new MainWindow(FullPath, Name);

            DialogResult = true;
            this.Close();
        }

        private async void EditBttn_Click(object sender, RoutedEventArgs e)
        {
            Name = logName.Text.Trim();

            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Введите название");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void AddTreeBttn_Click(object sender, RoutedEventArgs e)
        {
            Name = logName.Text.Trim();
            if (string.IsNullOrEmpty(Name)) { MessageBox.Show("Выберите имя"); return; }
            DialogResult = true;
            this.Close();
        }
    }
}
