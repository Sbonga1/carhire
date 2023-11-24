using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CarRentalSystem.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        public string Name  { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public virtual DbSet<Car> Cars { get; set; } 
        public virtual DbSet<Booking> Bookings { get; set; } 
        public virtual DbSet<Invoice> Invoices { get; set; } 
        public virtual DbSet<CarInspection> CarInspections { get; set; } 
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Inspector> Inspectors { get; set; }
        public virtual DbSet<AssignInspector> AssignInspectors { get; set; }
        public virtual DbSet<RentalHandover> RentalHandovers { get; set; }
        public virtual DbSet<Refund> Refunds { get; set; }
        

        public virtual DbSet<CarReturn> CarReturns { get; set; }

    }
}