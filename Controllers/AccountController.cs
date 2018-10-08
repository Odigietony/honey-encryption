using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AuthenticationProject.Models;
using BCrypt.Net;
using System.Threading.Tasks;
using AuthenticationProject.Services;
using System.IO;
using System.Web.Hosting;
using System.Globalization;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Web.Security;

namespace AuthenticationProject.Controllers
{
    public class AccountController : Controller
    {
        public UserModelEntities db = new UserModelEntities();
        public int attempt;

        public AccountController()
        {
            attempt = 3;
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            Session["AttemptsLeft"] = attempt;
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            var _user = db.Users.Where(u => u.Username == user.Username).SingleOrDefault();

            if (_user != null)
            {
                var password = BCrypt.Net.BCrypt.Verify(user.Password, _user.Password);

                var attemptsLeft = Convert.ToInt32(Session["AttemptsLeft"]);
               // if (ModelState.IsValid)
                //{
                    attemptsLeft -= 1;
                    if (password == true)
                    {
                        Session["Username"] = _user.Username.ToString();
                        return RedirectToAction("DashBoard", "Account");
                    }
                    else if (password == false && attemptsLeft != 0)
                    {
                        if (attemptsLeft > 1)
                            ModelState.AddModelError("", "Invalid Email or Password" + " You Have " + attemptsLeft + "  tries left!!");
                        else
                            ModelState.AddModelError("", "Invalid Email or Password" + " You Have " + attemptsLeft + "  try left!!");
                        ViewBag.Tries = attempt;
                        Session["AttemptsLeft"] = attemptsLeft;
                    }
                    else if (password == false && attemptsLeft == 0)
                    {
                        return RedirectToAction("TempUnavailable", "Account");
                    }
                //}
            } 
            //var validateUser = db.Users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password); 
            return View(user);
        }

        public static async Task<string> EmailTemplate(string template)
        {
            var templateFilePath = HostingEnvironment.MapPath("~/Content/templates/") + template + ".html";
            StreamReader streamReader = new StreamReader(templateFilePath);
            var body = await streamReader.ReadToEndAsync();
            streamReader.Close();
            return body;
        }

        [HttpGet]
        public ActionResult TempUnavailable()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> TempUnavailable(User user)
        {
            var _user = db.Users.Where(s => s.Email == user.Email).FirstOrDefault();

            var verifyUser = db.Users.Where(u => u.PhoneNumber == user.PhoneNumber && u.Bvn == user.Bvn).FirstOrDefault();
            if (verifyUser != null)
            {
                return RedirectToAction("DashBoard");
            }
            else
            {
                ViewData["Catch"] = "Access Denied!! Wrong Details provided!";
                var message = await EmailTemplate("SecurityAlert");
                message = message.Replace("@ViewBag.Name", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_user.Username));
                await MessageServices.SendEmail(_user.Email, "Security Alert!!", message); 
            }
            return View();
        }


        public ActionResult DashBoard()
        { 
            return View(); 
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
    }
}