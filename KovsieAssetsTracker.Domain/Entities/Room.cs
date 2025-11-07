using System.ComponentModel.DataAnnotations;
public class Room
{
    [Key]
    public int RoomServerId { get; set; }
    public int? RoomLocalId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomDescription { get; set; } = string.Empty;
    public string? RoomImage { get; set; }
    public bool IsAndroidBackedUp { get; set; }=false;
    public bool IsServerBackedUp { get; set; } = false; 
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    /*-------------------- Navigation Properties --------------------*/
    public int? FloorServerId { get; set; }
    public Floor? Floor { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
