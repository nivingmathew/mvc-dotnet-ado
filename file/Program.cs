using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Emp.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EmpContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmpContext") ?? throw new InvalidOperationException("Connection string 'EmpContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
