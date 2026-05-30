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