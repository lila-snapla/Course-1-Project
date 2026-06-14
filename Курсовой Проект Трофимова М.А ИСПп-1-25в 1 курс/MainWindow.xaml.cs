using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Курсовая_работа_1_семестр;
using Курсовая_работа_1_семестр.для_работы_с_файлами;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.для_работы_с_данными;


namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<DocumentsLog> documents { get; } = new();
        public string filePath { get; private set; } 
        public string logName { get; private set; } 
        
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(string filePath) : this()
        {
            this.filePath = filePath;
            this.logName = Path.GetFileNameWithoutExtension(filePath);
        }
        public MainWindow(string logName,string filePath) : this()
        {
            this.filePath = filePath;
            this.logName = logName;
        }
        private async void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock text) return;
            if (!int.TryParse(text.Tag?.ToString(), out int key))
            {
                MessageBox.Show("Документ не найден");
                return;
            }

            try
            {
                var selectedDoc = await App.repository.FindDocument(key);

                if (selectedDoc == null)
                {
                    MessageBox.Show("Документ не найден");
                    return;
                }

                if (!string.IsNullOrEmpty(selectedDoc.MainTree) && File.Exists(selectedDoc.MainTree))
                {
                    Статьи page = new(selectedDoc.MainTree);
                    nextPage.Navigate(page);
                }
                else
                    MessageBox.Show($"Файл не найден по пути: {selectedDoc?.MainTree}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private List<int> GetAllChildren(TreeViewItem item)
        {
            var ids = new List<int>();

            if (item.Tag is int id)
                ids.Add(id);

            foreach (TreeViewItem child in item.Items) ids.AddRange(GetAllChildren(child));
            return ids;
        }

        private async void RemoveTreeItem_Click(object sender, RoutedEventArgs e)
        {
            var message = MessageBox.Show("Все статьи данной ветви будут удалены.Продолжить?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (message != MessageBoxResult.Yes) return;
            var selectedItem = Категории.SelectedItem as TreeViewItem;
            if (selectedItem == null) return;

            try
            {
                var ids = GetAllChildren(selectedItem);

                foreach (int id in ids)
                    await App.repository.DeleteDocument(id);

                var currentWindow = Window.GetWindow(this);
                var newWindow = new MainWindow();
                currentWindow.Close();
                newWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
        private void renameTree_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrEdit(Name);
            window.ShowDialog();

            var currentWindow = Window.GetWindow(this);
            var newWindow = new MainWindow();
            currentWindow.Close();
            newWindow.Show();
        }

        private void AddPrint_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrEdit(Name, filePath);
            window.ShowDialog();

            var currentWindow = Window.GetWindow(this);
            var newWindow = new MainWindow();
            currentWindow.Close();
            newWindow.Show();
        }

        private async void RemovePrint_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = Категории.SelectedItem as TreeViewItem;
            if (selectedItem == null) return;
            int findId = Convert.ToInt32(App.repository.FindDocument(App.doc.LogsId));
            var delete = App.repository.DeleteDocument(findId);

            var currentWindow = Window.GetWindow(this);
            var newWindow = new MainWindow();
            currentWindow.Close();
            newWindow.Show();
        }

        private void EmailPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteTreeViewItem(TreeViewItem item)
        {
            while (item.Items.Count > 0)
            {
                var child = item.Items[0] as TreeViewItem;

                if (child != null)
                    DeleteTreeViewItem(child);
                else
                    item.Items.RemoveAt(0);
            }

            var parent = GetParentTreeViewItem(item);
            var dataItem = item.Tag as object;

            if (parent != null)
                parent.Items.Remove(dataItem);
            else Категории.Items.Remove(item);
        }
        private TreeViewItem GetParentTreeViewItem(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (parent != null && !(parent is TreeViewItem)) 
                parent = VisualTreeHelper.GetParent(parent);
            
            return parent as TreeViewItem;
        }

        private void AddTreeItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrEdit(Name);
            window.ShowDialog();
            Категории.Items.Add(Name);

            var currentWindow = Window.GetWindow(this);
            var newWindow = new MainWindow();
            currentWindow.Close();
            newWindow.Show();
        }

        private void Категории_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
    public class Categories : INotifyPropertyChanged
    {
        private DocumentsLog _selectedDocument;
        public Repository _repository;
        public RelayCommand OpenDocumentCommand { get; private set; }
        public ObservableCollection<DocumentsLog> documents { get; } = new();
        private int _currentPage = 1;

        public Categories(DocumentsLog document, Repository repository)
        {
            _selectedDocument = document;
            _repository = repository;
            OpenDocumentCommand = new RelayCommand(() => OpenDocument());
        }
        public DocumentsLog SelectedDocument
        {
            get => _selectedDocument;
            set { _selectedDocument = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        private void OpenDocument()
        {
            if (_selectedDocument == null || string.IsNullOrEmpty(_selectedDocument.MainTree)) return;

            try
            {
                Статьи page = new(_selectedDocument.MainTree);
                byte[] fileBytes = System.IO.File.ReadAllBytes(_selectedDocument.MainTree);

                FlowDocument document = WorkWithRTF.LoadRtf(fileBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла" + "\n" + ex.Message);
            }
        }
    }
}