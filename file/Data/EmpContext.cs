using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Emp.Models;

namespace Emp.Data
{
    public class EmpContext : DbContext
    {
        public EmpContext (DbContextOptions<EmpContext> options)
            : base(options)
        {
        }

        public DbSet<Emp.Models.Employee> Employee { get; set; } = default!;
    }
}
