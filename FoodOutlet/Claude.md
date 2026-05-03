# FoodOutlet - Developer Onboarding Guide

**Project Status:** Final Project for CS (In Development)

---

## 📋 PROJECT OVERVIEW

**FoodOutlet** is a restaurant management system built with **ASP.NET Core 8.0**. It provides an integrated solution for managing staff, menu items, inventory, and customer orders with QR-based table ordering.

### Project Structure:
- **Admin Dashboard** - Staff, menu, and inventory management (_Layout.cshtml)
- **Customer/User View** - Customer menu and ordering interface (_LayoutWebsite.cshtml)

---

## 🛠️ TECH STACK

| Component | Technology |
|-----------|-----------|
| Backend | ASP.NET Core 8.0 (.NET 8) |
| Database | MySQL 8.0 |
| UI Framework | Bootstrap 4 (AdminLTE 3) |
| Frontend Logic | jQuery |
| Image Processing | SixLabors.ImageSharp 3.1.3 |
| QR Code Generation | QRCoder 1.4.3 |
| ORM/Data Access | MySql.Data 9.5.0 (Direct SQL) |

---

## ✅ COMPLETED FEATURES

### 1. Staff Management (Admin Dashboard)
- ✅ Role Management (CRUD)
- ✅ Staff Registration with photo upload
- ✅ Staff list with filters (name, email, phone, address, role, status)
- ✅ Staff resignation records tracking
- ✅ Password generation service for new staff
- ✅ Photo image processing and storage

### 2. Restaurant Menu Management (Admin Dashboard)
- ✅ Category Management (CRUD)
- ✅ Recipe/Dish Management
  - Create recipe with image upload
  - Set price, description, category
  - View recipe list
- ✅ Inventory Management
  - Track stock quantities per recipe
  - View inventory list

### 3. Table Management
- ✅ Table Registration (create new tables)
- ✅ QR Code Generation (one QR per table)
- ✅ Table List view with QR codes
- ✅ Database schema for table tracking

### 4. Database Layer
- ✅ MySQL connection factory with dependency injection
- ✅ Complete schema with all tables and foreign keys
- ✅ Tables: Roles, Registrations, Resigns, Categories, Recipes, Inventories, Table_Lists, Status, Payment_Methods, Payments, Order, Order_Detail, tables

### 5. Services & Infrastructure
- ✅ IDbConnectionFactory interface
- ✅ MySqlConnectionFactory implementation
- ✅ ImageProcessingService (upload, store, retrieve)
- ✅ PasswordGenerated service
- ✅ Staff service with database queries

### 6. API Endpoints (Implemented)
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | /api/get_all_roles | Fetch all roles |
| GET | /api/get_all_staffs | Fetch all staff members |
| GET | /api/get_all_categories | Fetch all categories |
| GET | /api/get_all_recipes | Fetch all recipes |
| GET | /api/get_all_inventories | Fetch inventory |
| GET | /api/get_all_tables | Fetch all tables |
| GET | /api/get_resigned_staff | Fetch resigned staff records |
| GET | /api/get_counts | Fetch dashboard counts |
| POST | /api/set_staff | Create/Update staff |
| POST | /api/set_staff_with_photo | Create/Update staff with photo |
| POST | /api/set_category | Create/Update category |
| POST | /api/set_recipe | Create/Update recipe |
| POST | /api/set_role | Create/Update role |
| POST | /api/set_inventory | Create/Update inventory |
| POST | /api/delete_staff | Delete staff |
| POST | /api/delete_category | Delete category |
| POST | /api/delete_recipe | Delete recipe |
| POST | /api/delete_role | Delete role |
| POST | /api/delete_inventory | Delete inventory |
| POST | /api/upload_recipe_image | Upload recipe image |

---

## 🚀 IN PROGRESS / TODO FEATURES

### Priority 1 - Core Functionality
- [ ] **Order Management (CRUD)**
  - Create orders from customer menu
  - View order details (table, items, quantity, status)
  - Update order status (pending, cooking, ready, served)
  - Delete/cancel orders
  
- [ ] **Order Detail Management**
  - Track individual items in an order
  - Quantity and pricing per item
  - Status tracking (pending, cooking, ready)

- [ ] **Kitchen Display System**
  - Show pending orders in kitchen
  - Update order status as cooking progresses
  - Mark items as ready

- [ ] **Payment Processing**
  - Select payment method
  - Calculate bill total
  - Update payment status

