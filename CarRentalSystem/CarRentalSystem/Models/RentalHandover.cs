using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class RentalHandover
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HandoverID { get; set; }
        public int BookingId { get; set; }
        public int InspId { get; set; }
        public string CustEmail { get; set; }
        public DateTime HandoverDate { get; set; }
        public string HandoverTime { get; set; }
        public string Status { get; set; }
        public string Signature { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

       

        [ForeignKey("InspId")]
        public virtual Inspector Inspector { get; set; }
    }
}