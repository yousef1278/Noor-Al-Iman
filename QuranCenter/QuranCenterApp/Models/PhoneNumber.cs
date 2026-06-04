using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class PhoneNumber
{
    [Key]
    public int PhoneID { get; set; }

    [Required]
    public int PersonID { get; set; }

    [Required, MaxLength(20), Display(Name = "Phone Number")]
    public string Phone { get; set; } = "";

    public Person Person { get; set; } = null!;
}
