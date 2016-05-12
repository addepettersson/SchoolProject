using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class CostModel
    {
        public DateTime Datepicker { get; set; }
        public int CarId { get; set; }
        public int TypeOfCost { get; set; }
        public decimal Cost { get; set; }
        public string Comment { get; set; }
    }
}