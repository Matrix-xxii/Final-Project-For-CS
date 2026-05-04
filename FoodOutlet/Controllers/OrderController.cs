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
            return View(orders);
        }

        [HttpPost("api/order/update_status")]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest req)
        {
            if (req == null)
                return BadRequest(new { success = false, message = "Invalid request." });

            // Verify the role is allowed to make this transition
            if (!AllowedTransitions.TryGetValue(Role, out var byStatus))
                return StatusCode(403, new { success = false, message = "Not authorised." });

            // Find the order's current status first
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

        public class UpdateStatusRequest
        {
            public int    order_id   { get; set; }
            public string new_status { get; set; } = "";
        }
    }
}
