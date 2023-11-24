using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefundId { get; set; }
        public int BookingId { get; set; }
        public string Reason { get; set; }
        public double InitialAmt { get; set; }
        [DisplayName("Reason")]
        public string DeclineReason { get; set; }
        [DisplayName("Requested On")]
        public DateTime RefundDate { get; set; }
        
        [DisplayName("Client Email")]
        public string emailaddress { get; set; }
        [DisplayName("Refund Fee")]
        public double RefundFee { get; set; }
        [DisplayName("Amount To Be Paid")]
        public double tobePaid { get; set; }
        public string Status { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
    }
}