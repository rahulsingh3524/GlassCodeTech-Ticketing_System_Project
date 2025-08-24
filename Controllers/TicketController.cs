using Microsoft.AspNetCore.Mvc;
using GlassCodeTech_Ticketing_System_Project.Services;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using GlassCodeTech_Ticketing_System_Project.Models;

namespace GlassCodeTech_Ticketing_System_Project.Controllers
{
    public class TicketController : Controller
    {
        private readonly DatabaseHelper _databaseHelper;
        private readonly CookieService _cookieService;
        private readonly NotificationService _notificationService;
        public LoginDetail logindata;
        public TicketController(DatabaseHelper databaseHelper, CookieService cookieService, NotificationService notificationService)
        {
            _databaseHelper = databaseHelper;
            _cookieService = cookieService;
            logindata = new LoginDetail();
            _notificationService = notificationService;
        }

        // GET: /Ticket/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Ticket/Create
        [HttpPost]
        public IActionResult Create(
            string subject,
            string description,
            string category,
            string priority,
            IFormFile attachment)
        {
            // Get customer ID from cookie
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return RedirectToAction("Login", "Login");
            string customerId = DatabaseHelper.Decrypt(cookieDict[logindata.Id]);

            // Handle attachment
            string attachmentPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
                string filePath = Path.Combine(uploads, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    attachment.CopyTo(fs);
                }
                attachmentPath = "/attachments/" + uniqueFileName;
            }

            // Insert ticket into database
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@customer_id", customerId),
                new SqlParameter("@subject", subject),
                new SqlParameter("@description", description),
                new SqlParameter("@category", category),
                new SqlParameter("@priority", priority),
                new SqlParameter("@created_at", DateTime.Now),
                new SqlParameter("@attachment", (object)attachmentPath ?? DBNull.Value)
            };
            // Insert ticket and get new ticket ID
            var result = _databaseHelper.ExecuteStoredProcedure("sp_CreateTicket", parameters);
            int newId = 0;
            string ticketId = null;

            if (result != null && result.Count > 0)
            {
                var row = result[0]; // not result.ContainsKey!
                if (row.ContainsKey("new_id"))
                    newId = Convert.ToInt32(row["new_id"]);
                if (row.ContainsKey("ticket_id"))
                    ticketId = row["ticket_id"].ToString(); // use ToString() only, no int conversion
            }


            try
            {
                var admins = _databaseHelper.ExecuteStoredProcedure("sp_GetAdmins", null);
                //string ticketId = "TCK-" + DateTime.Now.Year + "-" + newId.ToString().PadLeft(5, '0');

                foreach (var admin in admins)
                {
                    long adminId = Convert.ToInt64(admin["id"]);
                    string message = $"<h3>New Ticket Created</h3>" +
                                   $"<p><strong>Ticket ID:</strong> {ticketId}</p>" +
                                   $"<p><strong>Subject:</strong> {subject}</p>" +
                                   $"<p><strong>Customer ID:</strong> {customerId}</p>" +
                                   $"<p>Please review and assign this ticket.</p>";

                    _notificationService.SendNotificationWithEmail(adminId, newId, 1,
                        $"New Ticket Created: {ticketId}", message);
                }
            }
            catch (Exception ex)
            {
                // Log but don't break the flow
                Console.WriteLine($"Notification failed: {ex.Message}");
            }

            return RedirectToAction("DashboardIndex", "Dashboard");
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return Json(new { success = false, message = "Not authorized." });

            string customerId = DatabaseHelper.Decrypt(cookieDict[logindata.Id]);
            var parameters = new[]
            {
        new SqlParameter("@id", id),
        new SqlParameter("@customer_id", customerId)
    };
            var result = _databaseHelper.ExecuteStoredProcedure("sp_CancelCustomerTicket", parameters);

