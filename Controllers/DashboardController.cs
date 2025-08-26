using Microsoft.AspNetCore.Mvc;
using GlassCodeTech_Ticketing_System_Project.Services;
using GlassCodeTech_Ticketing_System_Project.Models;
using System.Data.SqlClient;

namespace GlassCodeTech_Ticketing_System_Project.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CookieService _cookieService;
        private readonly DatabaseHelper _databaseHelper;
        public LoginDetail logindata; 
        public DashboardController(CookieService cookieService, DatabaseHelper databaseHelper)
        {
            _cookieService = cookieService;
            logindata = new LoginDetail();
            _databaseHelper = databaseHelper;
        }

        public IActionResult DashboardIndex()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Role))
                return RedirectToAction("Login", "Login");

            // Role is stored in encrypted format, so decrypt it
            var role = DatabaseHelper.Decrypt(cookieDict[logindata.Role]);
            // Optionally: ToUpperInvariant() for consistency
            switch (role.ToUpperInvariant())
            {
                case "2":
                    AdminDashboard();
                    return View("AdminDashboard");
                case "3":
                    SupporterDashboard();
                    return View("SupporterDashboard");
                case "1":
                    return View("CustomerDashboard");
                default:
                    return RedirectToAction("Login", "Login"); // Unknown role, force re-login
            }
        }
        public void AdminDashboard()
        {
            // Add admin authentication/role check as needed
            // (Assume admin session is checked before.)

            // Example stored procedure calls - you should create these SPs as needed
            var totalTickets = _databaseHelper.ExecuteStoredProcedure("sp_AdminTotalTickets", null);
            var assignedTickets = _databaseHelper.ExecuteStoredProcedure("sp_AdminAssignedTickets", null);
            var assignmentList = _databaseHelper.ExecuteStoredProcedure("sp_AdminTicketAssignments", null);
            var statusCounts = _databaseHelper.ExecuteStoredProcedure("sp_AdminTicketStatusCounts", null);
            var allTickets = _databaseHelper.ExecuteStoredProcedure("sp_AdminAllTickets", null);
            ViewBag.AllTickets = allTickets;
            // Put results in ViewBag for simplicity, or use a structured ViewModel
            ViewBag.TotalTickets = totalTickets?[0]?["count"] ?? 0;
            ViewBag.AssignedTickets = assignedTickets?[0]?["count"] ?? 0;
            ViewBag.AssignmentList = assignmentList;
            ViewBag.StatusCounts = statusCounts;
        }
        public void SupporterDashboard()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            long supporterId = long.Parse(DatabaseHelper.Decrypt(cookieDict[logindata.Id]));
            var assignedTickets = _databaseHelper.ExecuteStoredProcedure(
                "sp_SupporterAssignedTickets",
                new[] { new SqlParameter("@supporter_id", supporterId) });
            var statusCounts = _databaseHelper.ExecuteStoredProcedure(
                "sp_SupporterStatusCounts",
                new[] { new SqlParameter("@supporter_id", supporterId) });
            ViewBag.Tickets = assignedTickets;
            ViewBag.StatusCounts = statusCounts;
        }
    

        [HttpPost]
        public IActionResult UpdateTicketStatus(int ticketId, int newStatus, string remark)         /*Method for Updating the new Status and remark*/
        {

            var parameters = new[]
            {
        new SqlParameter("@ticket_id", ticketId),
        new SqlParameter("@new_status", newStatus),
        
        new SqlParameter("@remark", string.IsNullOrEmpty(remark) ? (object)DBNull.Value : remark)
    };

            _databaseHelper.ExecuteStoredProcedure("sp_AdminChangeTicketStatus", parameters);

            // Return a JSON response for AJAX success handling
            return Json(new { success = true });
            
        }

    }
}
