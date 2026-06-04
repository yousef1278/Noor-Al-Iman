using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class ZipCode
{
    [Key, MaxLength(10)]
    public string ZipCodeValue { get; set; } = "";

    [Required, MaxLength(50)]
    public string City { get; set; } = "";

    [Required, MaxLength(50)]
    public string State { get; set; } = "";

    public ICollection<Person> Persons { get; set; } = new List<Person>();
}
