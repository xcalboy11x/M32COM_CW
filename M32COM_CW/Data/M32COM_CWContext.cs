using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using M32COM_CW.Models;
using Microsoft.AspNetCore.Identity;

namespace M32COM_CW.Models
{
    public class M32COM_CWContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<M32COM_CW.Models.Event> Event { get; set; }

        public DbSet<M32COM_CW.Models.Member> Member { get; set; }

        public DbSet<M32COM_CW.Models.Venue> Venue { get; set; }

        public DbSet<M32COM_CW.Models.Team> Team { get; set; }

        public DbSet<M32COM_CW.Models.Boat> Boat { get; set; }

        public DbSet<M32COM_CW.Models.Entry> Entry { get; set; }

        public M32COM_CWContext(DbContextOptions<M32COM_CWContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Team)
                .WithMany(t => t.Members)
                .HasForeignKey("TeamID");

            modelBuilder.Entity<Team>()
               .HasMany(t => t.Members)
               .WithOne(m => m.Team)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Boat)
                .WithOne(b => b.Team)
                .OnDelete(DeleteBehavior.Cascade);


        }




    }
}