### Priority 2 - Customer Experience
- [ ] **Customer Menu Interface** (via Table.cshtml)
  - Display categories
  - Display recipes with images and prices
  - Add items to cart
  - Remove items from cart
  - Place order

- [ ] **Order Tracking** (Customer View)
  - View current order status
  - See estimated wait time
  - Receive notifications when order is ready

- [ ] **Customer QR Code Flow**
  - Scan table QR code
  - Automatically associate with table
  - Browse menu specific to that table

### Priority 3 - Admin Features
- [ ] **Dashboard Metrics**
  - Total orders (today/week/month)
  - Revenue analysis
  - Best selling dishes
  - Staff performance

- [ ] **Reports**
  - Sales reports
  - Inventory reports
  - Staff attendance

### Priority 4 - System Features
- [ ] **Authentication & Authorization**
  - Login system for staff
  - Role-based access control
  - Session management

- [ ] **Data Validation**
  - Input validation (server-side)
  - Error handling & logging
  - User feedback messages

- [ ] **Testing**
  - Unit tests
  - Integration tests

---

## 📂 PROJECT STRUCTURE

```
FoodOutlet/
├── AppCode/                    # Database and business logic layer
│   ├── IDbConnectionFactory.cs # Interface for DB connection
│   ├── MySqlConnectionFactory.cs # MySQL implementation
│   ├── Staff.cs               # Database queries for staff/menu/inventory
│   ├── ImageProcessingService.cs
│   └── HtmlHelpers.cs
├── Controllers/               # Request handlers
│   ├── HomeController.cs      # Home page
│   ├── EntryController.cs     # All admin CRUD + APIs + Table QR
├── Models/                    # Data models
│   ├── Staff.cs              # Staff, Role, Resign, Message models
│   ├── Recipe.cs             # Recipe model
│   ├── Category.cs           # Category model
│   ├── Inventory.cs          # Inventory model
│   ├── Table.cs              # Table model
│   └── ErrorViewModel.cs
├── Views/                    # Razor views
│   ├── Home/                 # Customer public views
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Entry/                # Admin dashboard views
│   │   ├── Role.cshtml              # Role management
│   │   ├── Registration.cshtml      # Staff registration form
│   │   ├── StaffList.cshtml         # Staff list view
│   │   ├── StaffResignRecords.cshtml# Resignation records
│   │   ├── Category.cshtml          # Category management
│   │   ├── Recipe.cshtml            # Recipe form
│   │   ├── RecipeList.cshtml        # Recipe list
│   │   ├── Inventory.cshtml         # Inventory management
│   │   ├── TableRegistration.cshtml # Table & QR management
│   │   └── Table.cshtml             # Customer menu (user view)
│   └── Shared/               # Layouts
│       ├── _Layout.cshtml           # Admin dashboard layout
│       ├── _LayoutWebsite.cshtml    # Customer/User layout
│       ├── _Layout.cshtml.css
│       └── _ValidationScriptsPartial.cshtml
├── Properties/
│   └── launchSettings.json
├── Services/
│   └── PasswordGenerated.cs
├── wwwroot/                  # Static files
│   ├── css/                  # AdminLTE CSS
│   ├── js/                   # AdminLTE JS + site.js
│   ├── plugins/              # Bootstrap, DataTables, DatePicker, etc.
│   ├── img/                  # Images
│   ├── tableQR/              # Generated QR codes
│   └── uploads/              # Uploaded staff photos & recipe images
├── schema.sql                # Database schema
├── appsettings.json          # Configuration
├── Program.cs                # App startup & dependency injection
├── FoodOutlet.csproj         # Project file
└── README.md                 # Project overview

```

---

## 🗄️ DATABASE SCHEMA

### Key Tables:
1. **Roles** - Job roles (Manager, Chef, Waiter, etc.)
2. **Registrations** - Staff members with personal info
3. **Resigns** - Records of staff resignations
4. **Categories** - Menu categories (Appetizer, Main, Dessert, etc.)
5. **Recipes** - Dishes with name, price, description, image
6. **Inventories** - Stock quantity per recipe
7. **Table_Lists** - Restaurant tables
8. **tables** - Table details with QR codes
9. **Status** - Kitchen order status (Pending, Cooking, Ready, Served)
10. **Payment_Methods** - Payment options (Cash, Card, etc.)
11. **Payments** - Payment transactions
12. **Order** - Main order records
13. **Order_Detail** - Individual items in an order

