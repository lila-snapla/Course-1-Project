using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.для_работы_с_данными;

namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static WorkDbContext db = new();
        public static DocumentsLog doc { get; set; }
        public static Repository repository { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            repository = new Repository();
        }

        public string GetPath(string path)
        {
            if (path == null) return null;
            return repository.GetDirectory(path);
        }       
    }
}
