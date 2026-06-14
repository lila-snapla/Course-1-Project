using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Configuration;
using System.Data;
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
        public static DocumentsLog doc = new();
        public static string _filePath = doc.MainTree;
        public static Repository repository = new();
    }

}
