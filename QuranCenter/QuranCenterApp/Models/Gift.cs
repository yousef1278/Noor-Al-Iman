using System.ComponentModel.DataAnnotations;

namespace QuranCenterApp.Models;

public class Gift
{
    [Key]
    public int GiftID { get; set; }

    [Required, MaxLength(100), Display(Name = "Gift Name")]
    public string GiftName { get; set; } = "";

    [Required, MaxLength(50), Display(Name = "Gift Type")]
    public string GiftType { get; set; } = "";

    [Required, Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public ICollection<GiftDistribution> GiftDistributions { get; set; } = new List<GiftDistribution>();
}
