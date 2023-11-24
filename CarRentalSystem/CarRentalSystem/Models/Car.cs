using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CarId { get; set; }
        [DisplayName("Car Name")]
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Model { get; set; }
        public double Mileage { get; set; }
        [DataType(DataType.Date)]
        public DateTime Year { get; set; }
        [DisplayName("Rental Rate(1hr)")]
        public double Price { get; set; }
        [DisplayName("Rental Rate(1km)")]
        public double DistPrice { get; set; }
        public double FuelAmt { get; set; }
       public string Status { get; set; }
    }
}