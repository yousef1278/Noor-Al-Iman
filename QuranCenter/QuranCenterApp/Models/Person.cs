using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenterApp.Models;

public class Person
{
    [Key]
    public int PersonID { get; set; }

    [Required, MaxLength(50), Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(50), Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required, StringLength(1), Display(Name = "Gender")]
    public string Gender { get; set; } = "M";

    [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(100)]
    public string? Address { get; set; }

    [MaxLength(10), Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }

    [EmailAddress, MaxLength(100)]
    public string? Email { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public ZipCode? ZipCodeNav { get; set; }
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public Supervisor? Supervisor { get; set; }
    public ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
}
