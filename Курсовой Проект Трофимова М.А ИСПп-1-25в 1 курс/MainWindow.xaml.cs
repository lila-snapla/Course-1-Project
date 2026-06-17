using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Identity.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Курсовая_работа_1_семестр;
using Курсовая_работа_1_семестр.для_работы_с_файлами;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.для_работы_с_данными;


namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public App _app = (App)Application.Current;
        private ObservableCollection<DocumentsLog> _documents = new();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public ObservableCollection<DocumentsLog> Documents
        {
            get => _documents;
            set { _documents = value; OnPropertyChanged(); }
        } 
        private string filePath;
        public string logName;
        private DocumentsLog _selectedDocument;
        public DocumentsLog SelectedDocument
        {
            get => _selectedDocument;
            set { _selectedDocument = value; OnPropertyChanged(); }
        } 
        public string FilePath
        {
            get => filePath;
            set { filePath = value; OnPropertyChanged(); }
        }
        public string LogName
        {
            get => logName;
            set { logName = value; OnPropertyChanged(); }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadCategories();
        }
        public MainWindow(string filePath) : this()
        {
            FilePath = filePath;
            LogName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        }
        public MainWindow(string logName,string filePath) : this()
        {
            FilePath = filePath;
            LogName = logName;
        }
        private async void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock text) return;

            string rawTag = text.Tag?.ToString() ?? "";
            string cleanTag = new string(rawTag.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(cleanTag) || !int.TryParse(cleanTag, out int key))
            {
                MessageBox.Show($"Не удалось распарсить тег: '{rawTag}' (чистый: '{cleanTag}')");
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
                string fullPath = _app.GetPath(selectedDoc.MainTree);
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    Статьи page = new(fullPath);
                    nextPage.Navigate(page);

                    App.doc = selectedDoc;
                    SelectedDocument = selectedDoc;
                }
                else
                    MessageBox.Show($"Файл не найден по пути: {selectedDoc.MainTree}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private async Task LoadCategories()
        {
            try
            {
                var allDocs = await App.repository.GetAllDocuments();
                Documents.Clear();
                foreach (var doc in allDocs) Documents.Add(doc);
                BuildTreeView(allDocs);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки: {ex.Message}"); }
        }
        private void BuildTreeView(List<DocumentsLog> docs)
        {
           foreach (TreeViewItem item in Категории.Items)
            {
                if (item.Tag is int id)
                {
                    item.Items.Clear();

                    var filtered = docs.Where(d => d.LogsId == id);

                    foreach (var doc in filtered)
                    {
                        var docItem = new TreeViewItem()
                        {
                            Header = doc.LogsName,
                            Tag = doc.LogsId
                        };
                        item.Items.Add(docItem);
                    }
                }
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
            var selectedItem = Категории.SelectedItem as TreeViewItem;
            var message = MessageBox.Show("Все статьи данной ветви будут удалены.Продолжить?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (selectedItem == null) return;
            if (message != MessageBoxResult.Yes) return;
            
            try
            {
                var ids = GetAllChildren(selectedItem);

                if (ids.Any())
                {
                    foreach (int id in ids)
                        await App.repository.DeleteDocument(id);
                }               
                DeleteTreeViewItem(selectedItem);
                await LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
        private async void renameTree_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null) { MessageBox.Show("Вы не выбрали статью"); return; }
            var window = new AddOrEdit(SelectedDocument.LogsName);
            window.ShowDialog();
            string NewName = window.Name;
            if (string.IsNullOrEmpty(NewName)) { MessageBox.Show("Выберите название"); return; }

            bool edited = await App.repository.EditDocument(SelectedDocument.LogsId, NewName, SelectedDocument.MainTree);
            if (edited) await LoadCategories();
        }

        private async void AddPrint_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrEdit();
            window.ShowDialog();
            string NewName = window.Name;
            string NewFullPath = window.FullPath;
            if (string.IsNullOrEmpty(NewName) || string.IsNullOrEmpty(NewFullPath)) { MessageBox.Show("Пожалуйста, заполните поля"); return; }
            bool added = await App.repository.AddDocument(NewName, NewFullPath);
            
            if (added) await LoadCategories();
        }

        private async void RemovePrint_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = await App.repository.FindDocument(App.doc.LogsId);
            if (selectedItem == null) { MessageBox.Show("Вы не выбрали статью"); return; }
            var confirm = MessageBox.Show($"Удалить статью '{SelectedDocument.LogsName}'?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
            bool deleted = await App.repository.DeleteDocument(App.doc.LogsId);
            
            if (deleted) { await LoadCategories(); SelectedDocument = null; }
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

            if (parent != null)
                parent.Items.Remove(item);
            else Категории.Items.Remove(item);
        }
        private TreeViewItem GetParentTreeViewItem(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (parent != null && !(parent is TreeViewItem)) 
                parent = VisualTreeHelper.GetParent(parent);
            
            return parent as TreeViewItem;
        }

        private async void AddTreeItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrEdit();
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var parentMenu = contextMenu?.PlacementTarget as TreeViewItem;

            window.ShowDialog();

            if (window.DialogResult != true) return;

            string newCategoryName = window.Name;
            if (string.IsNullOrEmpty(newCategoryName)) { MessageBox.Show("Выберите название ветки"); return; }

            var newItem = new TreeViewItem
            {
                Header = newCategoryName,
                IsExpanded = true
            };

            parentMenu.Items.Add(newItem);
            parentMenu.IsExpanded = true;

            await LoadCategories();
        }
        
        private void Категории_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private async void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (item?.Tag is int Id)
            {
                var filtered = await App.repository.FindDocument(Id);

                if (filtered != null)
                {
                    string fullPath = _app.GetPath(filtered.MainTree);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        var page = new Статьи(fullPath);
                        nextPage.Navigate(page);
                        App.doc = filtered;
                        SelectedDocument = filtered;
                    }
                }
                else MessageBox.Show($"Статья не найдена по пути {App.doc.MainTree}");
            }
        }
    }
}