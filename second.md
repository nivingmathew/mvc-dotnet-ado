# Full-Stack ASP.NET Core MVC CRUD Application with Entity Framework Core (EF Core)

This comprehensive guide contains all project files and step-by-step instructions to build the exact same **ASP.NET Core MVC** application using **Entity Framework Core** instead of raw ADO.NET. 

---

## Step 1: Database Architecture Setup
If you already ran this for the ADO.NET version, you can skip this step. Otherwise, open **SQL Server Management Studio (SSMS)**, connect to your server instance (`ACTS04\SQLEXP2016`), and execute:

```sql
CREATE DATABASE EmployeeDB;
GO

USE EmployeeDB;
GO

CREATE TABLE Employee
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Salary DECIMAL(18,2) NOT NULL
);
GO

-- Seed Data
INSERT INTO Employee(Name, Email, Salary)
VALUES
('John', 'john@gmail.com', 50000.00),
('David', 'david@gmail.com', 60000.00);
GO
```

---

## Step 2: Project Creation
In Visual Studio:
1. Select **Create a new project**.
2. Select **ASP.NET Core Web App (Model-View-Controller)**.
3. Target **.NET 6 / 7 / 8**.
4. Name the project `WebApplication1`.

---

## Step 3: Install Entity Framework Core Packages
Instead of installing raw SQL clients, you need the EF Core providers. Open the **NuGet Package Manager Console** (Tools -> NuGet Package Manager -> Package Manager Console) and run:

```text
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
```

---

## Step 4: Application Configuration (`appsettings.json`)
Keep your connection string identical to before. EF Core will read the exact same `DBCS` configuration key.

```json
{
  "ConnectionStrings": {
    "DBCS": "Server=ACTS04\SQLEXP2016;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## Step 5: Domain Models (`Models/`)

### 1. Employee Model (`Models/Employee.cs`)
The domain model remains exactly the same. EF Core automatically maps properties to database columns based on standard naming conventions (e.g., `Id` automatically becomes the Primary Key identity).

```csharp
namespace WebApplication1.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public decimal Salary { get; set; }
    }
}
```

### 2. Error View Model (`Models/ErrorViewModel.cs`)
```csharp
namespace WebApplication1.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
```

---

## Step 6: Create the Database Context (`Models/AppDbContext.cs`)
Instead of a manual repository class executing raw SQL strings, EF Core uses a `DbContext` class. This class acts as the bridge between your C# code and the database.

```csharp
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        // Constructor passes configuration settings down to the base EF DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet represents the Employee table in the database
        public DbSet<Employee> Employees { get; set; }
    }
}
```

---

## Step 7: Register EF Core DbContext in the Middleware Container (`Program.cs`)
Open `Program.cs`. Instead of injecting a manual repository using `AddScoped`, we register `AppDbContext` and bind it directly to our connection string (`DBCS`).

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Entity Framework Core DbContext with SQL Server configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBCS")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map default route straight to the Employee Controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();
```

---

## Step 8: App Controllers (`Controllers/`)

### 1. Employee Controller (`Controllers/EmployeeController.cs`)
Notice how clean the CRUD actions become. EF Core eliminates the need for `SqlConnection`, `SqlCommand`, opening connections manually, or iterating through a `SqlDataReader`.

```csharp
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        // Inject the AppDbContext instance via constructor dependency injection
        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // READ (ALL)
        public IActionResult Index()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            _context.Employees.Add(emp); // Generates INSERT SQL implicitly
            _context.SaveChanges();      // Executes transaction against DB
            return RedirectToAction("Index");
        }

        // GET: Edit/5
        public IActionResult Edit(int id)
        {
            var emp = _context.Employees.Find(id); // Generates SELECT WHERE Id = id implicitly
            if (emp == null)
            {
                return NotFound();
            }
            return View(emp);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            _context.Employees.Update(emp); // Generates UPDATE SQL implicitly
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Delete/5
        public IActionResult Delete(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null)
            {
                return NotFound();
            }
            return View(emp);
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(Employee emp)
        {
            _context.Employees.Remove(emp); // Generates DELETE SQL implicitly
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
```

### 2. Home Controller (`Controllers/HomeController.cs`)
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
```

---

## Step 9: View Configuration Files (`Views/`)

### 1. View Imports Configuration (`Views/_ViewImports.cshtml`)
```html
@using WebApplication1
@using WebApplication1.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### 2. View Layout Initialization (`Views/_ViewStart.cshtml`)
```html
@{
    Layout = "_Layout";
}
```

---

## Step 10: Complete CRUD Views Subdirectory (`Views/Employee/`)
*Note: Because our design cleanly isolates backend logic from visual presentation, the frontend Razor layout code stays **100% identical** to the ADO.NET version.*

### 1. Main Table Interface (`Views/Employee/Index.cshtml`)
```html
@model IEnumerable<WebApplication1.Models.Employee>

<h2>Employees</h2>

<a asp-action="Create">Create New</a>

<table border="1">
    <tr>
        <th>Id</th>
        <th>Name</th>
        <th>Email</th>
        <th>Salary</th>
        <th>Action</th>
    </tr>

@foreach(var item in Model)
{
    <tr>
        <td>@item.Id</td>
        <td>@item.Name</td>
        <td>@item.Email</td>
        <td>@item.Salary</td>
        <td>
            <a asp-action="Edit" asp-route-id="@item.Id">Edit</a>
            |
            <a asp-action="Delete" asp-route-id="@item.Id">Delete</a>
        </td>
    </tr>
}
</table>
```

### 2. Creation Form View (`Views/Employee/Create.cshtml`)
```html
@model WebApplication1.Models.Employee

<form asp-action="Create" method="post">

    <p>Name</p>
    <input asp-for="Name" />

    <p>Email</p>
    <input asp-for="Email" />

    <p>Salary</p>
    <input asp-for="Salary" />

    <br /><br />

    <button type="submit">Save</button>

</form>
```

### 3. Modification Form View (`Views/Employee/Edit.cshtml`)
```html
@model WebApplication1.Models.Employee

<form asp-action="Edit" method="post">

    <input type="hidden" asp-for="Id" />

    <p>Name</p>
    <input asp-for="Name" />

    <p>Email</p>
    <input asp-for="Email" />

    <p>Salary</p>
    <input asp-for="Salary" />

    <br /><br />

    <button type="submit">Update</button>

</form>
```

### 4. Removal Confirmation View (`Views/Employee/Delete.cshtml`)
```html
@model WebApplication1.Models.Employee

<form asp-action="Delete" method="post">

    <input type="hidden" asp-for="Id" />

    <h3>Delete @Model.Name ?</h3>

    <button type="submit">Delete</button>

</form>
```

---

## Step 11: Build and Execution
1. Press **Ctrl + Shift + B** to build the application workspace.
2. Press **Ctrl + F5** to start the application server context without activating attached debugging utilities.
3. The application will leverage Entity Framework Core to query database resources instantly and return the tabular structure natively inside your web browser.
