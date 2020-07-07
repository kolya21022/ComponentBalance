using System.Data.Entity;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Data
{
    public class DomainContext : DbContext
    {
        public DomainContext() : base("DbConnectionString")
        {

        }

        #region ----- Справочные -----
        /// <summary>
        /// Таблица [Продукты] содержит информацию о всех изделиях завода.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Таблица [Детали] содержит информацию о всех деталях завода (используемых в программе).
        /// </summary>
        public DbSet<Detail> Details { get; set; }

        /// <summary>
        /// Таблица [Еденицы измерения] содержит информацию о всех еденицах измерения.
        /// </summary>
        public DbSet<Measure> Measures { get; set; }

        /// <summary>
        /// Таблица [Цеха] содержит информацию о всех цехах завода.
        /// </summary>
        public DbSet<WorkGuild> WorkGuilds { get; set; }

        /// <summary>
        /// Таблица [Типы ТМЦ] содержит справочную информация всех типов ТМЦ.
        /// </summary>
        public DbSet<TmcType> TmcTypes { get; set; }
        #endregion

        /// <summary>
        /// Таблица [Балансы] содержит информацию о балансе деталей в цеху.
        /// </summary>
        public DbSet<Balance> Balances { get; set; }

        /// <summary>
        /// Таблица [Закрытые месяца] содержит информацию закрыл ли цех рабочий месяц.
        /// </summary>
        public DbSet<MonthClose> MonthCloses { get; set; }

        /// <summary>
        /// Таблица [Месяц и Год] содержит различные пары Месяц и Год и расшифровку этой пары.
        /// </summary>
        public DbSet<MonthYear> MonthYears { get; set; }

        
        /// <summary>
        /// Таблица [Состав изделий] содержит информацию из каких деталей состоит изделие по документации.
        /// </summary>
        public DbSet<CompositionProduct> CompositionProducts { get; set; }

        /// <summary>
        /// Таблица [Взаимозаменяймые детали] содержит информацию о разрешенных заменах деталей.
        /// </summary>
        public DbSet<ReplacementDetail> ReplacementDetails { get; set; }

        /// <summary>
        /// Таблица [Выпущенные изделия] содержит информацию о выпущенных изделиях цехом.
        /// </summary>
        public DbSet<ReleaseProduct> ReleaseProducts { get; set; }

        /// <summary>
        /// Таблица [Замены деталей в выпусках] содержит информацию о том какие были сделаны замены при выпуске изделия.
        /// </summary>
        public DbSet<ReleaseReplacementDetail> ReleaseReplacementDetails { get; set; }

        #region ----- Движение -----
        /// <summary>
        /// Таблица [Типы движений] содержит справочную информация всех типов движения.
        /// </summary>
        public DbSet<MoveType> MoveTypes { get; set; }

        /// <summary>
        /// Таблица [Движения] содержит информацию [Всех передвижений] деталей цеха.
        /// </summary>
        public DbSet<Move> Moves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Брак] цеха.
        /// </summary>
        public DbSet<DefectMove> DefectMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Докомплектация] цеха.
        /// </summary>
        public DbSet<EquipmentMove> EquipmentMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Расход на доработку] цеха.
        /// </summary>
        public DbSet<ExportReworkMove> ExportReworkMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Передача на склад] цеха.
        /// </summary>
        public DbSet<ExportWarehouseMove> ExportWarehouseMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Передача в другой цех] цеха.
        /// </summary>
        public DbSet<ExportWorkGuildMove> ExportWorkGuildMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Приход на доработку] цеха.
        /// </summary>
        public DbSet<ImportReworkMove> ImportReworkMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Приход со склада] цеха.
        /// </summary>
        public DbSet<ImportWarehouseMove> ImportWarehouseMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Приход с другого цеха] цеха.
        /// </summary>
        public DbSet<ImportWorkGuildMove> ImportWorkGuildMoves { get; set; }

        /// <summary>
        /// ПодТаблица [Движения] содержит информацию [Расход на выпуск] цеха.
        /// </summary>
        public DbSet<ReleaseMove> ReleaseMoves { get; set; }
        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>().Property(x => x.CountStart).HasPrecision(9, 3);
            modelBuilder.Entity<Balance>().Property(x => x.CountEnd).HasPrecision(9, 3);
            modelBuilder.Entity<Move>().Property(x => x.Count).HasPrecision(9, 3);
            modelBuilder.Entity<CompositionProduct>().Property(x => x.Count).HasPrecision(9, 3);
            modelBuilder.Entity<ReleaseReplacementDetail>().Property(x => x.Count).HasPrecision(9, 3);
        }
    }
}
