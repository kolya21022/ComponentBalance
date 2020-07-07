using ComponentBalanceSqlv2.Data;

namespace ComponentBalanceSqlv2.Model
{
    /// <summary>
    /// Запись отчета [Не хватающие детали].
    /// </summary>
    public class NotEnoughDetailReport
    {
        public long ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductDesignation { get; set; }
        public int ProductCount { get; set; }
        public long DetailCode { get; set; }
        public string DetailName { get; set; }
        public string DetailDesignation { get; set; }
        public string DetailGost { get; set; }
        public decimal DetailCount { get; set; }
        public string MeasureShortName { get; set; }
        public decimal DetailBalanceCount { get; set; }
        public string Type { get; set; }

        public NotEnoughDetailReport(string type, Product product, int productCount, 
            Detail detail, decimal detailCount, decimal detailBalanceCount)
        {
            ProductCode = product.Code;
            ProductName = product.Name;
            ProductDesignation = product.Designation;
            ProductCount = productCount;
            DetailCode = detail.Code;
            DetailName = detail.Name;
            DetailDesignation = detail.Designation;
            DetailGost = detail.Gost;
            DetailCount = detailCount;
            MeasureShortName = detail.Measure.ShortName;
            DetailBalanceCount = detailBalanceCount;
            Type = type;
        }

        public NotEnoughDetailReport(string type, Product product, int productCount, 
            ReplacementDetail replacementDetail, string measureShortName, decimal detailBalanceCount)
        {
            ProductCode = product.Code;
            ProductName = product.Name;
            ProductDesignation = product.Designation;
            ProductCount = productCount;
            DetailCode = replacementDetail.DetailEndCode;
            DetailName = replacementDetail.DetailEndName;
            DetailDesignation = replacementDetail.DetailEndDesignation;
            DetailGost = replacementDetail.DetailEndGost;
            DetailCount = replacementDetail.Count;
            MeasureShortName = measureShortName;
            DetailBalanceCount = detailBalanceCount;
            Type = type;
        }
    }
}
