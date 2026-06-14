using Microsoft.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace Курсовая_работа_1_семестр.для_работы_с_файлами
{
    internal class WorkWithRTF
    {
        public static FlowDocument LoadRtf(byte[] rtfBytes)
        {
            try
            {
                var document = new FlowDocument();
                var range = new TextRange(document.ContentStart, document.ContentEnd);
                
                using (var stream = new MemoryStream(rtfBytes))
                {
                    range.Load(stream, DataFormats.Rtf);
                }

                return document;
            }
            catch (Exception ex)
            {
                return CreateDocument($"Ошибка загрузки: {ex}");
            }
        }
        public static FlowDocument CreateDocument(string message)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph(new Run(message));
            paragraph.FontSize = 14;
            doc.Blocks.Add(paragraph);
            return doc;
        }
    }
}