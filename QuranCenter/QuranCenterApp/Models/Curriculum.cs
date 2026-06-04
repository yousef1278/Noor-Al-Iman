using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Curriculum
{
    [Key]
    public int CurriculumID { get; set; }

    [Required, MaxLength(100), Display(Name = "Curriculum Name")]
    public string CurriculumName { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string Category { get; set; } = "Holy Quran";

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}
