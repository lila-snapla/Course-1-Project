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

        }
        public AddOrEdit(string name, string fullPath) : this()
        {

        }
        public string fullPath { get; set; }
        public string Name { get; set; }
        private void show_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new();
                ofd.Filter = "RTF документ (*.rtf)|*.rtf";

                if (ofd.ShowDialog() == true)
                {
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        string path = System.IO.Path.GetDirectoryName(ofd.FileName);
                        string name = System.IO.Path.GetFileName(ofd.FileName);

                        fullPath = path + name;
                        Name = name;

                        mainTree.Text = fullPath;
                        logName.Text = name;
                        var window = new MainWindow(fullPath, Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Расширение не поддерживается данным приложением \n " +
                    $"Код ошибки: {ex.Message}", "Не удалось открыть документ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddPageBttn_Click(object sender, RoutedEventArgs e)
        {
            App.repository.AddDocument(Name, fullPath);
            var window = new MainWindow(fullPath, Name);
            this.Close();
        }

        private void EditBttn_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(App.repository.FindDocument(App.doc.LogsId));
            App.repository.EditDocument(id, Name, fullPath);
            var window = new MainWindow(Name);
            this.Close();
        }

        private void AddTreeBttn_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow(fullPath, Name);
            this.Close();
        }
    }
}
