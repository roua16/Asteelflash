namespace ITStockM.Models.ViewModels
{
    public class MaterielUsageViewModel
    {
        public int Id { get; set; }
        public string MaterielName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        /// <summary>
        /// Fraction between 0 and 1 (e.g. 0.42 => 42%)
        /// </summary>
        public double UsageShare { get; set; }
    }
}