using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace AIZ_MVP_Data.Context
{
    public class AIZMvpDbContext : DbContext
    {
        public AIZMvpDbContext(DbContextOptions<AIZMvpDbContext> options)
        : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<License> Licenses => Set<License>();

        public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
        public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
        public DbSet<InterviewTurn> InterviewTurns => Set<InterviewTurn>();
        public DbSet<InterviewAnswer> InterviewAnswers => Set<InterviewAnswer>();
        public DbSet<InterviewEvaluation> InterviewEvaluations => Set<InterviewEvaluation>();
        public DbSet<InterviewReport> InterviewReports => Set<InterviewReport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // REFRESH TOKEN
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // INTERVIEW SESSION
            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.User)
                .WithMany(u => u.InterviewSessions)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.JobDescription)
                .WithMany()
                .HasForeignKey(s => s.JobDescriptionId);

            // INTERVIEW TURN
            modelBuilder.Entity<InterviewTurn>()
                .HasOne(t => t.InterviewSession)
                .WithMany(s => s.Turns)
                .HasForeignKey(t => t.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // INTERVIEW ANSWER
            modelBuilder.Entity<InterviewAnswer>()
                .HasOne(a => a.InterviewTurn)
                .WithOne(t => t.Answer) // Chỉ định rõ thuộc tính Answer trong InterviewTurn
                .HasForeignKey<InterviewAnswer>(a => a.InterviewTurnId)
                .OnDelete(DeleteBehavior.Cascade);

            // INTERVIEW EVALUATION
            modelBuilder.Entity<InterviewEvaluation>()
                .HasOne(e => e.InterviewAnswer)
                .WithOne(a => a.Evaluation) // Chỉ định rõ thuộc tính Evaluation trong InterviewAnswer
                .HasForeignKey<InterviewEvaluation>(e => e.InterviewAnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // INTERVIEW REPORT
            modelBuilder.Entity<InterviewReport>()
                .HasOne(r => r.InterviewSession)
                .WithOne()
                .HasForeignKey<InterviewReport>(r => r.InterviewSessionId);

            // INDEX: 
            modelBuilder.Entity<InterviewTurn>()
                .HasIndex(t => new { t.InterviewSessionId, t.TurnIndex }) //đảm bảo mỗi TurnIndex là duy nhất trong 1 Session
                .IsUnique();
            modelBuilder.Entity<InterviewSession>()
                .HasIndex(s => s.UserId);

            modelBuilder.Entity<InterviewTurn>()
                .HasIndex(t => t.InterviewSessionId);

            modelBuilder.Entity<InterviewAnswer>()
                .HasIndex(a => a.InterviewTurnId);
            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(t => t.TransactionCode)
                .IsUnique();

            // DECIMAL PRECISION CONFIGURATION
            // InterviewEvaluation.Score: 0-100 with 2 decimal places (e.g., 85.50)
            modelBuilder.Entity<InterviewEvaluation>()
                .Property(e => e.Score)
                .HasPrecision(5, 2);

            // InterviewReport.OverallScore: 0-100 with 2 decimal places
            modelBuilder.Entity<InterviewReport>()
                .Property(r => r.OverallScore)
                .HasPrecision(5, 2);

            // PaymentTransaction: Money fields with 2 decimal places
            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.Discount)
                .HasPrecision(18, 2);
        }
    }
}