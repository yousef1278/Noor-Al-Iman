using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenterApp.Models;

public class Supervisor
{
    [Key]
    public int SupervisorID { get; set; }

    [Required, MaxLength(100), Display(Name = "Department")]
    public string Department { get; set; } = "";

    [ForeignKey(nameof(SupervisorID))]
    public Person Person { get; set; } = null!;

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}
