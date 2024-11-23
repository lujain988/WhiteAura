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
                                       .Sum(b => b.Amount.Value );

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
                .Select(g => new { Month = g.Key, Total = g.Sum(b => b.Amount.Value) })
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
            var bookings = db.Bookings
                .Include(b => b.User)
                .Include(b => b.Service) 
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (DateTime.TryParse(searchTerm, out DateTime parsedDate))
                {
                    bookings = bookings.Where(b => DbFunctions.TruncateTime(b.BookingDate) == parsedDate.Date);
                }
                else
                {
                    bookings = bookings.Where(b =>
                        b.User.FullName.Contains(searchTerm) ||
                        b.Service.ServiceName.Contains(searchTerm) ||
                        b.User.Email.Contains(searchTerm)); 
                }
            }

            return View(bookings.ToList());
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
                        category.Image = uploadDir + "/" + fileName;
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

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            var category = db.Categories.Include(a => a.Services).FirstOrDefault(c => c.ID == id);
            if (category != null)
            {
                db.Services.RemoveRange(category.Services);
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
                    string uploadDir = "~/Uploads";
                    string uploadPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

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
                try
                {
                    var existingService = db.Services.Find(service.ID);
                    if (existingService == null)
                    {
                        return Json(new { success = false, message = "Service not found." });
                    }

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

                    string uploadDir = "~/Uploads";
                    string uploadPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var mainImagePath = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(mainImagePath);
                        existingService.Image = uploadDir + "/" + fileName; 
                    }

                    for (int i = 0; i < imgFiles.Length; i++)
                    {
                        if (imgFiles[i] != null && imgFiles[i].ContentLength > 0)
                        {
                            var imgFileName = Path.GetFileName(imgFiles[i].FileName);
                            var imgPath = Path.Combine(uploadPath, imgFileName);
                            imgFiles[i].SaveAs(imgPath);
                            typeof(Service).GetProperty($"img{i + 1}").SetValue(existingService, uploadDir + "/" + imgFileName); 
                        }
                    }

                    db.SaveChanges();
                    return Json(new { success = true, message = "Service updated successfully!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
        }


        [HttpPost]
        public JsonResult DeleteService(int id)
        {
            var service = db.Services.Include(s => s.Bookings).FirstOrDefault(s => s.ID == id);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found." });
            }

            if (service.Bookings != null && service.Bookings.Any())
            {
                db.Bookings.RemoveRange(service.Bookings);
            }

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
                db.ConfirmationDates.RemoveRange(booking.ConfirmationDates);
                db.Payments.RemoveRange(booking.Payments);

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
                return HttpNotFound();  
            }

            if (string.IsNullOrEmpty(contactU.Email))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Contact email is missing.");
            }
            ViewBag.ContactId = contactU.ID;

            ViewBag.Email = contactU.Email; 
            return View();
        }


        [HttpPost]
        public ActionResult SendReply(int id, string email, string message)
        {
            try
            {
                ContactU contactU = db.ContactUs.FirstOrDefault(a => a.ID == id);
                if (contactU == null)
                {
                    return Json(new { success = false, message = "Contact message not found." });
                }

                string subject = "Re: " + contactU.Subject; 

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("whiteaurateam@gmail.com", "qcig jkns ypcl hrnr"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("whiteaurateam@gmail.com", "WhiteAura"),
                    Subject = subject, 
                    Body = message,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);

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

                    string uploadDir = "~/Uploads/Blogs";
                    string uploadPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    if (image != null && image.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(image.FileName);
                        string mainImagePath = Path.Combine(uploadPath, fileName);
                        image.SaveAs(mainImagePath);
                        blog.Image = uploadDir + "/" + fileName;
                    }

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

                    db.Blogs.Add(blog);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Your blog has been created." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Image upload failed: " + ex.Message });
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return Json(new { success = false, message = "There was an error creating the blog." });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBlog(Blog model, HttpPostedFileBase imageFile, IEnumerable<HttpPostedFileBase> imgFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = db.Blogs.Find(model.ID);
                    if (existingBlog != null)
                    {
                        existingBlog.Title = model.Title;
                        existingBlog.Content = model.Content;

                        string uploadDir = "~/Uploads/Blogs";
                        string uploadPath = Server.MapPath(uploadDir);

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        if (imageFile != null && imageFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(imageFile.FileName);
                            string mainImagePath = Path.Combine(uploadPath, fileName);
                            imageFile.SaveAs(mainImagePath);
                            existingBlog.Image = uploadDir + "/" + fileName; 
                        }

                        for (int i = 0; i < imgFiles.Count(); i++)
                        {
                            var imgFile = imgFiles.ElementAt(i);
                            if (imgFile != null && imgFile.ContentLength > 0)
                            {
                                string fileName = Path.GetFileName(imgFile.FileName);
                                string additionalImagePath = Path.Combine(uploadPath, fileName);
                                imgFile.SaveAs(additionalImagePath);
                                existingBlog.GetType().GetProperty($"img{i + 1}")
                                    .SetValue(existingBlog, uploadDir + "/" + fileName); 
                            }
                        }

                        db.SaveChanges();
                        return Json(new { success = true, message = "Blog updated successfully!" });
                    }
                    return Json(new { success = false, message = "Blog not found." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
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
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                testimonial.Status = status;
                db.SaveChanges(); 
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            return Json(new { success = false, message = "Testimonial not found." });
        }

        [HttpPost]
        public JsonResult DeleteTest(int id)
        {
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                db.Testimonials.Remove(testimonial); 

                try
                {
                    db.SaveChanges(); 
                    return Json(new { success = true, message = "Testimonial deleted successfully!" });
                }
                catch (Exception ex)
                {
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