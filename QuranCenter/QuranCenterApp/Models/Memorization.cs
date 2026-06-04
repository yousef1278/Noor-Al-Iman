using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Memorization
{
    [Key]
    public int MemorizationID { get; set; }

    [Required]
    public int StudentID { get; set; }

    [Required, MaxLength(100), Display(Name = "Surah Name")]
    public string SurahName { get; set; } = "";

    [Required, Range(1, 286), Display(Name = "From Ayah")]
    public int FromAyah { get; set; }

    [Required, Range(1, 286), Display(Name = "To Ayah")]
    public int ToAyah { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Date Completed")]
    public DateTime DateCompleted { get; set; } = DateTime.Today;

    [Range(1, 5)]
    public byte? Rating { get; set; }

    public Student Student { get; set; } = null!;
}
