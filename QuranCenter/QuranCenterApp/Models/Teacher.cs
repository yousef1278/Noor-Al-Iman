using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenterApp.Models;

public class Teacher
{
    [Key]
    public int TeacherID { get; set; }

    [Required, MaxLength(100), Display(Name = "Specialization")]
    public string Specialization { get; set; } = "";

    [Required, DataType(DataType.Date), Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [ForeignKey(nameof(TeacherID))]
    public Person Person { get; set; } = null!;

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}
