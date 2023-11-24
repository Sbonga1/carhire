using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class AssignInspectorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AssignInspectors
        public ActionResult Index()
        {
            var assignInspectors = db.AssignInspectors.Include(a => a.Booking).Include(a => a.Inspector);
            return View(assignInspectors.ToList());
        }
        public ActionResult MyAssignments()
        {
            var assignInspectors = db.AssignInspectors.Include(a => a.Booking).Include(a => a.Inspector);
            return View(assignInspectors.ToList());
        }


        // GET: AssignInspectors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignInspector assignInspector = db.AssignInspectors.Find(id);
            if (assignInspector == null)
            {
                return HttpNotFound();
            }
            return View(assignInspector);
        }

        // GET: AssignInspectors/Create
        public ActionResult Create(int inspId)
        {
           
            
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name");
            ViewBag.AssInspId = new SelectList(db.Inspectors, "InspId", "Name");
            return View();
        }

        // POST: AssignInspectors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssInspId,BookingId,InspId,Name,Surname,Email")] AssignInspector assignInspector)
        {
            if (ModelState.IsValid)
            {
                db.AssignInspectors.Add(assignInspector);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", assignInspector.BookingId);
            ViewBag.AssInspId = new SelectList(db.Inspectors, "InspId", "Name", assignInspector.AssInspId);
            return View(assignInspector);
        }

        // GET: AssignInspectors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignInspector assignInspector = db.AssignInspectors.Find(id);
            if (assignInspector == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", assignInspector.BookingId);
            ViewBag.AssInspId = new SelectList(db.Inspectors, "InspId", "Name", assignInspector.AssInspId);
            return View(assignInspector);
        }

        // POST: AssignInspectors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssInspId,BookingId,InspId,Name,Surname,Email")] AssignInspector assignInspector)
        {
            if (ModelState.IsValid)
            {
                db.Entry(assignInspector).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", assignInspector.BookingId);
            ViewBag.AssInspId = new SelectList(db.Inspectors, "InspId", "Name", assignInspector.AssInspId);
            return View(assignInspector);
        }

        // GET: AssignInspectors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignInspector assignInspector = db.AssignInspectors.Find(id);
            if (assignInspector == null)
            {
                return HttpNotFound();
            }
            return View(assignInspector);
        }

        // POST: AssignInspectors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AssignInspector assignInspector = db.AssignInspectors.Find(id);
            db.AssignInspectors.Remove(assignInspector);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
