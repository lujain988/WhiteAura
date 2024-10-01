using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WhiteAura.Models;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using WhiteAura.DTOs;
using System.Net.Mail;
namespace WhiteAura.Controllers

{
    public class AdminController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();
        public ActionResult Index()
        {
            if (Session["AdminID"] == null)
            {

                return RedirectToAction("LoginAdmin", "Admin");
            }


            ViewBag.AdminName = Session["AdminName"];


            var bookings = db.Bookings.ToList();
            var payment = db.Payments.ToList();
            var totalBookings = bookings.Count;
            var totalRevenue = payment.Where(b=>b.Amount.HasValue)
                                       .Sum(b => b.Amount.Value * 0.5M);

            var monthlyBookings = bookings
                .GroupBy(b => b.BookingDate.HasValue ? b.BookingDate.Value.Month : 0)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(m => m.Month)
                .ToList()
                .Cast<dynamic>()
                .ToList();

            var monthlyRevenue = payment
                .Where(b => b.Amount.HasValue)
                .GroupBy(b => b.Booking.BookingDate.HasValue ? b.Booking.BookingDate.Value.Month : 0)
                .Select(g => new { Month = g.Key, Total = g.Sum(b => b.Amount.Value * 0.5M) })
                .OrderBy(m => m.Month)
                .ToList()
                .Cast<dynamic>()
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                MonthlyBookings = monthlyBookings,
                MonthlyRevenue = monthlyRevenue
            };

