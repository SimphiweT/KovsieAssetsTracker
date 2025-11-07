using System.ComponentModel.DataAnnotations;

public class Floor
{
    [Key]
    public int FloorServerId { get; set; }
    public int? FloorLocalId { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public string? FloorDescription { get; set; }
    public bool IsAndroidBackedUp { get; set; } = false;
    public bool IsServerBackedUp { get; set; } = false;
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    /*-------------------- Navigation Properties --------------------*/
    public int? BuildingServerId { get; set; }
    public Building? Building { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