---

## 💻 DEVELOPER GUIDELINES

### 1. Coding Conventions
- Keep it **simple** - No complex helper functions; write direct logic
- **No debug code** in AppCode layer
- Use **direct SQL** queries with MySql.Data
- Follow existing patterns in Staff.cs for database access
- Place all scripts in the same .cshtml view file (no separate JS files for views)

### 2. View/Layout Rules
- Use **_Layout.cshtml** for admin dashboard pages
- Use **_LayoutWebsite.cshtml** for customer/user pages
- Each view that needs data retrieval should include:
  - Form for input (if applicable)
  - Display/list section
  - jQuery AJAX script at the bottom

### 3. File Placement Rules
- Models in `/Models/`
- Controllers in `/Controllers/`
- Database queries in `/AppCode/Staff.cs` or extend with new service files
- Views in `/Views/Entry/` (for admin) or `/Views/Home/` (for customer)
- Static files in `/wwwroot/`
- Never modify project structure

### 4. Database Access Pattern
```csharp
// Always use IDbConnectionFactory in constructors
private readonly IDbConnectionFactory _connectionFactory;

// Create and use connections like this:
using (MySqlConnection conn = _connectionFactory.CreateConnection())
{
    conn.Open();
    using (MySqlCommand cmd = new MySqlCommand("SELECT ...", conn))
    {
        // Use cmd.ExecuteReader() or ExecuteScalar() or ExecuteNonQuery()
    }
}
```

### 5. Image Processing
- Use **ImageProcessingService** for photo/recipe image uploads
- Store images in `/wwwroot/uploads/`
- Save path to database

### 6. API Response Format
```json
{
  "roles": [...],     // For list endpoints
  "message": "...",   // For success/error messages
  "data": {...}       // For single object responses
}
```

### 7. Form & Script Pattern (from Role.cshtml example)
```html
<form structure with inputs>
<button onclick="functionName()">Action</button>

<div with table/list to display results>

<script>
  function GetAll() {
    $.ajax({ type:"GET", url:"/api/endpoint" })
      .done(function(data) { /* populate list */ });
  }
  
  function Create() {
    $.ajax({ type:"POST", url:"/api/endpoint", data: {...} })
      .done(function(result) { GetAll(); });
  }
</script>
```

---

## 🔧 HOW TO START

### 1. Setup Database
```sql
-- Run schema.sql in MySQL
-- Update appsettings.json with your MySQL connection string
```

### 2. Install NuGet Packages
- MySql.Data 9.5.0
- QRCoder 1.4.3
- SixLabors.ImageSharp 3.1.3

### 3. Update Connection String
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=foodoutlet;Uid=root;Pwd=your_password;"
  }
}
```

### 4. Run Application
```bash
dotnet run
```

Access at: `https://localhost:5001`

---

## 👤 WHO IS IN CHARGE

**Project Owner/Student:** Computer Science Final Project Student
- Overall architecture and requirements
- Database design
- Feature prioritization

**Developer (You):** 
- Implement new features from TODO list
- Fix bugs and enhance existing features
- Maintain code quality and follow conventions
- Add new API endpoints as needed
- Create new views for new features

---

## 📝 KEY NOTES

1. **Project Structure is Fixed** - Don't reorganize folders
2. **Keep Code Simple** - This is a CS final project, not enterprise-level
3. **All Scripts in Views** - Use jQuery in the same .cshtml file
4. **Two Layouts** - Use correct layout for admin vs customer pages
5. **MySQL Direct** - Use direct SQL with MySql.Data, no ORM
6. **No Helpers for Simple Tasks** - Insert/update/delete should be straightforward SQL

---

## 🎯 NEXT STEPS FOR YOU

1. **Review** the schema.sql to understand data relationships
2. **Study** Staff.cs to see the database access pattern
3. **Look at** existing views (Role.cshtml, StaffList.cshtml) to understand the form+list+script pattern
4. **Start implementing** Order Management (see Priority 1 TODO)
5. **Follow the conventions** outlined in this document

---

## 📞 QUICK REFERENCE

- **App Entry Point:** Program.cs
- **Database Queries:** AppCode/Staff.cs
- **All Admin Pages:** Controllers/EntryController.cs
- **Admin Views:** Views/Entry/
- **Config:** appsettings.json
- **Database Design:** schema.sql
- **Dependencies:** Program.cs (line 1-14)
