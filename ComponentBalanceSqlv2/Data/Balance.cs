using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись таблицы [Балансы].
    /// </summary>
    public class Balance : BaseNotificationClass
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Цеха], 
        /// указывающий какого цеха этот баланс.
        /// </summary>
        public long WorkGuildId { get; set; }
        public virtual WorkGuild WorkGuild { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Детали], 
        /// указывающий какой детали этот баланс.
        /// </summary>
        public long DetailId { get; set; }
        public virtual Detail Detail { get; set; }

        private decimal _countStart = -1M;
        /// <summary>
        /// Кол-во деталей на балансе на начало месяца.
        /// </summary>
        public decimal CountStart
        {
            get { return _countStart; }
            set
            {
                // Костыли в студию
                var flag = _countStart;

                _countStart = value;
                OnPropertyChanged();

                if (flag != -1M)
                {
                    CountEnd = GetRecalculateCountEnd();
                }
            }
        }

        private decimal _costStart = -1M;
        /// <summary>
        /// Цена одной детали на балансе на начало месяца.
        /// </summary>
        public decimal CostStart
        {
            get { return _costStart; }
            set
            {
                // Костыли в студию
                var flag = _costStart;

                _costStart = value;
                OnPropertyChanged();

                if (flag != -1M)
                {
                    SetNewCostEnd();
                    SetNewTransferCost();
                }
            }
        }
        /// <summary>
        /// Конечная стоймость всех деталей на балансе на начало месяца (до 2ух знаков).
        /// </summary>
        public decimal SumStart => decimal.Round(CountStart * CostStart, 2);

        /// <summary>
        /// Кол-во деталей потраченное на выпуск.
        /// </summary>
        public decimal Expenditure => ReleaseMoves.Sum(releaseMove => releaseMove.Count) + SparePartMoves.Sum(sparePartMove => sparePartMove.Count);

        private decimal _countEnd;
        /// <summary>
        /// Конечное кол-во деталей на балансе. 
        /// (Кол-во с учетом всех проведенных движений) 
        /// </summary>
        public decimal CountEnd
        {
            get { return _countEnd; }
            set
            {
                _countEnd = value;
                OnPropertyChanged();
            }
        }

        private decimal _costEnd;
        /// <summary>
        /// Конечная цена одной детали на балансе. 
        /// (Цена с учетом всех проведенных движений) 
        /// </summary>
        public decimal CostEnd
        {
            get { return _costEnd; }
            set
            {
                _costEnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Конечная стоймость всех деталей на балансе (до 2ух знаков). 
        /// (Конечная стоймость всех деталей с учетом всех проведенных движений) 
        /// </summary>
        public decimal SumEnd => decimal.Round(CountEnd * CostEnd, 2);

        /// <summary>
        /// Месяц баланса.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Год баланса.
        /// </summary>
        public int Year { get; set; }

        [ForeignKey("BalanceId")]
        public virtual ICollection<DefectMove> DefectMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<EquipmentMove> EquipmentMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ExportReworkMove> ExportReworkMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ExportWarehouseMove> ExportWarehouseMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ExportWorkGuildMove> ExportWorkGuildMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ImportReworkMove> ImportReworkMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ImportWarehouseMove> ImportWarehouseMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ImportWorkGuildMove> ImportWorkGuildMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<ReleaseMove> ReleaseMoves { get; set; }
        [ForeignKey("BalanceId")]
        public virtual ICollection<SparePartMove> SparePartMoves { get; set; }

        [ForeignKey("TransferBalanceId")]
        public virtual ICollection<ExportWorkGuildMove> TransferExportWorkGuildMoves { get; set; }
        [ForeignKey("TransferBalanceId")]
        public virtual ICollection<ImportWorkGuildMove> TransferImportWorkGuildMoves { get; set; }

        public virtual ICollection<ReleaseReplacementDetail> ReleaseReplacementDetails { get; set; }

        /// <summary>
        /// Все движения баланса (детали).
        /// </summary>
        public IEnumerable<Move> Moves
        {
            get
            {
                var moves = new List<Move>();

                moves.AddRange(DefectMoves);
                moves.AddRange(EquipmentMoves);
                moves.AddRange(ExportReworkMoves);
                moves.AddRange(ExportWarehouseMoves);
                moves.AddRange(ExportWorkGuildMoves);
                moves.AddRange(ImportReworkMoves);
                moves.AddRange(ImportWarehouseMoves);
                moves.AddRange(ImportWorkGuildMoves);
                moves.AddRange(ReleaseMoves);

                moves.AddRange(SparePartMoves);

                return moves;
            }
        }

        /// <summary>
        /// Информация за какой месяц год этот баланс.
        /// </summary>
        public string InfoCalculate => "за " + Constants.MonthsFullNames[Month] + " " + Year + " г.";

        #region Подсчеты для отчета
        /// <summary>
        /// Получить кол-во деталей пришедших со склада на указанное изделие.
        /// <param name="codeProduct">Изделие на которое был приход со склада.</param>
        /// </summary>
        public decimal GetCountImportWarehouse(long codeProduct)
        {
            return ImportWarehouseMoves.Where(move => move.Product.Code == codeProduct).Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали пришедшей со склада на указанное изделие.
        /// </summary>
        /// <param name="codeProduct">Изделие на которое был приход со склада.</param>
        public decimal GetAverageCostImportWarehouse(long codeProduct)
        {
            var sum = ImportWarehouseMoves.Where(move => move.Product.Code == codeProduct)
                .Sum(move => move.Count * move.Cost);
            var count = GetCountImportWarehouse(codeProduct);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Получить кол-во деталей потраченное на выпуск указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetCountRelease(long codeProduct)
        {
            return ReleaseMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct).Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали потраченной на выпуск указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetAverageCostRelease(long codeProduct)
        {
            var sum = ReleaseMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct)
                .Sum(move => move.Count * move.Cost);
            var count = GetCountRelease(codeProduct);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Получить кол-во деталей потраченное на докомплектацию выпусков указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetCountEquipment(long codeProduct)
        {
            return EquipmentMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct).Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали докомплектации выпуска указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetAverageCostEquipment(long codeProduct)
        {
            var sum = EquipmentMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct)
                .Sum(move => move.Count * move.Cost);
            var count = GetCountEquipment(codeProduct);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Получить кол-во деталей пришедших при доработки выпуска указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetCountImportRework(long codeProduct)
        {
            return ImportReworkMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct).Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали пришедшей при доработки выпуска указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetAverageCostImportRework(long codeProduct)
        {
            var sum = ImportReworkMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct)
                .Sum(move => move.Count * move.Cost);
            var count = GetCountImportRework(codeProduct);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Получить кол-во деталей потраченных при доработки выпуска указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetCountExportRework(long codeProduct)
        {
            return ExportReworkMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct).Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали потраченной при доработки выпуска указанного изделия.
        /// <param name="codeProduct">Код выпущенного изделия.</param>
        /// </summary>
        public decimal GetAverageCostExportRework(long codeProduct)
        {
            var sum = ExportReworkMoves.Where(move => move.ReleaseProduct.Product.Code == codeProduct)
                .Sum(move => move.Count * move.Cost);
            var count = GetCountExportRework(codeProduct);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Получить кол-во деталей в указанном движении.
        /// <param name="moves">Движения.</param>
        /// </summary>
        public decimal GetCountSelectedMoves(IEnumerable<Move> moves)
        {
            return moves.Sum(move => move.Count);
        }

        /// <summary>
        /// Получить среднюю цену одной детали в указанном движении.
        /// <param name="moves">Движения.</param>
        /// </summary>
        public decimal GetAverageCostSelectedMoves(IEnumerable<Move> moves)
        {
            var m = moves as Move[] ?? moves.ToArray();
            var sum = m.Sum(move => (move.Count * move.Cost));
            var count = GetCountSelectedMoves(m);

            return count != 0M ? decimal.Round(sum / count, 2) : 0M;
        }

        /// <summary>
        /// Получить все номера актов докомплектации этой деталью.
        /// </summary>
        public string GetNumbersEquipment()
        {
            return EquipmentMoves.Aggregate(string.Empty, (current, move) => current + move.Number + "; ");
        }

        /// <summary>
        /// Получить все номера актов доработки с которых пришла деталь.
        /// </summary>
        public string GetNumbersImportRework()
        {
            return ImportReworkMoves.Aggregate(string.Empty, (current, move) => current + move.Number + "; ");
        }

        /// <summary>
        /// Получить все номера актов доработки на которые ушла деталь.
        /// </summary>
        public string GetNumbersExportRework()
        {
            return ExportReworkMoves.Aggregate(string.Empty, (current, move) => current + move.Number + "; ");
        }

        /// <summary>
        /// Получить все номера актов браков этой деталь.
        /// </summary>
        public string GetNumbersDefect()
        {
            return DefectMoves.Aggregate(string.Empty, (current, move) => current + move.Number + "; ");
        }

        /// <summary>
        /// Получить все номера складов на которые ушла деталь.
        /// </summary>
        public string GetNumberExportWarehouse()
        {
            return ExportWarehouseMoves.Aggregate(string.Empty, (current, move) => current + (move.Number + "; "));
        }

        /// <summary>
        /// Получить все номера цехов в которые ушла деталь.
        /// </summary>
        public string GetNumbersExportWorkGuild()
        {
            return ExportWorkGuildMoves.Aggregate(string.Empty,
                (current, move) => current + (move.TransferBalance.WorkGuild.WorkGuildNumber + "; "));
        }

        /// <summary>
        /// Получить все номера цехов с которых пришла деталь.
        /// </summary>
        public string GetNumbersImportWorkGuild()
        {
            return ImportWorkGuildMoves.Aggregate(string.Empty,
                (current, move) => current + (move.TransferBalance.WorkGuild.WorkGuildNumber + "; "));
        }

        /// <summary>
        /// Получить все номера актов продаж запчестей.
        /// </summary>
        public string GetNumberSparePart()
        {
            return SparePartMoves.Aggregate(string.Empty, (current, move) => current + (move.Number + "; "));
        }
        #endregion
        #region Add/Delete Move
        /// <summary>
        /// Добавляет движение [Брак] детали.
        /// </summary>
        /// <param name="count">Кол-во бракованных деталей.</param>
        /// <param name="cost">Цена одной бракованной детали.</param>
        /// <param name="number">Номер акта брака.</param>
        public void AddDefectMove(decimal count, decimal cost, string number)
        {
            DefectMoves.Add(new DefectMove(Id, count, cost, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [Докомплектация] выпущенного изделия деталью.
        /// </summary>
        /// <param name="count">Кол-во деталей докомплектации.</param>
        /// <param name="cost">Цена одной докомплектованной детали.</param>
        /// <param name="releaseProductId">Вторичный ключ на таблицу [Выпущенные изделия], указывающий какое выпущенное изделие докомплектуется.</param>
        /// <param name="number">Номер акта докомплектации.</param>
        public void AddEquipmentMove(decimal count, decimal cost, long releaseProductId, string number)
        {
            EquipmentMoves.Add(new EquipmentMove(Id, count, cost, releaseProductId, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [Расход доработка] деталью.
        /// </summary>
        /// <param name="count">Кол-во деталей потраченных на доработку.</param>
        /// <param name="releaseProductId">Вторичный ключ на таблицу [Выпущенные изделия], указывающий какое выпущенное изделие дорабатывается.</param>
        /// <param name="number">Номер акта доработки.</param>
        public void AddExportReworkMove(decimal count, long releaseProductId, string number)
        {
            ExportReworkMoves.Add(new ExportReworkMove(Id, count, CostEnd, releaseProductId, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [На склад] деталь.
        /// </summary>
        /// <param name="count">Кол-во деталей ушедших на склад.</param>
        /// <param name="number">Номер склада.</param>
        public void AddExportWarehouseMove(decimal count, string number)
        {
            ExportWarehouseMoves.Add(new ExportWarehouseMove(Id, count, CostEnd, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [В другой цех] деталь.
        /// </summary>
        /// <param name="count">Кол-во деталей переданых в другой цех.</param>
        /// <param name="cost">Цена одной переданной в другой цех детали.</param>
        /// <param name="transferBalanceId">Вторичный ключ на таблицу [Балансы], указывающий на какой баланс(цех) передана деталь.</param>
        public void AddExportWorkGuildMove(decimal count, decimal cost, long transferBalanceId)
        {
            ExportWorkGuildMoves.Add(new ExportWorkGuildMove(Id, count, cost, transferBalanceId, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [Приход доработка] деталью.
        /// </summary>
        /// <param name="count">Кол-во деталей пришедших при доработке.</param>
        /// <param name="cost">Цена одной пришедшей при доработке детали.</param>
        /// <param name="releaseProductId">Вторичный ключ на таблицу [Выпущенные изделия], указывающий какое выпущенное изделие дорабатывается.</param>
        /// <param name="number">Номер акта доработки.</param>
        public void AddImportReworkMove(decimal count, decimal cost, long releaseProductId, string number)
        {
            ImportReworkMoves.Add(new ImportReworkMove(Id, count, cost, releaseProductId, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
            SetNewCostEnd();
        }

        /// <summary>
        /// Добавляет движение [Со склада] деталь.
        /// </summary>
        /// <param name="count">Кол-во деталей пришедших со склада.</param>
        /// <param name="cost">Цена одной пришедшей со склада детали.</param>
        /// <param name="productId">Вторичный ключ на таблицу [Изделия], указывающий для какого изделия пришла деталь.</param>
        public void AddImportWarehouseMove(decimal count, decimal cost, long productId)
        {
            ImportWarehouseMoves.Add(new ImportWarehouseMove(Id, count, cost, productId, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
            SetNewCostEnd();
        }

        /// <summary>
        /// Добавляет движение [Из цеха] деталь.
        /// </summary>
        /// <param name="count">Кол-во деталей пришедших из другого цеха.</param>
        /// <param name="cost">Цена одной пришедшей из другого цеха детали.</param>
        /// <param name="transferBalanceId">Вторичный ключ на таблицу [Балансы], указывающий с какого баланс(цеха) пришла деталь.</param>
        public void AddImportWorkGuildMove(decimal count, decimal cost, long transferBalanceId)
        {
            ImportWorkGuildMoves.Add(new ImportWorkGuildMove(Id, count, cost, transferBalanceId, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
            SetNewCostEnd();
        }

        /// <summary>
        /// Добавляет движение [Расход] детали.
        /// </summary>
        /// <param name="count">Кол-во деталей потраченных на выпуск изделия.</param>
        /// <param name="releaseProductId">Вторичный ключ на таблицу [Выпущенные изделия], указывающий на какое выпущенное изделие потрачена деталь.</param>
        public void AddReleaseMove(decimal count, long releaseProductId)
        {
            ReleaseMoves.Add(new ReleaseMove(Id, count, CostEnd, releaseProductId, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Добавляет движение [Деталь] детали.
        /// </summary>
        /// <param name="count">Кол-во деталей.</param>
        /// <param name="number">Номер акта продажи детали.</param>
        public void AddSparePartMove(decimal count, string number)
        {
            SparePartMoves.Add(new SparePartMove(Id, count, CostEnd, number, new MonthYear(Month, Year)));
            CountEnd = GetRecalculateCountEnd();
        }

        /// <summary>
        /// Удаляет указанное движение.
        /// </summary>
        public void DeleteMove(DomainContext context, Move move)
        {
            context.Moves.Remove(context.Moves.First(i => i.Id == move.Id));

            CountEnd = GetRecalculateCountEnd();

            var baseType = move.GetType().GetTypeInfo().BaseType;
            if (baseType == null) { return; }

            switch (baseType.Name)
            {
                case "ExportWorkGuildMove":
                    var transferBalanceId = ((ExportWorkGuildMove)move).TransferBalanceId;
                    var transferBalance = context.Balances
                        .Include(i => i.ImportWorkGuildMoves)
                        .First(i => i.Id == transferBalanceId);
                    var importWorkGuildMove =
                        transferBalance.ImportWorkGuildMoves.First(i => i.TransferBalanceId == Id);
                    transferBalance.DeleteMove(context, importWorkGuildMove);
                    break;
                case "ImportWarehouseMove":
                case "ImportWorkGuildMove":
                    SetNewCostEnd();
                    break;
            }
        }
        #endregion
        #region Recalculate Count/Cost End
        /// <summary>
        /// Получить пересчитанное с учетом всех движений конечное кол-во деталей на балансе.
        /// </summary>
        public decimal GetRecalculateCountEnd()
        {
            return CountStart + Moves.Sum(i => i.IsSupply ? i.Count : -i.Count);
        }

        /// <summary>
        /// Установить новую цену одной детали в движениях зависящих от конечной цены детали.
        /// </summary>
        private void SetNewCostEnd()
        {
            var newCostEnd = GetRecalculateCostEnd();
            if (CostEnd != newCostEnd)
            {
                foreach (var releaseMove in ReleaseMoves)
                {
                    releaseMove.Cost = newCostEnd;
                }

                foreach (var exportWarehouseMove in ExportWarehouseMoves)
                {
                    exportWarehouseMove.Cost = newCostEnd;
                }

                foreach (var sparePartMove in SparePartMoves)
                {
                    sparePartMove.Cost = newCostEnd;
                }
            }

            CostEnd = newCostEnd;
        }

        /// <summary>
        /// Получить пересчитанную с учетом всех движений конечную цену одной детали на балансе.
        /// </summary>
        private decimal GetRecalculateCostEnd()
        {
            if (CountEnd == 0M)
            {
                return 0M;
            }

            var moves = Moves.ToList();
            var countSupplyMove = moves.Sum(i => i.IsSupply ? i.Count : 0);
            var sumSupplyMove = moves.Sum(i => i.IsSupply ? i.Sum : 0);

            return decimal.Round((SumStart + sumSupplyMove) / (CountStart + countSupplyMove), 2);
        }

        /// <summary>
        /// Установить новую цену переброски одной детали у всех перебросок этой детали.
        /// </summary>
        private void SetNewTransferCost()
        {
            if (ExportWorkGuildMoves.Count <= 0) { return; }

            var newTransferCost = GetTransferCost();
            if (ExportWorkGuildMoves.First().Cost == newTransferCost) { return; }

            foreach (var exportWorkGuildMove in ExportWorkGuildMoves)
            {
                exportWorkGuildMove.Cost = newTransferCost;
                using (var context = new DomainContext())
                {
                    var transferBalance = context.Balances
                        .Include(i => i.ImportWorkGuildMoves)
                        .First(i => i.Id == exportWorkGuildMove.TransferBalanceId);
                    var importWorkGuildMove = transferBalance.ImportWorkGuildMoves.First(i => i.TransferBalanceId == Id);
                    importWorkGuildMove.Cost = newTransferCost;
                    transferBalance.SetNewCostEnd();
                    transferBalance.SetNewTransferCost();
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Получить цену одной детали для переброски.
        /// </summary>
        public decimal GetTransferCost()
        {
            if (CostStart != 0M)
            {
                return CostStart;
            }

            var count = 0M;
            var cost = 0M;
            foreach (var importWarehouseMove in ImportWarehouseMoves)
            {
                count += importWarehouseMove.Count;
                cost += importWarehouseMove.Cost;
            }

            if (count != 0)
            {
                return decimal.Round(cost / count, 2);
            }

            return CostEnd;
        }
        #endregion

        public Balance()
        {
            DefectMoves = new HashSet<DefectMove>();
            EquipmentMoves = new HashSet<EquipmentMove>();
            ExportReworkMoves = new HashSet<ExportReworkMove>();
            ExportWarehouseMoves = new HashSet<ExportWarehouseMove>();
            ExportWorkGuildMoves = new HashSet<ExportWorkGuildMove>();
            ImportReworkMoves = new HashSet<ImportReworkMove>();
            ImportWarehouseMoves = new HashSet<ImportWarehouseMove>();
            ImportWorkGuildMoves = new HashSet<ImportWorkGuildMove>();
            ReleaseMoves = new HashSet<ReleaseMove>();

            SparePartMoves = new HashSet<SparePartMove>();

            TransferExportWorkGuildMoves = new HashSet<ExportWorkGuildMove>();
            TransferImportWorkGuildMoves = new HashSet<ImportWorkGuildMove>();

            ReleaseReplacementDetails = new HashSet<ReleaseReplacementDetail>();
        }
        public Balance(long workGuildId, long detailId, MonthYear monthYear) : this()
        {
            WorkGuildId = workGuildId;
            DetailId = detailId;
            CountStart = 0M;
            CostStart = 0M;
            CountEnd = 0M;
            CostEnd = 0M;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }

        public Balance(long workGuildId, long detailId, 
            decimal countStart, decimal costStart, 
            decimal countEnd, decimal costEnd,
            MonthYear monthYear) : this()
        {
            WorkGuildId = workGuildId;
            DetailId = detailId;
            CountStart = countStart;
            CostStart = costStart;
            CountEnd = countEnd;
            CostEnd = costEnd;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
