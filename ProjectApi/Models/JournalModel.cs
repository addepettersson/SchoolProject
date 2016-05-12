using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class JournalModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal FuelAmount { get; set; }

        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
        public int DriverId { get; set; }
        public int CarId { get; set; }
        public int FuelTypeId { get; set; }
        public int MileAge { get; set; }
        public string Regnr { get; set; }
        public string FuelType { get; set; }
        public int OriginalMileage { get; set; }
        public int CarTypeId { get; set; }
    }
}