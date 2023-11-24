using CarRentalSystem.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarRentalSystem.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Create()
        {
            
            return View();
        }
        [HttpPost]
        public ActionResult Create(User users)
        {
            string role = users.Role.ToString();
            string email = users.Email;
            string name = users.Name;
            string password = users.Password;

            ApplicationDbContext db = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            try
            {
                //Admin
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
                var user = new ApplicationUser();
                user.Name = name;
                user.UserName = email;
                user.Email = email;
                user.EmailConfirmed = true;
                string pwd = password;

                var newuser = userManager.Create(user, pwd);
                if (newuser.Succeeded)
                {
                    userManager.AddToRole(user.Id, role);
                }
                TempData["UserCreationSuccess"] = "User created successfully!!!";
            }
            catch
            {
                TempData["UserCreationError"] = "Something went wrong, Please try again later.";
            }


            return View();
        }
    }
}