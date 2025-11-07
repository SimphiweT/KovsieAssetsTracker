using System.ComponentModel.DataAnnotations;

public class Campus
{
    [Key]
    public int CampusServerId { get; set; }
    public int? CampusLocalId { get; set; }
    public string CampusName { get; set; } = string.Empty;
    public string CampusDescription { get; set; } = string.Empty;
    public string? CampusImage { get; set; }
    public bool IsAndroidBackedUp { get; set; } = false;
    public bool IsServerBackedUp { get; set; } = false;
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    /*-------------------- Navigation Properties --------------------*/
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
}
