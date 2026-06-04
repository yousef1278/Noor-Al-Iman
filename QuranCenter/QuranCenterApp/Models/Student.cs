using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenterApp.Models;

public class Student
{
    [Key]
    public int StudentID { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Enrollment Date")]
    public DateTime EnrollmentDate { get; set; } = DateTime.Today;

    [Required, MaxLength(50), Display(Name = "Level")]
    public string Level { get; set; } = "Beginner";

    [ForeignKey(nameof(StudentID))]
    public Person Person { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Memorization> Memorizations { get; set; } = new List<Memorization>();
    public ICollection<GiftDistribution> GiftDistributions { get; set; } = new List<GiftDistribution>();
}
