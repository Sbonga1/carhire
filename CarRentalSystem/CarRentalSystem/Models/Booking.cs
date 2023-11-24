using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }
        public int CarId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        [StringLength(13, MinimumLength = 13, ErrorMessage = "IdNumber must be exactly 13 characters.")]
        public string IdNumber { get; set; }
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }
        [DataType(DataType.Time)]
        public DateTime PickupTime { get; set; }
        [DataType(DataType.Time)]
        public DateTime ReturnTime { get; set; }
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        [DisplayName("Cost(Time)")]
        public double Cost { get; set; }
        [DisplayName("Cost(Distance)")]
        public double DistCost { get; set; }
        [DisplayName("Total Cost")]
        public double FinalCost { get; set; }

        public string LicenseFile { get; set; }
        public byte[] LicenseContent { get; set; } 
        public string IdFile { get; set; }
        public byte[] IdContent { get; set; } 
        public string BankStatName { get; set; }
        public byte[] BankStatContent { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }

    }
}