using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using T3App.Shared;

namespace T3App.API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Employee>().HasData(new Employee { Id = Guid.NewGuid(), Name = "Bart Van Hoey" });
            modelBuilder.Entity<Employee>().HasData(new Employee { Id = Guid.NewGuid(), Name = "Jakob Van Hoey" });
            modelBuilder.Entity<Employee>().HasData(new Employee { Id = Guid.NewGuid(), Name = "Lukas Van Hoey" });
        }

    }
}
