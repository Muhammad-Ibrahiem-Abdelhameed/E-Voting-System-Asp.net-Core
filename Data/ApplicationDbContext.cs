using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EVotingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EVotingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        //public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Voter> Voters { get; set; }

        public DbSet<Vote> Votes { get; set; }
        public DbSet<SupervisorsVotes> SupervisorsVotes { get; set; }

        public DbSet<State> States { get; set; }        
        public DbSet<Candidate> Candidates { get; set; }        
        
        public DbSet<VoterVotes> VoterVotes { get; set; }
        public DbSet<Result> Results { get; set; }

        //public DbSet<UserJwtToken> UserJwtTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(au => au.PhoneNumber )
                .IsUnique();            
                
            modelBuilder.Entity<Voter>()
                .HasIndex(v => v.NationalId)
                .IsUnique();

            modelBuilder.Entity<Voter>()
                .HasOne(v => v.ApplicationUser);

            modelBuilder.Entity<Candidate>()
                .HasIndex(c => new { c.VoteId, c.ListNumber })
                .IsUnique();

            modelBuilder.Entity<SupervisorsVotes>()
                .HasKey(c => new { c.SupervisorId, c.VoteId });

            modelBuilder.Entity<VoterVotes>()
                .HasIndex(uv => new { uv.Id, uv.VoteId })
                .IsUnique();

            modelBuilder.Entity<Vote>()
                .HasIndex(v => v.Title)
                .IsUnique();
           

            /*modelBuilder.Entity<IdentityRole>()
                .HasData(new { Id = "1", Name = "Admin", });*/

            /*modelBuilder.Entity<Area>()
                .HasData(
                new { Id = 1, Name = "El Gharbia" },
                new { Id = 2, Name = "El Beheira" });*/


            /* modelBuilder.Entity<UserJwtToken>()
                 .HasKey(u => new {  u.ApplicationUserId, u.JwtToken });

             modelBuilder.Entity<Person>()
                 .HasIndex(p => new { p.FirstName, p.LastName });*/

        }        

        //public DbSet<UserJwtToken> UserJwtTokens { get; set; }
        

        


    }
}
