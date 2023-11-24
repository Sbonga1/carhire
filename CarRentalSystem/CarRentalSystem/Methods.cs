using CarRentalSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CarRentalSystem
{
    public class Methods
    {
        public decimal GetHourlyRate(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                string CarId = "";
               // Car rate = db.Cars.Find(id);
               // if (rate != null)
               // {
                    //return rate.Rate;
               // }
                // Handle the case where the rate is not found in the database
                return 0; // Provide a default rate or handle the error as needed
            }
        }
    }
}