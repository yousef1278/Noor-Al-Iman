using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Attendance
{
    [Key]
    public int AttendanceID { get; set; }

    [Required]
    public int StudentID { get; set; }

    [Required]
    public int ClassroomID { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Date")]
    public DateTime AttendanceDate { get; set; } = DateTime.Today;

    [Required, StringLength(1), Display(Name = "Status")]
    public string Status { get; set; } = "P"; // P=Present, A=Absent, L=Late

    [MaxLength(300)]
    public string? Notes { get; set; }

    public Student Student { get; set; } = null!;
    public Classroom Classroom { get; set; } = null!;
}
