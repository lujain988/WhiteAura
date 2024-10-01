using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class PaymeantController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();


        [HttpGet]
        public ActionResult Payment(int bookingId)
        {
            var booking = db.Bookings
                            .Include(b => b.User) // Include related entities if needed
                            .Include(b => b.Service) // Include related service information if needed
                            .FirstOrDefault(b => b.ID == bookingId);

            if (booking == null)
            {
                return HttpNotFound(); // Return 404 if the booking is not found
            }

            return View(booking); // Return the single booking to the view
        }


        [HttpPost]
        public ActionResult Success(int bookingId, string paymentMethod, string orderID)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Update booking status to "Confirmed" and mark it as paid
            booking.Status = "Confirmed";
            booking.IsPaid = true;

            // Calculate 10% of the booking price for the payment
            decimal paymentAmount = (booking.Service != null ? booking.Service.Price ?? 0 : 0) * 0.10M;

            // Save the payment record with the selected method and amount
            var payment = new Payment
            {
                BookingID = bookingId,
                Amount = paymentAmount,  // Only 10% of the service price
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Pending" // For admin approval
            };

            try
            {
                db.Payments.Add(payment);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return Json(new { success = false, message = "Error processing payment: " + ex.Message });
            }

            TempData["SuccessMessage"] = "Your booking has been successfully completed!";
            return Json(new { success = true });
        }
        [HttpGet]
        public ActionResult ShowTestimonial(int bookingId)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Check if the booking date has passed
            if (booking.BookingDate.HasValue && booking.BookingDate.Value < DateTime.Now && !db.Testimonials.Any(t => t.BookingId == bookingId))
            {
                // This booking is eligible for a testimonial
                return PartialView("_TestimonialPopup", booking); // Return a partial view for the popup
            }

            return Json(new { showPopup = false }, JsonRequestBehavior.AllowGet); // No popup needed
        }

        // POST action to submit the testimonial
        [HttpPost]
        public ActionResult SubmitTestimonial(int bookingId, string testimonialText)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Create a new testimonial
            var testimonial = new Testimonial
            {
                UserId = booking.UserID,
                BookingId = bookingId,
                Text = testimonialText,
                CreatedAt = DateTime.Now
            };

            try
            {
                // Add the testimonial to the database
                db.Testimonials.Add(testimonial);
                db.SaveChanges();

                // Optionally, you can set a success message or do additional processing
                TempData["SuccessMessage"] = "Thank you for your testimonial!";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return Json(new { success = false, message = "Error saving testimonial: " + ex.Message });
            }
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
