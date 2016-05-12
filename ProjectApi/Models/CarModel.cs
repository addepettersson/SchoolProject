using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
    public class CarModel
    {
        public int Id { get; set; }
        public string Regnr { get; set; }
        public int OriginalMileage { get; set; }
        public int? Colour_Id { get; set; }
        public int? Year_Id { get; set; }
        public int FuelType_Id { get; set; }
        public string Description { get; set; }
        public int CarType_Id { get; set; }
    }
}