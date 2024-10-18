using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WhiteAura.DTOs;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class ServicesController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();

        // GET: Services
        [HttpGet]
        public ActionResult Services()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

public ActionResult Placese(int? id, string city, string type, decimal? price)
    {
        var services = db.Services.AsQueryable();

        if (id != null)
        {
            services = services.Where(s => s.CategoryID == id);
        }

        if (!string.IsNullOrEmpty(city) && city != "all")
        {
            services = services.Where(s => s.City == city);
        }

        if (!string.IsNullOrEmpty(type) && type != "all")
        {
            services = services.Where(s => s.Type == type);
        }

        if (price != null)
        {
            services = services.Where(s => s.Price <= price);
        }

        // Populate the ViewBag with cities
        ViewBag.Cities = db.Services.Select(s => s.City).Distinct().ToList();

        // Optionally handle if the query returns no cities, just to avoid another null issue
        if (ViewBag.Cities == null || ViewBag.Cities.Count == 0)  // Changed to Count
        {
            ViewBag.Cities = new List<string> { "No Cities Available" }; // Placeholder
        }


        return View(services.ToList());
    }

    [HttpGet]
        public ActionResult Planner(int? id)
        {
            List<Service> services;
            if (id == null)
            {
                services = db.Services.ToList();
            }
            else
            {
                services = db.Services.Where(a => a.CategoryID == id).ToList();
            }

            ViewBag.CategoryId = id;
            return View(services);
        }




        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound("No Content here");
            }
            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View("Details", service);
        }

        [HttpGet]
        public ActionResult Book(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Login", "LoginandSignup");
            }

            int? userId = Session["UserId"] as int?;

            // Retrieve service details
            var service = db.Services
                            .Where(s => s.ID == id)
                            .Select(s => new
                            {
                                s.ServiceName,
                                Price = s.Price ?? 0.0m,
                                s.Type,
                                s.City,
                                NumbeOfGuests = s.NumbeOfGuests ?? 0,
                                s.Details,
                                s.Image
                            })
                            .FirstOrDefault();

            // Retrieve user details
            var user = db.Users
                         .Where(u => u.ID == userId)
                         .Select(u => new
                         {
                             u.FullName,
                             u.Email,
                             u.PhoneNumber
                         })
                         .FirstOrDefault();

            // Initialize BookingViewModel
            var booking = new BookingViewModel
            {
                ServiceID = id.Value,
                ServiceName = service.ServiceName,
                Price = service.Price,
                Place = service.ServiceName,
                NumberOfGuests = 0,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                // Use DateTime.UtcNow instead of DateTime.Now
                WeddingDate = DateTime.UtcNow.Date
            };

            // Get unavailable dates and reserved hours
            // Get unavailable dates and reserved hours
            var reservedBookings = db.Bookings
                .Where(b => b.ServiceID == id && b.Status == "Confirmed")
                .Select(b => new
                {
                    // Retrieve BookingDate directly without converting it
                    BookingDate = DbFunctions.TruncateTime(b.BookingDate),
                    ReservedHour = b.ReservedHours
                })
                .ToList();

            // Now, convert BookingDate to UTC after fetching from the database
            var unavailableHoursByDate = reservedBookings
                .GroupBy(rb => rb.BookingDate)
                .ToDictionary(
                    g => g.Key?.ToUniversalTime().ToString("yyyy-MM-dd") ?? string.Empty, // Convert to UTC after grouping
                    g => g.Select(x => x.ReservedHour).ToList()
                );

            if (!unavailableHoursByDate.Any())
            {
                System.Diagnostics.Debug.WriteLine("No unavailable hours found.");
            }

            // Pass to ViewBag
            ViewBag.UnavailableHoursByDate = unavailableHoursByDate;

            return View(booking);
        }



        [HttpPost]
        public ActionResult ConfirmBooking(BookingViewModel model)
        {
            // Ensure the wedding date is selected
            if (model.WeddingDate == null)
            {
                ViewBag.ErrorMessage = "Please choose a wedding date before selecting a time.";
                return View("Book", model);
            }

            // Ensure the wedding time is selected
            if (model.ReservedHours == null)
            {
                ViewBag.ErrorMessage = "Please select a time for your wedding.";
                return View("Book", model);
            }

            // Check if the time is valid
            TimeSpan reservedTime = model.ReservedHours.Value;

            // Check if the time slot is available
            bool isDateTimeAvailable = !db.Bookings.Any(b =>
                b.ServiceID == model.ServiceID &&
                DbFunctions.TruncateTime(b.BookingDate) == DbFunctions.TruncateTime(model.WeddingDate) &&
                b.ReservedHours == reservedTime &&
                (b.Status == "Paid" || b.Status == "Confirmed"));

            if (!isDateTimeAvailable)
            {
                ViewBag.ErrorMessage = "This time is already reserved. Please choose a different time.";
                return View("Book", model);
            }

            // Combine date and time for booking
            DateTime weddingDateTime = model.WeddingDate.Value.Add(reservedTime);

            // Proceed to save the booking with both date and time
            var booking = new Booking
            {
                ServiceID = model.ServiceID,
                UserID = (int)(Session["UserId"] ?? 0),
                BookingDate = weddingDateTime,
                NumOfGuest = model.NumberOfGuests,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                ReservedHours = reservedTime
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            //// Add confirmation logic
            //var confirmationDate = new ConfirmationDate
            //{
            //    BookingsID = booking.ID,
            //    Status = "Pending",
            //    CreatedAt = DateTime.Now,

            //};

            //db.ConfirmationDates.Add(confirmationDate);
            db.SaveChanges(); // Save confirmation

            // Additional logic for confirmation and payment...

            return RedirectToAction("Payment", "Paymeant", new { bookingId = booking.ID });
        }


        private void CheckAndDeleteExpiredBookings()
        {
            var expiredBookings = db.Bookings.Include(b => b.ConfirmationDates)
                                              .Where(b => b.PaymentDeadline < DateTime.Now &&
                                                          b.Status == "Confirmed" &&
                                                          (b.IsPaid == null || b.IsPaid == false)) // Check for unpaid bookings
                                              .ToList();

            foreach (var bookingToDelete in expiredBookings)
            {
                // Remove related confirmation dates first
                db.ConfirmationDates.RemoveRange(bookingToDelete.ConfirmationDates);

                // Now remove the booking
                db.Bookings.Remove(bookingToDelete);
            }

            db.SaveChanges();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBooking(int id)
        {
            var booking = db.Bookings
                            .Include(b => b.ConfirmationDates)
                            .Include(b => b.Payments)
                            .FirstOrDefault(b => b.ID == id);
            if (booking != null)
            {
                db.ConfirmationDates.RemoveRange(booking.ConfirmationDates);
                db.Payments.RemoveRange(booking.Payments);
                db.SaveChanges();

                db.Bookings.Remove(booking);
                db.SaveChanges();

                return RedirectToAction("Profile", "LoginandSignup");
            }

            return RedirectToAction("Profile", "LoginandSignup");
        }



    }
}
