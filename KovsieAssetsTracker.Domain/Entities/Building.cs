using System.ComponentModel.DataAnnotations;
public class Building
{
    [Key]
    public int BuildingServerId { get; set; }
    public int? BuildingLocalId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string BuildingDescription { get; set; } = string.Empty;
    public string? BuildingImage { get; set; }
    public bool IsAndroidBackedUp { get; set; } = false;
    public bool IsServerBackedUp { get; set; } = false;
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    /*-------------------- Navigation Properties --------------------*/
    public int? CampusServerId { get; set; }
    public Campus? Campus { get; set; }

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
