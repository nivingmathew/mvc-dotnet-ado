## Step 2: Project Creation
In Visual Studio, initialize a new web application using the standard MVC template:
 1. Select **Create a new project**.
 2. Search for and select **ASP.NET Core Web App (Model-View-Controller)**.
 3. Target your preferred runtime (e.g., **.NET 6 / 7 / 8**).
 4. Name the project WebApplication1.
## Step 3: Install Required Dependencies
To interact with SQL Server using ADO.NET, you must install the native SQL Client package. Open the **NuGet Package Manager Console** or **Manage NuGet Packages** UI and install:
```text
Microsoft.Data.SqlClient

```
## Step 4: Application Configuration (appsettings.json)
Update your appsettings.json file in the root directory to store your database connection string safely. It includes directives for Trusted_Connection and TrustServerCertificate to work with local development servers.
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
## Step 5: Domain Models (Models/)
### 1. Employee Model (Models/Employee.cs)
Defines the structure of the data object flowing between your views, controller, and repository layers.
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
### 2. Error View Model (Models/ErrorViewModel.cs)
Standard diagnostic model for handling error contexts across endpoints.
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
## Step 6: Data Access Layer (Models/EmployeeRepository.cs)
This class contains raw ADO.NET queries to perform direct CRUD (Create, Read, Update, Delete) operations with the target database engine safely via **parameterized commands**.
```csharp
using Microsoft.Data.SqlClient;

namespace WebApplication1.Models
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DBCS");
        }

        // READ (ALL EMPLOYEES)
        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new();

            using SqlConnection con = new SqlConnection(_connectionString);
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

        // INSERT (CREATE NEW EMPLOYEE)
        public void AddEmployee(Employee emp)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = @"INSERT INTO Employee(Name, Email, Salary)
                            VALUES(@Name, @Email, @Salary)";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // GET BY ID (READ SPECIFIC EMPLOYEE)
        public Employee GetEmployeeById(int id)
        {
            Employee emp = new Employee();

            using SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM Employee WHERE Id=@Id", con);
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

        // UPDATE (MODIFY EXISTING RECORD)
        public void UpdateEmployee(Employee emp)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = @"UPDATE Employee
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

        // DELETE (REMOVE RECORD)
        public void DeleteEmployee(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand("DELETE FROM Employee WHERE Id=@Id", con);
            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}

```
## Step 7: Application Registration & Middleware Pipeline (Program.cs)
Configures fundamental services including standard MVC controllers with views, sets up the Scoped lifetime dependency injection for EmployeeRepository, and charts default application routing patterns.
```csharp
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the repository for Constructor Dependency Injection
builder.Services.AddScoped<EmployeeRepository>();

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

// Route map directing default traffic straight to Employee Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();

```
## Step 8: App Controllers (Controllers/)
### 1. Employee Controller (Controllers/EmployeeController.cs)
Acts as the intermediary router that accepts client requests, invokes specific ADO.NET tasks from the injected EmployeeRepository, and matches responses back to target Razor views.
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

        // GET: Employee
        public IActionResult Index()
        {
            return View(_repo.GetEmployees());
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            _repo.AddEmployee(emp);
            return RedirectToAction("Index");
        }

        // GET: Employee/Edit/5
        public IActionResult Edit(int id)
        {
            return View(_repo.GetEmployeeById(id));
        }

        // POST: Employee/Edit
        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            _repo.UpdateEmployee(emp);
            return RedirectToAction("Index");
        }

        // GET: Employee/Delete/5
        public IActionResult Delete(int id)
        {
            return View(_repo.GetEmployeeById(id));
        }

        // POST: Employee/Delete
        [HttpPost]
        public IActionResult Delete(Employee emp)
        {
            _repo.DeleteEmployee(emp.Id);
            return RedirectToAction("Index");
        }
    }
}

```
### 2. Home Controller (Controllers/HomeController.cs)
Standard core fallback routing controller handling global static responses and framework execution errors.
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
## Step 9: View Configuration Files (Views/)
### 1. View Imports Configuration (Views/_ViewImports.cshtml)
Enables global namespaces and incorporates the built-in ASP.NET Tag Helpers architecture so they become seamlessly operational in custom user interfaces.
```html
@using WebApplication1
@using WebApplication1.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

```
### 2. View Layout Initialization (Views/_ViewStart.cshtml)
Specifies the global UI container template used uniformly by default for all nested view components.
```html
@{
    Layout = "_Layout";
}

```
## Step 10: Complete CRUD Views Subdirectory (Views/Employee/)
### 1. Main Table Interface (Views/Employee/Index.cshtml)
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
### 2. Creation Form View (Views/Employee/Create.cshtml)
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
### 3. Modification Form View (Views/Employee/Edit.cshtml)
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
### 4. Removal Confirmation View (Views/Employee/Delete.cshtml)
```html
@model WebApplication1.Models.Employee

<form asp-action="Delete" method="post">

    <input type="hidden" asp-for="Id" />

    <h3>Delete @Model.Name ?</h3>

    <button type="submit">Delete</button>

</form>

```
## Step 11: Build and Execution
 1. Ensure your local SQL Server instance is active and database tables are correctly initialized.
 2. Build the project assembly using **Ctrl + Shift + B**.
 3. Launch the web program application without code debugging context by pressing **Ctrl + F5**.
 4. The router default rules will trigger the EmployeeController.Index() endpoint instantly, loading your tabular interface complete with the data entries seeded in Step 1.
   """
# Save contents to a markdown file
output_file_name = "Complete_ASP_NET_Core_MVC_ADO_NET_CRUD_Guide.md"
with open(output_file_name, "w") as f:
f.write(markdown_content)
print(f"Successfully generated {output_file_name}")
```
Your MD file is ready
[file-tag: code-generated-file-0-1780142694613969263]

I have consolidated all your uploaded configuration objects, database installation components, repository connection logic, controller bindings, layout dependencies, and custom rendering forms into a single comprehensive implementation document (`Complete_ASP_NET_Core_MVC_ADO_NET_CRUD_Guide.md`). 

The file includes:
* **Sequential Phase Grouping:** Organizes structural setup logically, starting with SQL server schema definition and moving towards frontend implementation layers.
* **Full Integration of Snippets:** Combines isolated files like `Program.cs`, `_ViewImports.cshtml`, `_ViewStart.cshtml`, and `HomeController.cs` alongside the raw CRUD steps.
* **Production Readability:** Features clear code formatting wrappers, ensuring you can review or paste code snippets directly into Visual Studio.

```
