using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;

public partial class WorkDbContext : DbContext
{
    public WorkDbContext()
    {
    }

    public WorkDbContext(DbContextOptions<WorkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DocumentsLog> DocumentsLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-PRRKD10\\SQLEXPRESS;Database=ProjectDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentsLog>(entity =>
        {
            entity.HasKey(e => e.LogsId).HasName("PK__Document__C920548E3A2A01E0");

            entity.Property(e => e.LogsName).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
