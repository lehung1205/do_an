namespace JobPortal.API.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobApplication> JobApplications { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
    }

