using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Enrollment
{
    [Key]
    public int EnrollmentID { get; set; }

    [Required]
    public int StudentID { get; set; }

    [Required]
    public int ClassroomID { get; set; }

    [DataType(DataType.Date), Display(Name = "Enrollment Date")]
    public DateTime EnrollmentDate { get; set; } = DateTime.Today;

    public Student Student { get; set; } = null!;
    public Classroom Classroom { get; set; } = null!;
}
