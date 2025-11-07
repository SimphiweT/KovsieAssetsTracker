using System.ComponentModel.DataAnnotations;

public class Asset
{
    [Key]
    public int AssetServerId { get; set; }
    public int? AssetLocalId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetDescription { get; set; } = string.Empty;
    public string? AssetImage { get; set; }
    public string? AssetQRCodeImage { get; set; }
    public string? QRCodeSecretText { get; set; }
    public string? Status { get; set; }
    public bool IsAndroidBackedUp { get; set; } =false;
    public bool IsServerBackedUp { get; set; } =false;
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    /*-------------------- Navigation Properties --------------------*/
    public int? RoomServerId { get; set; }
    public Room? Room { get; set; }

    public int? AssetCategoryServerId { get; set; }
    public AssetCategory? AssetCategory { get; set; }
}
