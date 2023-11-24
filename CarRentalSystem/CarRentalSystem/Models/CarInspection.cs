using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class CarInspection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DisplayName("Are there any dents?")]
        public string Dents { get; set; }
        [DisplayName("Are the tires in a good condition?")]
        public string TireCondition { get; set; }
        [DisplayName("Is the Exterior in a good condition?")]
        public string ExteriorCondition { get; set; }
        [DisplayName("Is the Interior in a good condition?")]
        public string InteriorCondition { get; set; }
        [DisplayName("Test Drive")]
        public string TestDrive { get; set; }
        public double CarMileage { get; set; }
        public double FuelAmt { get; set; }
        public  string InspEmail { get; set; }
        public  double kmtravelled { get; set; }
        public double timeTravelled { get; set; }
        public  double extraKm { get; set; }
        public double Extratime { get; set; }
        public double ShortFuel { get; set; }

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

    }
}