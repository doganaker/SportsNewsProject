using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SportsNewsProject.Models.ORM.Context;
using SportsNewsProject.Models.ORM.Entities;
using SportsNewsProject.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportsNewsProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SportsNewsContext _newscontext;

        public LoginController(SportsNewsContext newscontext)
        {
            _newscontext = newscontext;
        }

        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(MainLoginVM model)
        {
            if (ModelState.IsValid)
            {
                User user = _newscontext.Users.FirstOrDefault(x => x.EMail == model.EMail && x.Password == model.Password && x.IsActive == true && x.IsDeleted == false);
                if(user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, model.EMail),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.UserData,"Site")
                    };

                    var userIdentity = new ClaimsIdentity(claims, "login");
                    
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                    
                    await HttpContext.SignInAsync("UserScheme", principal);

                    user.LastLogin = DateTime.Now;

                    _newscontext.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.error = "Email veya şifre hatalı";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [Route("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                User email = _newscontext.Users.FirstOrDefault(x => x.EMail == model.Email && x.IsActive == true);

                if (email != null)
                {
                    string resetcode = Guid.NewGuid().ToString();

                    email.ResetCode = resetcode;

                    _newscontext.SaveChanges();

                    string reseturl = "http://sportsnewsprojectapp.herokuapp.com./Login/Reset/" + resetcode;

                    MimeMessage message = new MimeMessage();

                    MailboxAddress from = new MailboxAddress("SportsNewsTeam", "sportsnewsteam.noreply@gmail.com");
                    message.From.Add(from);

                    MailboxAddress to = new MailboxAddress(email.Name, email.EMail);
                    message.To.Add(to);

                    message.Subject = "Reset Password";

                    BodyBuilder bodyBuilder = new BodyBuilder();
                    bodyBuilder.TextBody = "Lütfen şifrenizi sıfırlamak için linke tıklayın: " + reseturl;

                    message.Body = bodyBuilder.ToMessageBody();

                    SmtpClient client = new SmtpClient();

                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("sportsnewsteam.noreply@gmail.com", "$Rdot3PxrtV9QQpYFzVYA#w%RpU2!BGC5UN8cSXNhAs@iq@GvZ");


                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();

                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ModelState.AddModelError("Email", "Girmiş olduğunuz Email adresi sistemimizde kayıtlı değil!");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [HttpGet("Login/Reset/{resetcode}")]
        public IActionResult Reset(string resetcode)
        {
            User user = _newscontext.Users.FirstOrDefault(x => x.ResetCode == resetcode);
            
            ResetPasswordVM model = new ResetPasswordVM();

            model.ResetCode = user.ResetCode;

            return View(model);
        }

        [HttpPost]
        public IActionResult Reset(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                User password = _newscontext.Users.FirstOrDefault(x => x.ResetCode == model.ResetCode);
                password.Password = model.Password;

                _newscontext.SaveChanges();

                return Redirect("/Login/Login/");
            }
            else
            {
                return View();
            }

        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return Redirect("/");
        }
    }
}
