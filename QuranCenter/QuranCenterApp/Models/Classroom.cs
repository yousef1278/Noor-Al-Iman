using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Classroom
{
    [Key]
    public int ClassroomID { get; set; }

    [Required, MaxLength(100), Display(Name = "Classroom Name")]
    public string ClassroomName { get; set; } = "";

    [Required, Range(1, 100), Display(Name = "Max Size")]
    public int MaxSize { get; set; }

    [Required, Display(Name = "Curriculum")]
    public int CurriculumID { get; set; }

    [Required, Display(Name = "Teacher")]
    public int TeacherID { get; set; }

    [Display(Name = "Supervisor")]
    public int? SupervisorID { get; set; }

    [Required, MaxLength(20), Display(Name = "Required Level")]
    public string RequiredLevel { get; set; } = "Beginner";

    public Curriculum Curriculum { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Supervisor? Supervisor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
