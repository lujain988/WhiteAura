using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class HomeController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();
        public ActionResult Index()
        {
            bool testimonialExists = false;

            if (Session["UserID"] != null)
            {
                var userId = (int)Session["UserID"];

                // Check if the user has submitted a testimonial
                testimonialExists = db.Testimonials.Any(t => t.UserId == userId && t.TestimonialSubmitted == true);

                // Show the toast if the user has not submitted a testimonial and has past bookings
                var pastBookings = db.Bookings.Where(b => b.UserID == userId && b.BookingDate < DateTime.Now).Any();
                if (pastBookings && !testimonialExists)
                {
                    ViewBag.ShowToastNotification = true; // Set to true to show the toast
                }
            }

            ViewBag.TestimonialExists = testimonialExists;

            // Fetch only approved testimonials (change 'Approved' to match your exact status string)
            var approvedTestimonials = db.Testimonials
                                          .Where(t => t.Status == "Approved")
                                          .Take(4) // Limit to 4 testimonials
                                          .ToList();
            ViewBag.ApprovedTestimonials = approvedTestimonials;

            return View();
        }







        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Contact(ContactU model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedAt = DateTime.Now;
                    db.ContactUs.Add(model);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }
            }

            return Json(new { success = false });
        }

        public ActionResult Policy()
        {


            return View();

        }

        public ActionResult Eco()
        {

            return View();

        }
        public ActionResult cultural()
        {

            return View();

        }

    }

}