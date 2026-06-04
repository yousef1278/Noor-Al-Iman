using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class GiftDistribution
{
    [Key]
    public int DistributionID { get; set; }

    [Required]
    public int GiftID { get; set; }

    [Required]
    public int StudentID { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Date Received")]
    public DateTime DateReceived { get; set; } = DateTime.Today;

    public Gift Gift { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
