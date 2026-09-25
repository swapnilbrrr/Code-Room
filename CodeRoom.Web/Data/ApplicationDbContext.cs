using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Progress> Progress => Set<Progress>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Bio).HasMaxLength(500);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(x => new { x.UserId, x.CourseId })
            .IsUnique();

        modelBuilder.Entity<Progress>()
            .HasIndex(x => new { x.UserId, x.LessonId })
            .IsUnique();

        modelBuilder.Entity<UserActivity>()
            .HasIndex(x => new { x.UserId, x.CreatedAt });

        modelBuilder.Entity<Notification>()
            .HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

        modelBuilder.Entity<Course>()
            .HasMany(x => x.Lessons)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserActivity>()
            .HasOne(x => x.User)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(x => x.User)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
