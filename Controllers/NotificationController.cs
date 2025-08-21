using GlassCodeTech_Ticketing_System_Project.Models;
using GlassCodeTech_Ticketing_System_Project.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GlassCodeTech_Ticketing_System_Project.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly CookieService _cookieService;
        public LoginDetail logindata;

        public NotificationController(NotificationService notificationService, CookieService cookieService)
        {
            _notificationService = notificationService;
            _cookieService = cookieService;
            logindata = new LoginDetail();
        }

        [HttpGet]
        public IActionResult GetUnread()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return Json(new { success = false });

            long userId = long.Parse(DatabaseHelper.Decrypt(cookieDict[logindata.Id]));
            var notifications = _notificationService.GetUnreadNotifications(userId);

            return Json(new { success = true, data = notifications });
        }

        [HttpPost]
        public IActionResult MarkAllRead()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return Json(new { success = false });

            long userId = long.Parse(DatabaseHelper.Decrypt(cookieDict[logindata.Id]));
            _notificationService.MarkAllNotificationsAsRead(userId);

            return Json(new { success = true });
        }
    }
}
