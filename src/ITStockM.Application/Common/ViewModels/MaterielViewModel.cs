

namespace ITStockM.Models.ViewModels
{
    public class MaterielViewModel
    {
        public ITStockM.Domain.Entities.Materiel Materiel { get; set; }
        public List<SerialNumber> SRList { get; set; }
        public Boolean HaveSr { get; set; } = false;

        public int Year { get; set; }
        public int Month { get; set; }


    }

    public class SerialNumber
    {
        public string SR { get; set; }
    }

}