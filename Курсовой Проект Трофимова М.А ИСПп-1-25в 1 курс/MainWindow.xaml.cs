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
using VisioForge.Core.ONVIFX.Analytics;
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
                    var winMenu = (ContextMenu)Resources["menu"];

                    foreach (var doc in filtered)
                    {
                        var docItem = new TreeViewItem()
                        {
                            Header = doc.LogsName,
                            Tag = doc.LogsId,
                            ContextMenu = winMenu
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
            var selectedItem = Категории.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите ветку");
                return;
            }

            var window = new AddOrEdit(selectedItem.Header?.ToString());
            window.ShowDialog();

            if (window.DialogResult != true) { MessageBox.Show("Не удалось переименовать ветку");  return; }

            string newName = window.Name;
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Введите название");
                return;
            }

            selectedItem.Header = newName;
        }

        private async void AddPrint_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var parentItem = contextMenu?.PlacementTarget as TreeViewItem;

            if (parentItem == null)
            {
                parentItem = Категории.SelectedItem as TreeViewItem;
            }

            if (parentItem == null)
            {
                MessageBox.Show("Выберите ветку для добавления статьи");
                return;
            }

            var window = new AddOrEdit();
            window.ShowDialog();

            if (window.DialogResult != true) return;

            string newName = window.Name;
            string newPath = window.FullPath;

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newPath))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            int newId = Convert.ToInt32(await App.repository.AddDocument(newName, newPath));

            if (newId == 0)
            {
                MessageBox.Show("Не удалось добавить статью в БД");
                return;
            }
            var winMenu = (ContextMenu)Resources["menu"];
            var newItem = new TreeViewItem
            {
                Header = newName,
                Tag = newId,
                IsExpanded = true,
                ContextMenu = winMenu,
                Foreground = Brushes.DarkGreen
            };

            parentItem.Items.Add(newItem);
            parentItem.IsExpanded = true;

            await LoadCategories();

            var doc = await App.repository.FindDocument(newId);
            if (doc != null)
            {
                string fullPath = _app.GetPath(doc.MainTree);
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    var page = new Статьи(fullPath);
                    nextPage.Navigate(page);
                    App.doc = doc;
                    SelectedDocument = doc;
                }
                else { MessageBox.Show($"Не удалось открыть статью по пути {fullPath}"); return; }
            }
        }

        private async void RemovePrint_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var parentItem = contextMenu?.PlacementTarget as TreeViewItem;

            if (parentItem == null)
            {
                parentItem = Категории.SelectedItem as TreeViewItem;
            }

            if (parentItem == null)
            {
                MessageBox.Show("Выберите статью для удаления");
                return;
            }

            if (!(parentItem.Tag is int id))
            {
                MessageBox.Show("Не удалось определить ID статьи");
                return;
            }

            var confirm = MessageBox.Show($"Удалить статью '{parentItem.Header}'?",
                                          "Подтверждение",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            bool deleted = await App.repository.DeleteDocument(id);
            if (deleted)
            {
                var parent = GetParentTreeViewItem(parentItem);
                if (parent != null)
                    parent.Items.Remove(parentItem);
                else
                    Категории.Items.Remove(parentItem);
            }
            else
            {
                MessageBox.Show("Не удалось удалить статью");
            }
            await LoadCategories();
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
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var parentItem = contextMenu?.PlacementTarget as TreeViewItem;

            if (parentItem == null)
            {
                parentItem = Категории.SelectedItem as TreeViewItem;
            }

            var window = new AddOrEdit();
            window.ShowDialog();

            if (window.DialogResult != true) return;

            string newName = window.Name;
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Введите название");
                return;
            }
            var winMenu = (ContextMenu)Resources["menu"];
            var newItem = new TreeViewItem
            {
                Header = newName,
                IsExpanded = true,
                ContextMenu = winMenu
            };

            if (parentItem != null)
            {
                parentItem.Items.Add(newItem);
                parentItem.IsExpanded = true;
            }
            else
            {
                Категории.Items.Add(newItem);
            }
        }

        private async void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item == null) return;
            if (e.OriginalSource is System.Windows.Controls.Primitives.ToggleButton)
                { MessageBox.Show("Это корневой элемент"); return;  }
            if (item.Tag == null)
            {
                MessageBox.Show("Tag отсутствует");
                return;
            }

            string rawTag = item.Tag.ToString();
            string cleanTag = new string(rawTag.Where(char.IsDigit).ToArray());

            if (!int.TryParse(cleanTag, out int id))
            {
                MessageBox.Show($"Не удалось распарсить Tag: '{rawTag}' (очищенный: '{cleanTag}')");
                return;
            }

            try
            {
                var doc = await App.repository.FindDocument(id);
                if (doc == null)
                {
                    MessageBox.Show($"Статья не найдена");
                    return;
                }

                string fullPath = _app.GetPath(doc.MainTree);

                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    MessageBox.Show($"Файл не найден: {doc.MainTree}");
                    return;
                }

                App.doc = doc;
                SelectedDocument = doc;
                var page = new Статьи(fullPath);
                nextPage.Navigate(page);             
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}