using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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
using System.Xml;
using Курсовая_работа_1_семестр.для_работы_с_файлами;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;

namespace Курсовая_работа_1_семестр
{
    /// <summary>
    /// Логика взаимодействия для Статьи.xaml
    /// </summary>
    public partial class Статьи : Page
    {
        /* 
         
        надо немного доработать:

        1.добавить возможность писать с правого края и с центра
        2.Добавить возможность добавлять и убирать отступы перед абзацами и после
        3.реализовать режим чтения
        4.сделать дизайн более автономным от той проги на гитхабе, которую ты нагло спиздила

        что нужно доработать в интерфейсе боковой панели:

        1.сделать адекватные размеры окна и адекватную возможность их менять(сделать так, чтобы окно могло открыться во все окно вплоть до панели задач)
        2.добавить иконки перед статьями и разделами
        3.сделать ползунок чуть медленнее
        4.грамотно разделить для восприятия статьи и разделы
        5.контекстное меню должно вписываться в общую стилистику интерфейса(так просто красивее)
         
         */
        private string _localFilePath;
        
        public Статьи(string filePath)
        { 
            InitializeComponent();
            _localFilePath = filePath;
            byte[] pathBytes = System.IO.File.ReadAllBytes(filePath);
            Texting.Document = WorkWithRTF.LoadRtf(pathBytes);

            ChangeColor.ItemsSource = typeof(Colors).GetProperties().
                Select(x => x.Name).OrderBy(name => name);

            ChangeSize.ItemsSource = new List<Double>()
            {
                8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
            };

            ChangeFont.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            ChangeSize.SelectedItem = Texting.FontSize;
            ChangeFont.SelectedItem = Texting.FontFamily;
            ChangeColor.SelectedItem = Texting.Foreground;
        }
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(0);
            try
            {
                OpenFileDialog ofd = new();
                ofd.Filter = "RTF документ (*.rtf)|*.rtf";

                if (ofd.ShowDialog() == true)
                {
                    TextRange doc = new TextRange(Texting.Document.ContentStart, 
                        Texting.Document.ContentEnd);
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        if (System.IO.Path.GetExtension(ofd.FileName).ToLower() == ".rtf") 
                            doc.Load(fs, DataFormats.Rtf);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Расширение не поддерживается данным приложением \n " +
                    $"Код ошибки: {ex.Message}", "Не удалось открыть документ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Safe_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(0);
            try
            {
                SaveFileDialog sfd = new();
                sfd.Filter = "RTF документ (*.rtf)|*.rtf";

                TextRange doc = new(Texting.Document.ContentStart, Texting.Document.ContentEnd);
                if (sfd.ShowDialog() == true)
                {
                    using (FileStream fs = File.Create(sfd.FileName))
                    {
                        if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".rtf")
                            doc.Save(fs, DataFormats.Rtf);
                    }

                    int findId = Convert.ToInt32(App.repository.FindDocument(App.doc.LogsId));
                    App.repository.EditDocument(findId, sfd.FileName, System.IO.Path.GetDirectoryName(sfd.FileName));
                }
                
            }
            catch(Exception ex) 
            {
                MessageBox.Show($"Расширение не поддерживается данным приложением \n " +
                    $"Код ошибки: {ex.Message}", "Не удалось сохранить документ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new();

                if (pd.ShowDialog() == true)
                {
                    pd.PrintDocument((((IDocumentPaginatorSource)Texting.Document).DocumentPaginator), "Распечатать документ");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Equals(Height = 200, Width = 300);
                MessageBox.Show($"Формат печати не поддерживается данным приложением \n " +
                    $"Код ошибки: {ex.Message}", "Не удалось распечатать документ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Texting != null && ChangeFont.SelectedItem != null)
            {
                Texting.Selection.ApplyPropertyValue(RichTextBox.FontFamilyProperty, ChangeFont.SelectedItem);
                Texting.Focus();
            }
        }

        private void ChangeSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            double size;

            if (Texting != null && ChangeSize.SelectedItem != null)
            {
                if (double.TryParse(ChangeSize.Text, out size))
                {
                    Texting.Selection.ApplyPropertyValue(Inline.FontSizeProperty, size);
                    Texting.Focus();
                }
            }
        }

        private void Texting_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                object temp = Texting.Selection.GetPropertyValue(Inline.FontWeightProperty);
                bold.IsChecked = ((temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold)));

                temp = Texting.Selection.GetPropertyValue((Inline.FontStyleProperty));
                Itallian.IsChecked = ((temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic)));

                temp = Texting.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                UnderLine.IsChecked = ((temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline)));

                temp = Texting.Selection.GetPropertyValue(Inline.FontFamilyProperty);
                ChangeFont.SelectedItem = temp;

                temp = Texting.Selection.GetPropertyValue(Inline.FontSizeProperty);
                ChangeSize.Text = temp.ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Equals(Height = 200, Width = 300);
                MessageBox.Show(ex.Message);
            }
        }
        private void ChangeColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Texting != null && ChangeColor.SelectedItem != null)
            {
                Texting.Selection.ApplyPropertyValue(Inline.ForegroundProperty, ChangeColor.SelectedItem);
                Texting.Focus();
            }
        }

        private void lock_Click(object sender, RoutedEventArgs e)
        {
            Open.IsEnabled = true; 
            Safe.IsEnabled = true;
            ChangeColor.IsEnabled = true;
            ChangeFont.IsEnabled = true;
            ChangeSize.IsEnabled = true;
        }
    }
}