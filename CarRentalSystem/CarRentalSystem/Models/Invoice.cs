using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarRentalSystem.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvId { get; set; }
        public int BookingId { get; set; }

        public DateTime Date { get; set; }
        [DataType(DataType.Date)]
       
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        [DisplayName("Penalty Fee(R)")]
        public double Penalty { get; set; }
        [DisplayName("Late Payment Fee(R)")]
        public double LatePaymentFee { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
    }
}