            if (result != null && result.Count > 0 && result[0].ContainsKey("cancel"))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Unable to cancel ticket (maybe already resolved/closed or not your ticket)." });
            }
        }




        // Show all OPEN/IN PROGRESS tickets for the current customer
        [HttpGet]
        public IActionResult Track()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return RedirectToAction("Login", "Login");
            string customerId = DatabaseHelper.Decrypt(cookieDict[logindata.Id]);

            var parameters = new[] { new SqlParameter("@customer_id", customerId) };
            var tickets = _databaseHelper.ExecuteStoredProcedure("sp_GetCustomerOpenTickets", parameters);
            return View("Track", tickets);
        }

        // Show all CLOSED/RESOLVED tickets for the current customer
        [HttpGet]
        public IActionResult Closed()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return RedirectToAction("Login", "Login");
            string customerId = DatabaseHelper.Decrypt(cookieDict[logindata.Id]);

            var parameters = new[] { new SqlParameter("@customer_id", customerId) };
            var tickets = _databaseHelper.ExecuteStoredProcedure("sp_GetCustomerClosedTickets", parameters);
            return View("Closed", tickets);
        }

        [HttpGet]
        public IActionResult GetRecentTickets()
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return Json(new { success = false, error = "Unauthorized" });

            string customerId = DatabaseHelper.Decrypt(cookieDict[logindata.Id]);
            var parameters = new[] { new SqlParameter("@customer_id", customerId) };
            var tickets = _databaseHelper.ExecuteStoredProcedure("sp_GetCustomerRecentTickets", parameters);

            return Json(new { success = true, data = tickets });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return RedirectToAction("Login", "Login");

            var parameters = new[] { new SqlParameter("@id", id) };
            var ticketResult = _databaseHelper.ExecuteStoredProcedure("sp_GetTicketDetails", parameters);
            if (ticketResult == null || ticketResult.Count == 0)
                return NotFound();
            var ticket = ticketResult[0]; // Dictionary<string, object>
            return View("Details", ticket);
        }


        [HttpPost]
        public IActionResult ChangeStatus(int ticketId, int newStatus)
        {
            try
            {
                var parameters = new[]
                {
            new SqlParameter("@ticket_id", ticketId),
            new SqlParameter("@new_status", newStatus)
        };
                _databaseHelper.ExecuteStoredProcedure("sp_AdminChangeTicketStatus", parameters);

                // Get ticket info (for customer notification and admin details)
                var ticketInfo = _databaseHelper.ExecuteStoredProcedure("sp_GetTicketParticipants",
                        new[] { new SqlParameter("@ticket_id", ticketId) });

                if (ticketInfo != null && ticketInfo.Count > 0)
                {
                    string displayTicketId = ticketInfo[0]["ticket_id"].ToString();
                    string subject = ticketInfo[0]["subject"].ToString();
                    long customerId = Convert.ToInt64(ticketInfo[0]["customer_id"]);
                    string statusName = (newStatus == 3 ? "Resolved" : "Closed");
                    string message = $"<h3>Ticket Status Updated</h3>" +
                                     $"<p><strong>Ticket ID:</strong> {displayTicketId}</p>" +
                                     $"<p><strong>Subject:</strong> {subject}</p>" +
                                     $"<p><strong>New Status:</strong> {statusName}</p>";

                    // Notify customer of status change
                    _notificationService.SendNotificationWithEmail(customerId, ticketId, 3,
                        $"Status Update: {displayTicketId}", message);

                    // --- NEW: Notify all admins of status change ---
                    var adminIds = _notificationService.GetAllAdminIds();  // You should implement this to return a list of admin user IDs

                    string adminMsg = $"<h3>Ticket Status Changed</h3>" +
                                      $"<p><strong>Ticket ID:</strong> {displayTicketId}</p>" +
                                      $"<p><strong>Subject:</strong> {subject}</p>" +
                                      $"<p><strong>New Status:</strong> {statusName}</p>";

                    foreach (var adminId in adminIds)
                    {
                        _notificationService.SendNotificationWithEmail(adminId, ticketId, 5, // Use a code like 5 for admin notifications
                            $"Ticket Status Changed: {displayTicketId}", adminMsg);
                    }
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Could not update status." });
            }
        }

        public List<long> GetAllAdminIds()
        {
            try
            {
                var result = _databaseHelper.ExecuteStoredProcedure("sp_GetAdmins", null);
                return result?.Select(r => Convert.ToInt64(r["id"])).ToList() ?? new List<long>();
            }
            catch
            {
                return new List<long>();
            }
        }


        [HttpGet]
        public IActionResult Assign(int ticketId)
        {
            // Get list of eligible supporters (role = supporter)
            var supporters = _databaseHelper.ExecuteStoredProcedure("sp_GetAllSupporters", null);
            // Get priorities
            var priorities = _databaseHelper.ExecuteStoredProcedure("sp_GetAllPriorities", null);
            // Pass info to view
            ViewBag.TicketId = ticketId;
            ViewBag.Supporters = supporters;
            ViewBag.Priorities = priorities;
            return View();
        }

        [HttpPost]
        public IActionResult AssignTask(int ticketId, long supporterId, int priority, DateTime deadline)
        {
            var cookieDict = _cookieService.GetDictionaryFromCookie("UI");
            if (cookieDict == null || !cookieDict.ContainsKey(logindata.Id))
                return RedirectToAction("Login", "Login");
            long adminId = long.Parse(DatabaseHelper.Decrypt(cookieDict[logindata.Id]));

            var parameters = new[]
            {
        new SqlParameter("@ticket_id", ticketId),
        new SqlParameter("@admin_id", adminId),
        new SqlParameter("@supporter_id", supporterId),
        new SqlParameter("@priority", priority),
        new SqlParameter("@deadline", deadline)
    };
            _databaseHelper.ExecuteStoredProcedure("sp_AssignTicketToSupporter", parameters);


            // **Notify supporter**
            try
            {
                var ticketInfo = _databaseHelper.ExecuteStoredProcedure("sp_GetTicketParticipants",
                    new[] { new SqlParameter("@ticket_id", ticketId) });

                if (ticketInfo != null && ticketInfo.Count > 0)
                {
                    string displayTicketId = ticketInfo[0]["ticket_id"].ToString();
                    string subject = ticketInfo[0]["subject"].ToString();
                    long customerId = Convert.ToInt64(ticketInfo[0]["customer_id"]);

                    // Notify supporter
                    string supporterMessage = $"<h3>Ticket Assigned to You</h3>" +
                                            $"<p><strong>Ticket ID:</strong> {displayTicketId}</p>" +
                                            $"<p><strong>Subject:</strong> {subject}</p>" +
                                            $"<p><strong>Priority:</strong> {priority}</p>" +
                                            $"<p><strong>Deadline:</strong> {deadline:yyyy-MM-dd}</p>";

                    _notificationService.SendNotificationWithEmail(supporterId, ticketId, 2,
                        $"Ticket Assigned: {displayTicketId}", supporterMessage);

                    // Notify customer
                    string customerMessage = $"<h3>Your Ticket Has Been Assigned</h3>" +
                                           $"<p><strong>Ticket ID:</strong> {displayTicketId}</p>" +
                                           $"<p><strong>Subject:</strong> {subject}</p>" +
                                           $"<p>Your ticket has been assigned to a support specialist and will be addressed soon.</p>";

                    _notificationService.SendNotificationWithEmail(customerId, ticketId, 2,
                        $"Ticket Update: {displayTicketId}", customerMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Assignment notification failed: {ex.Message}");
            }


            return RedirectToAction("DashboardIndex", "Dashboard");
        }


    }
}
