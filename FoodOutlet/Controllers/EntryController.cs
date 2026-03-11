using FoodOutlet.AppCode;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FoodOutlet.Controllers
{
    public class EntryController : Controller
    {
        private readonly Staff _staff;
        private readonly IWebHostEnvironment _env;

        public EntryController(Staff staff, IWebHostEnvironment env)
        {
            _staff = staff;
            _env = env;
        }

        #region view
        public IActionResult Inventory()
        {
            return View();
        }
        public IActionResult Role()
        {
            return View();
        }

        public IActionResult Registration(int? id)
        {
            if (id.HasValue)
            {
                var staff = _staff.GetStaffById(id.Value);
                ViewData["Title"] = staff?.name ?? "Registration";
                return View(staff);
            }
            ViewData["Title"] = "Registration";
            return View();
        }

        public IActionResult StaffList()
        {
            return View();
        }

        public IActionResult StaffResignRecords()
        {
            return View();
        }

        public IActionResult Category()
        {
            return View();
        }

        public IActionResult Recipe(int? id)
        {
            // load categories for server-side rendering as a fallback
            ViewData["Categories"] = _staff.GetAllCategories();

            if (id.HasValue)
            {
                var recipe = _staff.GetRecipeById(id.Value);
                ViewData["Title"] = recipe?.recipe_name ?? "Recipe";
                return View(recipe);
            }

            ViewData["Title"] = "Recipe";
            return View();
        }

        public IActionResult RecipeList()
        {
            // returns Views/Entry/RecipeList.cshtml
            return View();
        }
        #endregion

        #region Staff

        [HttpPost("api/set_staff")]
        public Models.Message SetStaff([FromBody] Models.Staff staff)
        {
            // if id present update, otherwise insert
            if (staff.id > 0)
                return _staff.UpdateStaff(staff);
            else
                return _staff.SetStaff(staff);
        }

        [HttpPost("api/delete_staff")]
        public Models.Message DeleteStaff([FromBody] dynamic payload)
        {
            int id = payload.id;
            return _staff.DeleteStaff(id);
        }

        #endregion

        #region Debug

        // temporary debugging helper: returns column names in registrations table
        [HttpGet("api/registration_columns")]
        public Dictionary<string, dynamic> GetRegistrationColumns()
        {
            var cols = _staff.GetRegistrationColumns();
            return new Dictionary<string, dynamic> { { "columns", cols } };
        }

        #endregion

        #region Role

        [HttpGet("api/get_all_roles")]
        public Dictionary<string, dynamic> GetAllRoles()
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();

            result.Add("roles", _staff.GetAllRoles());

            return result;
        }

        [HttpGet("api/get_all_staffs")]
        public Dictionary<string, dynamic> GetAllStaffs()
        {
            return new Dictionary<string, dynamic> { { "staff", _staff.GetAllStaffs() } };      
        }

        [HttpGet("api/get_all_categories")]
        public Dictionary<string, dynamic> GetAllCategories()
        {
            return new Dictionary<string, dynamic> { { "categories", _staff.GetAllCategories() } };
        }

        [HttpPost("api/set_category")]
        public Models.Message SetCategory([FromBody] Models.Category cat)
        {
            return _staff.SetCategory(cat);
        }

        [HttpPost("api/delete_category")]
        public Models.Message DeleteCategory([FromBody] dynamic payload)
        {
            int id = payload.id;
            return _staff.DeleteCategory(id);
        }

        [HttpGet("api/get_all_recipes")]
        public Dictionary<string, dynamic> GetAllRecipes()
        {
            return new Dictionary<string, dynamic> { { "recipes", _staff.GetAllRecipes() } };
        }

        [HttpPost("api/set_recipe")]
        public Models.Message SetRecipe([FromBody] Models.Recipe r)
        {
            return _staff.SetRecipe(r);
        }

        [HttpPost("api/delete_recipe")]
        public Models.Message DeleteRecipe([FromBody] dynamic payload)
        {
            int id = payload.id;
            return _staff.DeleteRecipe(id);
        }

        [HttpPost("api/upload_recipe_image")]
        public IActionResult UploadRecipeImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "img", "recipes");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var relative = $"/img/recipes/{fileName}";
            return Ok(new { imageUrl = relative });
        }

        // debugging helper for resign table: name, columns, count, raw rows
        [HttpGet("api/resign_debug")]
        public Dictionary<string, dynamic> ResignDebug()
        {
            return new Dictionary<string, dynamic>
            {
                { "table", _staff.GetResignTableName() },
                { "columns", _staff.GetResignColumns() },
                { "count", _staff.GetResignCount() },
                { "rows", _staff.GetRawResigns() }
            };
        }

        // an alias endpoint matching the new service method name
        [HttpGet("api/get_resigned_staff")]
        public Dictionary<string, dynamic> GetResignedStaff()
        {
            return new Dictionary<string, dynamic> { { "records", _staff.GetResignedStaffs() } };
        }

        [HttpPost("api/set_role")]
        public Models.Message SetRole([FromBody] Models.Role role)
        {
            return _staff.SetRole(role);
        }

        [HttpPost("api/delete_role")]
        public Models.Message DeleteRole([FromBody] dynamic payload)
        {
            int id = payload.id;
            return _staff.DeleteRole(id);
        }

        #endregion

        #region Inventory

        #endregion

        // recipe debug endpoint
        [HttpGet("api/recipe_debug")]
        public Dictionary<string, dynamic> RecipeDebug()
        {
            return new Dictionary<string, dynamic>
            {
                { "table", _staff.GetRecipeTableName() },
                { "columns", _staff.GetRecipeColumns() },
                { "count", _staff.GetRecipeCount() }
            };
        }

    }
}
