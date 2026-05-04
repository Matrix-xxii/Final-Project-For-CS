using FoodOutlet.AppCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOutlet.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly Staff _staff;

        public OrderController(Staff staff)
        {
            _staff = staff;
        }

        private string Role => User.FindFirst("RoleName")?.Value ?? "";

        // Allowed next-statuses per role + current status
        private static readonly Dictionary<string, Dictionary<string, string[]>> AllowedTransitions =
            new()
            {
                ["Cashier"] = new()
                {
                    ["Pending"]  = new[] { "Approved", "Cancelled" },
                    ["Served"]   = new[] { "Cleaning" },
                },
                ["Chief"] = new()
                {
                    ["Approved"] = new[] { "Ready", "Cancelled" },
                },
                ["Waiter"] = new()
                {
                    ["Ready"]    = new[] { "Served", "Cancelled" },
                },
                ["Cleaner"] = new()
                {
                    ["Cleaning"] = new[] { "Done", "Cancelled" },
                },
            };

        // Statuses each role needs to see
        private static readonly Dictionary<string, string[]> RoleStatuses = new()
        {
            ["Cashier"] = new[] { "Pending", "Served" },
            ["Chief"]   = new[] { "Approved" },
            ["Waiter"]  = new[] { "Ready" },
            ["Cleaner"] = new[] { "Cleaning" },
        };

        public IActionResult Index()
        {
            if (!RoleStatuses.TryGetValue(Role, out var statuses))
                return Forbid();

            var orders = _staff.GetOrdersWithItems(statuses);
            ViewData["Role"] = Role;

            // Cashier's Served queue is shown grouped by table (one card per table)
            if (Role == "Cashier")
                ViewData["ServedByTable"] = _staff.GetServedByTable();

            // Cleaner's Cleaning queue is also grouped by table
            if (Role == "Cleaner")
                ViewData["CleaningByTable"] = _staff.GetCleaningByTable();

            return View(orders);
        }

        [HttpPost("api/order/update_status")]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest req)
        {
            if (req == null)
                return BadRequest(new { success = false, message = "Invalid request." });

            if (!AllowedTransitions.TryGetValue(Role, out var byStatus))
                return StatusCode(403, new { success = false, message = "Not authorised." });

            var orders = _staff.GetOrdersWithItems(byStatus.Keys.ToArray());
            var order  = orders.FirstOrDefault(o => (int)o.order_id == req.order_id);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found or not in your queue." });

            string currentStatus = (string)order.status;
            if (!byStatus.TryGetValue(currentStatus, out var allowed) ||
                !allowed.Contains(req.new_status))
            {
                return BadRequest(new { success = false, message = $"Transition '{currentStatus}' → '{req.new_status}' is not allowed." });
            }

            var result = _staff.UpdateOrderStatus(req.order_id, req.new_status);
            bool ok = result.message == "Success";
            return ok
                ? Ok(new { success = true })
                : StatusCode(500, new { success = false, message = result.message });
        }

        /// <summary>
        /// Bulk-moves all Served orders for a table (identified by their IDs) to the next
        /// status.  Used by the cashier's consolidated table card "Send to Clean" button.
        /// </summary>
        [HttpPost("api/order/update_table_status")]
        public IActionResult UpdateTableStatus([FromBody] UpdateTableStatusRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.order_ids) || string.IsNullOrWhiteSpace(req.new_status))
                return BadRequest(new { success = false, message = "Invalid request." });

            // Validate role is allowed to perform bulk transitions
            var bulkAllowed = new Dictionary<string, string[]>
            {
                ["Cashier"]  = new[] { "Cleaning" },
                ["Cleaner"]  = new[] { "Done", "Cancelled" },
            };
            if (!bulkAllowed.TryGetValue(Role, out var allowed) || !allowed.Contains(req.new_status))
                return StatusCode(403, new { success = false, message = "Not authorised for this transition." });

            // Parse the comma-separated IDs sent from the view
            var ids = req.order_ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out int n) ? (int?)n : null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToList();

            if (ids.Count == 0)
                return BadRequest(new { success = false, message = "No valid order IDs." });

            var result = _staff.UpdateMultipleOrderStatus(ids, req.new_status);
            bool ok = result.message == "Success";
            return ok
                ? Ok(new { success = true })
                : StatusCode(500, new { success = false, message = result.message });
        }

        public class UpdateStatusRequest
        {
            public int    order_id   { get; set; }
            public string new_status { get; set; } = "";
        }

        public class UpdateTableStatusRequest
        {
            public string order_ids  { get; set; } = "";
            public string new_status { get; set; } = "";
        }
    }
}
