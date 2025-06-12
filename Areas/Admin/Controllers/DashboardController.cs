using FinalProject.MVC.Views.Account.Enums;
using FinalProject.MVC.Views.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinalProject.MVC.Areas.Admin;
using Microsoft.AspNetCore.Identity;
using System.Net;
namespace FinalProject.MVC.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin, Moderator")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
