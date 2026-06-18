using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;
using Курсовая_работа_1_семестр.для_работы_с_файлами;
using System.Windows;

namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.для_работы_с_данными
{
    public class Repository
    {
        public WorkDbContext db = new();
        private readonly string _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Статьи");
        public async Task<DocumentsLog> FindDocument(int Id)
        {
            return await db.DocumentsLogs.Where(d => d.LogsId == Id)
                .Select(s => new DocumentsLog
                {
                    LogsId = s.LogsId,
                    MainTree = s.MainTree,
                    LogsName = s.LogsName
                }).FirstOrDefaultAsync();
        }
        public async Task<List<DocumentsLog>> GetAllDocuments()
        {
            return await db.DocumentsLogs.ToListAsync();
        }
        public async Task<bool> DeleteDocument(int id)
        {
            var entity = await db.DocumentsLogs.FindAsync(id);
            if (entity == null) return false;
            db.DocumentsLogs.Remove(entity);
            return await db.SaveChangesAsync() > 0;
        }
        public async Task<int> AddDocument(string logsName, string mainTree)
        {
            var entity = new DocumentsLog
            {
                MainTree = mainTree,
                LogsName = logsName,
                CreatedAtDay = DateOnly.FromDateTime(DateTime.Now),
                CreatedAtTime = TimeOnly.FromDateTime(DateTime.Now),
                HasChanged = false
            };

            await db.DocumentsLogs.AddAsync(entity);
            await db.SaveChangesAsync();

            return entity.LogsId; 
        }
        public async Task<bool> EditDocument(int Id, string logsName, string mainTree)
        {
            var doc = await db.DocumentsLogs.FindAsync(Id);
            if (doc == null) return false;

            doc.LogsName = logsName;
            doc.MainTree = mainTree;
            doc.CreatedAtDay = DateOnly.FromDateTime(DateTime.Now);
            doc.CreatedAtTime = TimeOnly.FromDateTime(DateTime.Now);
            doc.HasChanged = true;

            return await db.SaveChangesAsync() > 0;
        }
        public string GetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (Path.IsPathRooted(path))
            {
                return File.Exists(path) ? path : null;
            }

            if (!Directory.Exists(_baseDirectory))
            {
                MessageBox.Show($"Базовая папка не существует: {_baseDirectory}");
                return null;
            }

            string fullPath = Path.Combine(_baseDirectory, path);
            if (File.Exists(fullPath))
                return fullPath;

            string fileName = Path.GetFileName(path);
            try
            {
                var foundFiles = Directory.GetFiles(_baseDirectory, fileName, SearchOption.AllDirectories);
                if (foundFiles.Length > 0)
                    return foundFiles[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }

            MessageBox.Show($"Файл не найден:\nИскали: {path}\nВ папке: {_baseDirectory}");
            return null;
        }
    }
}