            return View(viewModel);



        }


        [HttpGet]
        public ActionResult LoginAdmin()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult LoginAdmin(string email, string password)
        {
            if (ModelState.IsValid)
            {



                var admin = db.Admins.FirstOrDefault(a => a.Email == email && a.PasswordHash == password);

                if (admin != null)
                {

                    Session["AdminID"] = admin.AdminID;
                    Session["AdminName"] = admin.AdminName;
                    Session["Role"] = admin.Role;

                    return RedirectToAction("Index", "Admin");
                }


                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View();
        }


        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("LoginAdmin", "Admin");
        }
        public ActionResult GetUser()
        {
            var user = db.Users.ToList();
            return View(user);
        }

        public ActionResult GetVendros(string searchTerm = null)
        {
            var vendors = db.Services.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                vendors = vendors.Where(v => v.ServiceName.Contains(searchTerm) ||
                                              v.Category.CategoryName.Contains(searchTerm) ||
                                              v.Type.Contains(searchTerm));
            }

            return View(vendors.ToList());
        }


        public ActionResult GetCategory()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

        public ActionResult GetBooking(string searchTerm)
        {
            // Start with the initial query
            var bookings = db.Bookings
                .Include(b => b.User) // Include User to access FullName and Email
                .Include(b => b.Service) // Include Service to access ServiceName
                .AsQueryable(); // Start with an IQueryable

            // Apply filtering based on the search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Check if the search term is a valid date
                if (DateTime.TryParse(searchTerm, out DateTime parsedDate))
                {
                    // Filter by exact date match using DbFunctions.TruncateTime
                    bookings = bookings.Where(b => DbFunctions.TruncateTime(b.BookingDate) == parsedDate.Date);
                }
                else
                {
                    // Filter by user name, service name, or email
                    bookings = bookings.Where(b =>
                        b.User.FullName.Contains(searchTerm) || // Filter by user name
                        b.Service.ServiceName.Contains(searchTerm) || // Filter by service name
                        b.User.Email.Contains(searchTerm)); // Filter by user email
                }
            }

            return View(bookings.ToList()); // Convert to List before passing to the view
        }



        public ActionResult GetContact()
        {
            var contact = db.ContactUs.ToList();
            return View(contact);
        }

        public ActionResult DetailsContact(int id)
        {
            var contactMessage = db.ContactUs.Find(id);
            if (contactMessage == null)
            {
                return HttpNotFound();
            }
            return View(contactMessage);
        }
        public ActionResult DetailsVendors(int id)
        {
            var service = db.Services.Include(s => s.Category).FirstOrDefault(s => s.ID == id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        public ActionResult DetailsServices(int id)
        {
            var service = db.Categories.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }


        public ActionResult AddCategory()
        {
            return View();
        }



        [HttpPost]
        public ActionResult AddCategory(Category category, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    try
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string uploadDir = "~/Uploads";
                        string uploadPath = Path.Combine(Server.MapPath(uploadDir), fileName);

                        if (!Directory.Exists(Server.MapPath(uploadDir)))
                        {
                            Directory.CreateDirectory(Server.MapPath(uploadDir));
                        }

                        imageFile.SaveAs(uploadPath);
                        category.Image = uploadDir + "/" + fileName; // Save the path to the category object
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Image upload failed: " + ex.Message });
                    }
                }

                db.Categories.Add(category);
                db.SaveChanges();

                return Json(new { success = true, message = "Category added successfully." });
            }

            // Return validation errors if the model state is not valid
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            var category = db.Categories.Include(a => a.Services).FirstOrDefault(c => c.ID == id);
            if (category != null)
            {
                // Optionally, handle the deletion of associated vendors
                db.Services.RemoveRange(category.Services); // If you want to delete vendors associated with the category
                db.Categories.Remove(category);
                db.SaveChanges();
                return Json(new { success = true, message = "Category deleted successfully." });
            }
            return Json(new { success = false, message = "Category not found." });
        }

        // GET: Admin/EditCategory/5
        public ActionResult EditCategory(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(Category category, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = db.Categories.Find(category.ID);
                if (existingCategory != null)
                {
                    existingCategory.CategoryName = category.CategoryName;

                    // Handle image upload if a new file is provided
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(imageFile.FileName);
                            string uploadDir = "~/Uploads";
                            string uploadPath = Path.Combine(Server.MapPath(uploadDir), fileName);

                            if (!Directory.Exists(Server.MapPath(uploadDir)))
                            {
                                Directory.CreateDirectory(Server.MapPath(uploadDir));
                            }

                            imageFile.SaveAs(uploadPath);
                            existingCategory.Image = uploadDir + "/" + fileName;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                            return View(category);
                        }
                    }

                    existingCategory.Description = category.Description;
                    existingCategory.Details = category.Details;
                    db.SaveChanges();

                    return RedirectToAction("GetCategory");
                }
            }
            return View(category);
        }
        [HttpGet]
        public ActionResult CreateService()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "CategoryName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateService(Service service,
            HttpPostedFileBase image,
            HttpPostedFileBase img1,
            HttpPostedFileBase img2,
            HttpPostedFileBase img3,
            HttpPostedFileBase img4,
            HttpPostedFileBase img5,
            HttpPostedFileBase img6,
            HttpPostedFileBase img7)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Define the upload directory
                    string uploadDir = "~/Uploads";
                    string uploadPath = Server.MapPath(uploadDir);

                    // Ensure the upload directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Upload the main image
                    if (image != null && image.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(image.FileName);
                        string mainImagePath = Path.Combine(uploadPath, fileName);
                        image.SaveAs(mainImagePath);
                        service.Image = uploadDir + "/" + fileName;
                    }

                    for (int i = 1; i <= 7; i++)
                    {
                        var imgFile = this.HttpContext.Request.Files[$"img{i}"];
                        if (imgFile != null && imgFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(imgFile.FileName);
                            string additionalImagePath = Path.Combine(uploadPath, fileName);
                            imgFile.SaveAs(additionalImagePath);
                            typeof(Service).GetProperty($"img{i}").SetValue(service, uploadDir + "/" + fileName);
                        }
                    }

                    db.Services.Add(service);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Service created successfully." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Image upload failed: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        //public ActionResult EditService(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
        //    }
        //    Service service = db.Services.Find(id);
        //    if (service == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CategoryID = new SelectList(db.Categories, "ID", "CategoryName", service.CategoryID);
        //    return View(service);
        //}
        public ActionResult EditServiceVendor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "CategoryName", service.CategoryID);
            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult EditServiceVendor(Service service, HttpPostedFileBase imageFile, HttpPostedFileBase[] imgFiles)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing service from the database
                var existingService = db.Services.Find(service.ID);
                if (existingService == null)
                {
                    return HttpNotFound();
                }

                // Update the existing service properties
                existingService.ServiceName = service.ServiceName;
                existingService.Description = service.Description;
                existingService.Price = service.Price;
                existingService.CreatedAt = service.CreatedAt;
                existingService.CategoryID = service.CategoryID;
                existingService.Type = service.Type;
                existingService.City = service.City;
                existingService.Details = service.Details;
                existingService.NumbeOfGuests = service.NumbeOfGuests;
                existingService.Location = service.Location;
                existingService.LocationPlace = service.LocationPlace;

                // Handle the image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    imageFile.SaveAs(path);
                    existingService.Image = fileName; // Save the new image name
                }

                // Handle multiple image uploads
                for (int i = 0; i < imgFiles.Length; i++)
                {
                    if (imgFiles[i] != null && imgFiles[i].ContentLength > 0)
                    {
                        var imgFileName = Path.GetFileName(imgFiles[i].FileName);
                        var imgPath = Path.Combine(Server.MapPath("~/Uploads"), imgFileName);
                        imgFiles[i].SaveAs(imgPath);
                        existingService.GetType().GetProperty("img" + (i + 1)).SetValue(existingService, imgFileName); // Update the corresponding img property
                    }
                }

                // Save changes to the database
                db.SaveChanges();
                ViewBag.SuccessMessage = "Service updated successfully!";
                return Json(new { success = true, message = "Service updated successfully!" });
            }

            // If model state is invalid, redisplay the form
            return Json(new { success = false, message = "There was an error updating the service." });
        }

        [HttpPost]
        public JsonResult DeleteService(int id)
        {
            var service = db.Services.Include(s => s.Bookings).FirstOrDefault(s => s.ID == id);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found." });
            }

            // Delete related bookings first
            if (service.Bookings != null && service.Bookings.Any())
            {
                db.Bookings.RemoveRange(service.Bookings);
            }

            // Now delete the service
            db.Services.Remove(service);
            db.SaveChanges();

            return Json(new { success = true, message = "Service deleted successfully." });
        }


        public ActionResult CreateBooking()
        {
            ViewBag.UserID = new SelectList(db.Users, "ID", "Email");
            ViewBag.ServiceID = new SelectList(db.Services, "ID", "ServiceName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.CreatedAt = DateTime.Now;

                db.Bookings.Add(booking);
                db.SaveChanges();

                return RedirectToAction("GetBooking");
            }

            ViewBag.UserID = new SelectList(db.Users, "ID", "Email", booking.UserID);
            ViewBag.ServiceID = new SelectList(db.Services, "ID", "ServiceName", booking.ServiceID);
            return View(booking);
        }

        [HttpPost]
        public JsonResult DeleteBooking(int id)
        {
            var booking = db.Bookings.Include(b => b.Payments).FirstOrDefault(b => b.ID == id);
            if (booking != null)
            {
                // Remove all related payments
                db.ConfirmationDates.RemoveRange(booking.ConfirmationDates);
                db.Payments.RemoveRange(booking.Payments);

                // Remove the booking
                db.Bookings.Remove(booking);
                db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Booking not found." });
        }

        public ActionResult ReplyContact(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactU contactU = db.ContactUs.Find(id);

            if (contactU == null)
            {
                return HttpNotFound();  // Return 404 if no contact is found
            }

            // Ensure the email is not null
            if (string.IsNullOrEmpty(contactU.Email))
            {
                // Handle the case where the email is missing
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Contact email is missing.");
            }
            ViewBag.ContactId = contactU.ID;

            ViewBag.Email = contactU.Email; // Set the email to ViewBag
            return View();
        }


        [HttpPost]
        public ActionResult SendReply(int id, string email, string message)
        {
            try
            {
                // Retrieve the contact message using the ID
                ContactU contactU = db.ContactUs.FirstOrDefault(a => a.ID == id);
                if (contactU == null)
                {
                    return Json(new { success = false, message = "Contact message not found." });
                }

                // Prepend "Re:" to the original subject
                string subject = "Re: " + contactU.Subject; // Use the original subject with "Re:"

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("whiteaurateam@gmail.com", "qcig jkns ypcl hrnr"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("whiteaurateam@gmail.com", "WhiteAura"),
                    Subject = subject, // Set the subject as "Re: [Original Subject]"
                    Body = message,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);

                // Mark as replied
                contactU.IsReplied = true;
                db.Entry(contactU).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending email: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteContact(int id)
        {
            try
            {
                var contact = db.ContactUs.Find(id);
                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found." });
                }

                db.ContactUs.Remove(contact);
                db.SaveChanges();

                return Json(new { success = true, message = "Contact deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting contact: " + ex.Message });
            }
        }



        public ActionResult Testimonials()
        {

            return View();
        }


        public ActionResult Blog()
        {
            var blogs = db.Blogs.ToList();
            return View(blogs);
        }

        // GET: Blog/Create
        public ActionResult CreateBlog()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateBlog(Blog blog, HttpPostedFileBase image, HttpPostedFileBase img1, HttpPostedFileBase img2, HttpPostedFileBase img3, HttpPostedFileBase img4)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    blog.CreatedAt = DateTime.Now;

                    // Define the upload directory
                    string uploadDir = "~/Uploads/Blogs";
                    string uploadPath = Server.MapPath(uploadDir);

                    // Ensure the upload directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Upload the main image
                    if (image != null && image.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(image.FileName);
                        string mainImagePath = Path.Combine(uploadPath, fileName);
                        image.SaveAs(mainImagePath);
                        blog.Image = uploadDir + "/" + fileName;
                    }

                    // Upload additional images
                    for (int i = 1; i <= 4; i++)
                    {
                        var imgFile = this.HttpContext.Request.Files[$"img{i}"];
                        if (imgFile != null && imgFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(imgFile.FileName);
                            string additionalImagePath = Path.Combine(uploadPath, fileName);
                            imgFile.SaveAs(additionalImagePath);
                            typeof(Blog).GetProperty($"img{i}").SetValue(blog, uploadDir + "/" + fileName);
                        }
                    }

                    // Save the blog to the database
                    db.Blogs.Add(blog);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Your blog has been created." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Image upload failed: " + ex.Message });
                }
            }

            // Log ModelState errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return Json(new { success = false, message = "There was an error creating the blog." });
        }

        // GET: Load the blog edit form
        [HttpGet]
        public ActionResult EditBlog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }

        // POST: Save the edited blog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBlog(Blog model, HttpPostedFileBase imageFile, IEnumerable<HttpPostedFileBase> imgFiles)
        {
            if (ModelState.IsValid)
            {
                // Fetch the existing blog entry from the database
                var existingBlog = db.Blogs.Find(model.ID);
                if (existingBlog != null)
                {
                    // Update the properties from the model
                    existingBlog.Title = model.Title;
                    existingBlog.Content = model.Content;

                    // Only update the main image if a new file is uploaded
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Save the new main image
                        var mainImagePath = Path.Combine(Server.MapPath("~/Uploads/"), Path.GetFileName(imageFile.FileName));
                        imageFile.SaveAs(mainImagePath);
                        existingBlog.Image = imageFile.FileName;
                    }

                    // Handle additional images
                    for (int i = 0; i < imgFiles.Count(); i++)
                    {
                        var imgFile = imgFiles.ElementAt(i);
                        if (imgFile != null && imgFile.ContentLength > 0)
                        {
                            // Save the new additional image
                            var additionalImagePath = Path.Combine(Server.MapPath("~/Uploads/"), Path.GetFileName(imgFile.FileName));
                            imgFile.SaveAs(additionalImagePath);
                            existingBlog.GetType().GetProperty("img" + (i + 1)).SetValue(existingBlog, imgFile.FileName);
                        }
                    }

                    // Save changes to the database
                    db.SaveChanges();
                    return Json(new { success = true, message = "Blog updated successfully!" });
                }
            }
            return Json(new { success = false, message = "Failed to update blog." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBlog(int id)
        {
            try
            {
                var blog = db.Blogs.Find(id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Blog not found." });
                }

                db.Blogs.Remove(blog);
                db.SaveChanges();

                return Json(new { success = true, message = "Blog deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting blog: " + ex.Message });
            }
        }


        // GET: Blog/Details/5
        public ActionResult DetailsBlog(int id)
        {
            var blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }


        public ActionResult GetTest()
        {

            var Test = db.Testimonials.ToList();
            return View(Test);
        }

        [HttpPost]
        public JsonResult UpdateStatus(int id, string status)
        {
            // Find the testimonial by ID
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                testimonial.Status = status; // Update status
                db.SaveChanges(); // Save changes to the database
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            return Json(new { success = false, message = "Testimonial not found." });
        }

        [HttpPost]
        public JsonResult DeleteTest(int id)
        {
            // Find the testimonial by ID
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                db.Testimonials.Remove(testimonial); // Remove the testimonial

                try
                {
                    db.SaveChanges(); // Save changes to the database
                    return Json(new { success = true, message = "Testimonial deleted successfully!" });
                }
                catch (Exception ex)
                {
                    // Log the error message (using a logging framework or console)
                    Console.WriteLine(ex.Message);
                    return Json(new { success = false, message = "An error occurred while deleting the testimonial." });
                }
            }
            return Json(new { success = false, message = "Testimonial not found." });
        }
        [HttpGet]
        public JsonResult Details(int id)
        {
            var testimonial = db.Testimonials.Include(t => t.User).FirstOrDefault(t => t.ID == id);
            if (testimonial == null)
            {
                return Json(new { success = false, message = "Testimonial not found." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, testimonial }, JsonRequestBehavior.AllowGet);
        }



    }
}