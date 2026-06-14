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
        public static WorkDbContext db = new();
        public static DocumentsLog doc = new();
        public async Task<List<DocumentsLog>> GetDocumentPaged(int page, int pageSize)
        {
            return await db.DocumentsLogs.OrderBy(d => d.LogsId)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .Select(s => new DocumentsLog()
                     {
                         LogsId = s.LogsId,
                         MainTree = s.MainTree,
                         LogsName = s.LogsName
                     })
                     .ToListAsync();
        }
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
        public async Task<bool> AddDocument(string logsName, string mainTree)
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
            return await db.SaveChangesAsync() > 0;
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
    }
}
