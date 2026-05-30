Since you're using **ASP.NET Core MVC** (the newer version), here's a complete setup from scratch using:

* ASP.NET Core MVC (.NET 6/7/8)
* ADO.NET
* SQL Server Express
* Your SQL Server instance: `ACTS04\SQLEXP2016`

---

# Step 1: Create Database

Open **SQL Server Management Studio (SSMS)** and connect to:

```text
ACTS04\SQLEXP2016
```

Run:

```sql
CREATE DATABASE EmployeeDB;
GO

USE EmployeeDB;
GO

CREATE TABLE Employee
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100),
    Salary DECIMAL(18,2)
);
GO

INSERT INTO Employee(Name, Email, Salary)
VALUES
('John', 'john@gmail.com', 50000),
('David', 'david@gmail.com', 60000);
```

---

# Step 2: Create ASP.NET Core MVC Project

In Visual Studio:

```text
Create New Project
→ ASP.NET Core Web App (Model-View-Controller)
→ Create
```

---

# Step 3: Install SQL Client Package

Right-click project → Manage NuGet Packages

Install:

```text
Microsoft.Data.SqlClient
```

---

# Step 4: Configure appsettings.json

Open `appsettings.json`

Replace with:

```json
{
  "ConnectionStrings": {
    "DBCS": "Server=ACTS04\\SQLEXP2016;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True"
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

# Step 5: Create Model

Create:

```text
Models
 └─ Employee.cs
```

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

---

# Step 6: Create Repository

Create:

```text
Models
 └─ EmployeeRepository.cs
```

```csharp
using Microsoft.Data.SqlClient;

namespace WebApplication1.Models
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DBCS");
        }

        // READ
        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query = "SELECT * FROM Employee";

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                employees.Add(new Employee
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    Name = dr["Name"].ToString(),
                    Email = dr["Email"].ToString(),
                    Salary = Convert.ToDecimal(dr["Salary"])
                });
            }

            return employees;
        }

        // INSERT
        public void AddEmployee(Employee emp)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query =
                @"INSERT INTO Employee(Name,Email,Salary)
                  VALUES(@Name,@Email,@Salary)";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // GET BY ID
        public Employee GetEmployeeById(int id)
        {
            Employee emp = new Employee();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            SqlCommand cmd =
                new SqlCommand(
                    "SELECT * FROM Employee WHERE Id=@Id", con);

            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                emp.Id = Convert.ToInt32(dr["Id"]);
                emp.Name = dr["Name"].ToString();
                emp.Email = dr["Email"].ToString();
                emp.Salary = Convert.ToDecimal(dr["Salary"]);
            }

            return emp;
        }

        // UPDATE
        public void UpdateEmployee(Employee emp)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query =
                @"UPDATE Employee
                  SET Name=@Name,
                      Email=@Email,
                      Salary=@Salary
                  WHERE Id=@Id";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Id", emp.Id);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // DELETE
        public void DeleteEmployee(int id)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            SqlCommand cmd =
                new SqlCommand(
                    "DELETE FROM Employee WHERE Id=@Id", con);

            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
```

---

# Step 7: Register Repository

Open `Program.cs`

Add:

```csharp
builder.Services.AddScoped<EmployeeRepository>();
```

Example:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<EmployeeRepository>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();
```

---

# Step 8: Create Controller

Create:

```text
Controllers
 └─ EmployeeController.cs
```

```csharp
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository _repo;

        public EmployeeController(EmployeeRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetEmployees());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            _repo.AddEmployee(emp);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            return View(_repo.GetEmployeeById(id));
        }

        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            _repo.UpdateEmployee(emp);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            return View(_repo.GetEmployeeById(id));
        }

        [HttpPost]
        public IActionResult Delete(Employee emp)
        {
            _repo.DeleteEmployee(emp.Id);
            return RedirectToAction("Index");
        }
    }
}
```

---

# Step 9: Create Views Folder

Create:

```text
Views
 └─ Employee
```

---

# Index.cshtml

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

---

# Create.cshtml

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

---

# Edit.cshtml

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

---

# Delete.cshtml

```html
@model WebApplication1.Models.Employee

<form asp-action="Delete" method="post">

    <input type="hidden" asp-for="Id" />

    <h3>Delete @Model.Name ?</h3>

    <button type="submit">Delete</button>

</form>
```

---

# Step 10: Run

Press:

```text
Ctrl + F5
```

You should see:

```text
Employees

Id  Name   Email            Salary
1   John   john@gmail.com   50000
2   David  david@gmail.com  60000

Create New
```

and all four operations will work:

* Create
* Read
* Update
* Delete

If you get any compile error while entering the code, paste the exact error and the file name, and I'll help you fix it.
