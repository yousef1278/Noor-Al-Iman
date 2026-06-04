using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Models;

namespace QuranCenterApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ZipCode>          ZipCodes          { get; set; }
    public DbSet<Person>           Persons            { get; set; }
    public DbSet<PhoneNumber>      PhoneNumbers       { get; set; }
    public DbSet<Student>          Students           { get; set; }
    public DbSet<Teacher>          Teachers           { get; set; }
    public DbSet<Supervisor>       Supervisors        { get; set; }
    public DbSet<Curriculum>       Curricula          { get; set; }
    public DbSet<Classroom>        Classrooms         { get; set; }
    public DbSet<Enrollment>       Enrollments        { get; set; }
    public DbSet<Attendance>       Attendances        { get; set; }
    public DbSet<Memorization>     Memorizations      { get; set; }
    public DbSet<Gift>             Gifts              { get; set; }
    public DbSet<GiftDistribution> GiftDistributions  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map table names to match the SQL script (singular names)
        modelBuilder.Entity<Person>()      .ToTable("Person");
        modelBuilder.Entity<Student>()     .ToTable("Student");
        modelBuilder.Entity<Teacher>()     .ToTable("Teacher");
        modelBuilder.Entity<Supervisor>()  .ToTable("Supervisor");
        modelBuilder.Entity<Curriculum>()  .ToTable("Curriculum");
        modelBuilder.Entity<Classroom>()   .ToTable("Classroom");
        // Tables with triggers: disable OUTPUT clause so EF Core doesn't conflict
        modelBuilder.Entity<Enrollment>()
            .ToTable("Enrollment", t => t.UseSqlOutputClause(false));
        modelBuilder.Entity<GiftDistribution>()
            .ToTable("GiftDistribution", t => t.UseSqlOutputClause(false));

        modelBuilder.Entity<Attendance>()  .ToTable("Attendance");
        modelBuilder.Entity<Memorization>().ToTable("Memorization");
        modelBuilder.Entity<Gift>()        .ToTable("Gift");
        modelBuilder.Entity<PhoneNumber>() .ToTable("PhoneNumbers");

        // ZipCode mapping
        modelBuilder.Entity<ZipCode>()
            .ToTable("ZipCodes")
            .HasKey(z => z.ZipCodeValue);

        modelBuilder.Entity<ZipCode>()
            .Property(z => z.ZipCodeValue)
            .HasColumnName("ZipCode");

        // Person -> ZipCode
        modelBuilder.Entity<Person>()
            .HasOne(p => p.ZipCodeNav)
            .WithMany(z => z.Persons)
            .HasForeignKey(p => p.ZipCode)
            .HasPrincipalKey(z => z.ZipCodeValue);

        // PhoneNumber column name
        modelBuilder.Entity<PhoneNumber>()
            .Property(p => p.Phone)
            .HasColumnName("PhoneNumber");

        // Enrollment unique constraint
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentID, e.ClassroomID })
            .IsUnique();

        // Classroom -> Supervisor (optional)
        modelBuilder.Entity<Classroom>()
            .HasOne(c => c.Supervisor)
            .WithMany(s => s.Classrooms)
            .HasForeignKey(c => c.SupervisorID)
            .IsRequired(false);

        // Attendance Status check
        modelBuilder.Entity<Attendance>()
            .Property(a => a.Status)
            .HasMaxLength(1);

        // Memorization Rating optional
        modelBuilder.Entity<Memorization>()
            .Property(m => m.Rating)
            .IsRequired(false);
    }
}
