
using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using onlineTicketing.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace onlineTicketing.Data
{
    public class TicketDAL
    {
        private readonly string _connectionString;

        public TicketDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // A new helper method was added to get the status string from an ID.
        private string GetStatusString(int statusId)
        {
            return statusId switch
            {
                1 => "Open",
                2 => "In Progress",
                3 => "Resolved",
                4 => "Closed",
                _ => "Unknown"
            };
        }

        private int GetStatusId(string status)
        {
            return status.ToLower() switch
            {
                "open" => 1,
                "in progress" => 2,
                "resolved" => 3,
                "closed" => 4,
                _ => 1
            };
        }

        // correct

        public string CreateTicket(CreateTicketViewModel model, long customerId, string attachmentUrl)
        {
            string newTicketId = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_Create", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@customer_id", customerId);
                cmd.Parameters.AddWithValue("@subject", model.Subject);
                cmd.Parameters.AddWithValue("@description", model.Description);
                cmd.Parameters.AddWithValue("@category", model.Category);
                cmd.Parameters.AddWithValue("@priority", model.Priority);
                cmd.Parameters.AddWithValue("@status", 1); // Status for 'Open'
                cmd.Parameters.AddWithValue("@attachment_url", attachmentUrl ?? (object)DBNull.Value); // Handle null attachments

                con.Open();

                // Use ExecuteScalar to get the single value returned by the stored procedure
                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    newTicketId = result.ToString();
                }
            }
            return newTicketId;
        }

        //correct

        public List<TicketViewModel> GetAssignedTickets()
        {
            var tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Change the command to call the stored procedure that returns the deadline
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetAssignedTickets", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int statusId = Convert.ToInt32(reader["status"]);
                    int priorityId = Convert.ToInt32(reader["priority"]);

                    tickets.Add(new TicketViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = reader["ticket_id"].ToString(),
                        Subject = reader["subject"].ToString(),
                        Status = GetStatusString(statusId),
                        Priority = GetPriorityString(priorityId),
                        CreatedAt = Convert.ToDateTime(reader["created_at"]),
                        AssignedToName = reader["AssignedToName"].ToString(),
                        // This line is where the error occurs. It expects a 'deadline' column from the DB.
                        Deadline = reader["deadline"] != DBNull.Value ? Convert.ToDateTime(reader["deadline"]) : (DateTime?)null
                    });
                }
            }
            return tickets;
        }

        // correct

        public void AssignTicket(int ticketId, long adminId, long supporterId, int priorityCode, DateTime deadline)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_AssignWithDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ticket_id", ticketId);
                cmd.Parameters.AddWithValue("@supporter_id", supporterId);
                cmd.Parameters.AddWithValue("@priority_code", priorityCode);
                cmd.Parameters.AddWithValue("@deadline", deadline);
                cmd.Parameters.AddWithValue("@admin_id", adminId); // Add this parameter
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // correct

        public void UpdateTicketStatus(int ticketId, string newStatus, long changedBy)
        {
            int statusId = GetStatusId(newStatus);
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_UpdateStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ticket_id", ticketId);
                cmd.Parameters.AddWithValue("@status_code", statusId);
                cmd.Parameters.AddWithValue("@changed_by", changedBy);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        //correct

        public TicketViewModel GetTicketById(int ticketId)
        {
            TicketViewModel ticket = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", ticketId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ticket = new TicketViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = reader["ticket_id"].ToString(),
                        Subject = reader["subject"].ToString(),
                        Description = reader["description"].ToString(),
                        Category = reader["category"].ToString(),
                        Priority = reader["priority"].ToString(),
                        Status = reader["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                        /*AssignedTo = reader["assigned_to"].ToString()*/ // Make sure your SP returns this.
                    };
                }
            }
            return ticket;
        }

        // correct

        public string GetStatusName(int statusCode)
        {
            string statusName = string.Empty;
            string query = "SELECT status_name FROM status WHERE status_code = @statusCode";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@statusCode", statusCode);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    statusName = result.ToString();
                }
            }
            return statusName;
        }

        // correct

        public List<SelectListItem> GetStatusOptions()
        {
            var statusOptions = new List<SelectListItem>();
            string query = "SELECT status_code, status_name FROM status ORDER BY status_name";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    statusOptions.Add(new SelectListItem
                    {
                        Value = reader["status_code"].ToString(),
                        Text = reader["status_name"].ToString()
                    });
                }
            }
            return statusOptions;
        }

        // correct

        public void UpdateTicketStatusAndAddNote(int ticketId, string newStatusCode, long changedBy, string note)
        {
            int statusId = int.Parse(newStatusCode);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_UpdateStatusAndAddNote", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ticket_id", ticketId);
                cmd.Parameters.AddWithValue("@new_status_code", statusId);
                cmd.Parameters.AddWithValue("@changed_by", changedBy);
                cmd.Parameters.AddWithValue("@note", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // correct

        public void AddThreadMessage(int ticketId, long senderId, string message, string attachmentUrl = null)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_AddThreadMessage", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ticket_id", ticketId);
                cmd.Parameters.AddWithValue("@sender_id", senderId);
                cmd.Parameters.AddWithValue("@message", message);
                cmd.Parameters.AddWithValue("@attachment_url", (object)attachmentUrl ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }







        public List<TicketViewModel> GetTicketsByUserId(long customerId)
        {
            List<TicketViewModel> tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetByUserId", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@customer_id", customerId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = reader["ticket_id"].ToString(),
                        Subject = reader["subject"].ToString(),
                        Description = reader["description"].ToString(),
                        Category = reader["category"].ToString(),
                        Priority = reader["priority"].ToString(),
                        Status = reader["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            return tickets;
        }

        public List<TicketViewModel> GetUnassignedTicketList()
        {
            List<TicketViewModel> tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Ticket_unassigned", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = reader["ticket_id"].ToString(),
                        Subject = reader["subject"].ToString(),
                        Description = reader["description"].ToString(),
                        Category = reader["category"].ToString(),
                        Priority = reader["priority"].ToString(),
                        Status = reader["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            return tickets;
        }



        public TicketViewModel GetTicketByTicketId(string ticketId)
        {
            TicketViewModel ticket = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT id, ticket_id, customer_id, subject, description, category, priority, status, created_at FROM tickets WHERE ticket_id = @ticketId";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ticketId", ticketId);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    ticket = new TicketViewModel
                    {
                        Id = Convert.ToInt32(rdr["id"]),
                        TicketId = rdr["ticket_id"].ToString(),
                        Subject = rdr["subject"].ToString(),
                        Description = rdr["description"].ToString(),
                        Category = rdr["category"].ToString(),
                        Priority = rdr["priority"].ToString(),
                        Status = rdr["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(rdr["created_at"])
                    };
                }
            }
            return ticket;
        }

        public List<TicketThreadViewModel> GetTicketThreads(int ticketId)
        {
            List<TicketThreadViewModel> threads = new List<TicketThreadViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetTicketThreads", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ticket_id", ticketId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    threads.Add(new TicketThreadViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        SenderId = Convert.ToInt64(reader["sender_id"]),
                        SenderName = reader["sender_name"].ToString(),
                        Message = reader["message"].ToString(),
                        AttachmentUrl = reader["attachment_url"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            return threads;
        }







        public List<SelectListItem> GetAllSupporters()
        {
            var supporters = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Users_GetAllSupporters", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    supporters.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["name"].ToString()
                    });
                }
            }
            return supporters;
        }

        public List<SelectListItem> GetPriorityOptions()
        {
            var priorities = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Priorities_GetAll", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    priorities.Add(new SelectListItem
                    {
                        Value = reader["priority_code"].ToString(),
                        Text = reader["priority_name"].ToString()
                    });
                }
            }
            return priorities;
        }

        public int GetUserRole(long userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT role FROM dbo.users WHERE id = @userId", con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }
            return -1;
        }


        private string GetPriorityString(int priorityId)
        {
            return priorityId switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                _ => "Unknown"
            };
        }










        public List<TicketViewModel> GetFilteredTickets(int? status, int? priority, string category)
        {
            List<TicketViewModel> tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetFiltered", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters with DBNull.Value for optional filters
                cmd.Parameters.AddWithValue("@status_code", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@priority_code", (object)priority ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@category_name", string.IsNullOrEmpty(category) ? (object)DBNull.Value : category);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = reader["ticket_id"].ToString(),
                        Subject = reader["subject"].ToString(),
                        Description = reader["description"].ToString(),
                        Category = reader["category"].ToString(),
                        Priority = reader["priority"].ToString(),
                        Status = reader["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["created_at"])
                    });
                }
            }
            return tickets;
        }


        public List<SelectListItem> GetCategoryOptions()
        {
            var categoryOptions = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT category FROM tickets WHERE category IS NOT NULL", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categoryOptions.Add(new SelectListItem
                    {
                        Value = reader["category"].ToString(),
                        Text = reader["category"].ToString()
                    });
                }
            }
            return categoryOptions;
        }

        public List<TicketHistoryViewModel> GetTicketHistory(int ticketId)
        {
            List<TicketHistoryViewModel> history = new List<TicketHistoryViewModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Ticket_GetHistory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ticket_id", ticketId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    history.Add(new TicketHistoryViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TicketId = Convert.ToInt32(reader["ticket_id"]),
                        Status = reader["status"].ToString(),
                        ChangedByUsername = reader["changed_by_username"].ToString(),
                        ChangedAt = Convert.ToDateTime(reader["changed_at"]),
                        Note = reader["note"]?.ToString()
                    });
                }
            }

            return history;
        }







        //Dashboard

        // Method for Admin Dashboard Counts
        //public AdminDashboardViewModel GetAdminDashboardCounts()
        //{
        //    var model = new AdminDashboardViewModel();
        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_Tickets_GetTotalAssignedAndUnassignedCounts", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        con.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();
        //        if (rdr.Read())
        //        {
        //            model.TotalTickets = (int)rdr["TotalTickets"];
        //            model.AssignedTickets = (int)rdr["AssignedTickets"];
        //            model.UnassignedTickets = (int)rdr["UnassignedTickets"];
        //        }
        //    }
        //    return model;
        //}

        // Method to get all assigned tickets for Admin Dashboard
        public List<TicketViewModel> GetAllAssignedTicketsForAdmin()
        {
            var tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetAllAssignedWithSupporterDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = (int)rdr["id"],
                        TicketId = rdr["ticket_id"].ToString(),
                        Subject = rdr["subject"].ToString(),
                        Status = rdr["status"].ToString(),
                        Priority = rdr["priority"].ToString(),
                        CreatedAt = (DateTime)rdr["created_at"],
                        Deadline = rdr["deadline"] as DateTime?,
                        AssignedToName = rdr["AssignedToName"].ToString()
                    });
                }
            }
            return tickets;
        }











        public List<TicketViewModel> GetTicketsBySupporterId(long supporterId)
        {
            var tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetBySupporterId", con); // ✅ fixed
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@supporter_id", supporterId); // ✅ correct param name

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = (int)rdr["id"],
                        TicketId = rdr["ticket_id"].ToString(),
                        Subject = rdr["subject"].ToString(),
                        Description = rdr["description"].ToString(),
                        Category = rdr["category"].ToString(),
                        Priority = rdr["priority"].ToString(),
                        Status = rdr["status"].ToString(),
                        CreatedAt = (DateTime)rdr["created_at"],
                        Deadline = rdr["deadline"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["deadline"],
                        AssignedToName = rdr["AssignedToName"].ToString()
                    });
                }
            }
            return tickets;
        }








        //Method for Customer Dashboard(You already have this SP)
        public List<TicketViewModel> GetTicketsByCustomerId(long customerId)
        {
            var tickets = new List<TicketViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Tickets_GetByUserId", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@customer_id", customerId);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    tickets.Add(new TicketViewModel
                    {
                        Id = (int)rdr["id"],
                        TicketId = rdr["ticket_id"].ToString(),
                        Subject = rdr["subject"].ToString(),
                        Description = rdr["description"].ToString(),
                        Category = rdr["category"].ToString(),
                        Priority = rdr["priority"].ToString(),
                        Status = rdr["status"].ToString(),
                        CreatedAt = (DateTime)rdr["created_at"]
                    });
                }
            }

            return tickets;



        }
    }
}









