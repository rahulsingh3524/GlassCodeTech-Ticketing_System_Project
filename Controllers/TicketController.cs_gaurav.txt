


using GlassCodeTech_Ticketing_System_Project.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using onlineTicketing.Data;
using onlineTicketing.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace onlineTicketing.Controllers
{
    public class TicketController : Controller
    {
        private readonly TicketDAL _ticketDal;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TicketController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _ticketDal = new TicketDAL(configuration);
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            
            var tickets = _ticketDal.GetUnassignedTicketList();
            return View(tickets);
        }




        //correct

        public IActionResult Create()
        {
            var model = new CreateTicketViewModel();
            ViewBag.PriorityOptions = _ticketDal.GetPriorityOptions();
            return View(model);
        }

        // Add this method to the TicketController class
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
            {
                return userId;
            }
            return 0; // Fallback value
        }

        [HttpPost]
        public IActionResult Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PriorityOptions = _ticketDal.GetPriorityOptions();
                return View(model);
            }

            string attachmentUrl = null;
            if (model.Attachment != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "attachments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Attachment.CopyTo(fileStream);
                }
                attachmentUrl = "/attachments/" + uniqueFileName;
            }

            long userId = GetCurrentUserId(); // Get the actual ID of the logged-in user

            string newTicketId = _ticketDal.CreateTicket(model, userId, attachmentUrl);

            if (!string.IsNullOrEmpty(newTicketId))
            {
                return RedirectToAction("Details", new { ticketId = newTicketId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create ticket.";
                ViewBag.PriorityOptions = _ticketDal.GetPriorityOptions();
                return View(model);
            }
        }


        // Admin Details Correct

        public IActionResult Details(int id)
        {

            long currentUserId = GetCurrentUserId();
            var ticket = _ticketDal.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var threads = _ticketDal.GetTicketThreads(id);
            var supporters = _ticketDal.GetAllSupporters();
            var history = _ticketDal.GetTicketHistory(id); // <-- NEW
            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Open", Text = "Open" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Resolved", Text = "Resolved" },
                new SelectListItem { Value = "Closed", Text = "Closed" }
            };
            var priorityOptions = _ticketDal.GetPriorityOptions();

            var viewModel = new TicketDetailsViewModel
            {
                Ticket = ticket,
                Threads = threads,
                NewMessage = new AddMessageViewModel { TicketId = id },
                StatusOptions = statusOptions,
                Supporters = supporters,
                PriorityOptions = priorityOptions,

                IsSupporter = _ticketDal.GetUserRole(currentUserId) == 2,
                History = history // <-- NEW
            };

            return View(viewModel);
        }


        // correct
        public IActionResult AssignedTicketList()
        {
            var assignedTickets = _ticketDal.GetAssignedTickets();
            return View(assignedTickets);
        }


        // correct

        [HttpPost]
        public IActionResult UpdateTicket(int ticketId, string newStatus, long? supporterId, int? priorityCode, DateTime? deadline)
        {
            //long changedBy = 1;
            long changedBy = GetCurrentUserId();

            try
            {
                // Add a check to ensure supporterId, priorityCode, and deadline have values
                if (supporterId.HasValue && deadline.HasValue)
                {
                    // **CHANGE 1**: Providing a default value for priorityCode if it's null.
                    int finalPriority = priorityCode ?? 1; // Assuming '1' is a default priority like 'Low'.
                    _ticketDal.AssignTicket(ticketId, changedBy, supporterId.Value, finalPriority, deadline.Value);
                    TempData["SuccessMessage"] = "Ticket assigned successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Please fill all assignment fields.";
                }

                if (!string.IsNullOrEmpty(newStatus))
                {
                    _ticketDal.UpdateTicketStatus(ticketId, newStatus, changedBy);
                    // Update the success message if a status update also occurred
                    if (string.IsNullOrEmpty(TempData["SuccessMessage"]?.ToString()))
                    {
                        TempData["SuccessMessage"] = "Ticket status updated successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            var ticket = _ticketDal.GetTicketById(ticketId);
            if (ticket != null)
            {
                return RedirectToAction("Details", new { id = ticket.Id });
            }
            else
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //public IActionResult AssignedTicketList()
        //{
        //    var assignedTickets = _ticketDal.GetAssignedTickets();
        //    return View(assignedTickets);
        //}
        //

        // correct
        public IActionResult updatestatus1()
        {
            var assignedTickets = _ticketDal.GetAssignedTickets();
            return View(assignedTickets);
        }

        // correct
        public IActionResult UpdateStatus(int id)
        {
            var ticket = _ticketDal.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }

            int ticketStatusInt = -1;
            if (int.TryParse(ticket.Status, out int statusValue))
            {
                ticketStatusInt = statusValue;
            }

            string currentStatusName = _ticketDal.GetStatusName(ticketStatusInt);

            var viewModel = new UpdateStatusViewModel
            {
                TicketId = ticket.Id,
                //TicketSubject = ticket.Subject,
                //CurrentStatus = currentStatusName,
                StatusOptions = _ticketDal.GetStatusOptions()
            };

            return View("UpdateStatus", viewModel);
        }

        // correct

        [HttpPost]
        public IActionResult UpdateStatus(UpdateStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 🔍 Debugging ModelState Errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // You can also log this
                }

                // Repopulate dropdown since ModelState failed
                model.StatusOptions = _ticketDal.GetStatusOptions();
                return View("UpdateStatus", model);
            }

            long changedBy = GetCurrentUserId();
            try
            {
                _ticketDal.UpdateTicketStatusAndAddNote(
                    model.TicketId,
                    model.NewStatus,
                    changedBy,
                    model.Note
                );

                TempData["SuccessMessage"] = "Ticket status updated and note added successfully!";
                return RedirectToAction("AssignedTicketList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                model.StatusOptions = _ticketDal.GetStatusOptions();
                return View("UpdateStatus", model);
            }
        }

        // correct


        [HttpPost]
        public IActionResult AddMessage(AddMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string attachmentUrl = null;
                if (model.Attachment != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "attachments");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Attachment.CopyTo(fileStream);
                    }
                    attachmentUrl = "/attachments/" + uniqueFileName;
                }

                //long senderId = 1;
                long senderId = GetCurrentUserId();
                _ticketDal.AddThreadMessage(model.TicketId, senderId, model.Message, attachmentUrl);
            }

            var ticket = _ticketDal.GetTicketById(model.TicketId);
            if (ticket != null)
            {
                //return RedirectToAction("Details", new { ticketId = ticket.TicketId });
                return RedirectToAction("Details", new { id = ticket.Id });

            }
            else
            {
                return NotFound();
            }
        }

        //Report 

        public IActionResult Report()
        {
            var assignedTickets = _ticketDal.GetAssignedTickets();
            return View(assignedTickets);
        }



        //  method to handle filtering
        public IActionResult filter(int? status, int? priority, string category)
        {
            var model = new TicketListViewModel
            {
                // Assign the selected values from the URL parameters
                SelectedStatus = status,
                SelectedPriority = priority,
                SelectedCategory = category,

                // Populate the dropdown options
                StatusOptions = _ticketDal.GetStatusOptions(),
                PriorityOptions = _ticketDal.GetPriorityOptions(),
                CategoryOptions = _ticketDal.GetCategoryOptions()
            };

            // Call the correct method to get filtered tickets
            model.Tickets = _ticketDal.GetFilteredTickets(status, priority, category);

            return View(model);
        }













    }
}










