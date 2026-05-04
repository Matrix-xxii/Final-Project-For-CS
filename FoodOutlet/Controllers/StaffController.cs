using FoodOutlet.AppCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOutlet.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly Staff _staff;

        public StaffController(Staff staff)
        {
            _staff = staff;
        }

        private int GetStaffId()
        {
            var raw = User.FindFirst("StaffId")?.Value;
            return int.TryParse(raw, out var id) ? id : 0;
        }

        public IActionResult Status()
        {
            var staffId = GetStaffId();
            var model = _staff.GetMyStatus(staffId);
            return View(model);
        }

        [HttpGet]
        public IActionResult ResignForm()
        {
            var staffId = GetStaffId();
            ViewData["AlreadyResigned"] = _staff.HasResigned(staffId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResignForm(string reason)
        {
            var staffId = GetStaffId();

            if (string.IsNullOrWhiteSpace(reason))
            {
                ViewData["AlreadyResigned"] = false;
                ViewData["Error"] = "Please provide a reason for resignation.";
                return View();
            }

            var result = _staff.SubmitResign(staffId, reason.Trim());

            if (result.message == "Success")
            {
                TempData["Success"] = "Your resignation has been submitted successfully.";
                return RedirectToAction(nameof(ResignForm));
            }

            ViewData["AlreadyResigned"] = result.message.Contains("already");
            ViewData["Error"] = result.message.Replace("Error: ", "");
            return View();
        }
    }
}
