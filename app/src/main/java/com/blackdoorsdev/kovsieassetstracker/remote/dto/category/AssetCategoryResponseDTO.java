package com.blackdoorsdev.kovsieassetstracker.remote.dto.category;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.asset.AssetResponseDTO;
import java.util.List;

public class AssetCategoryResponseDTO {
    private int assetCategoryServerId;
    private Integer assetCategoryLocalId;
    private String assetCategoryName;
    private String assetCategoryDescription;
    private String assetCategoryImage;
    private boolean isAndroidBackedUp;
    private boolean isServerBackedUp;
    private String createdDate;
    private String updatedDate;
    private List<AssetResponseDTO> assets;

    // Getters and Setters
    public int getAssetCategoryServerId() { return assetCategoryServerId; }
    public void setAssetCategoryServerId(int id) { this.assetCategoryServerId = id; }

    public Integer getAssetCategoryLocalId() { return assetCategoryLocalId; }
    public void setAssetCategoryLocalId(Integer id) { this.assetCategoryLocalId = id; }

    public String getAssetCategoryName() { return assetCategoryName; }
    public void setAssetCategoryName(String name) { this.assetCategoryName = name; }

    public String getAssetCategoryDescription() { return assetCategoryDescription; }
    public void setAssetCategoryDescription(String desc) { this.assetCategoryDescription = desc; }

    public String getAssetCategoryImage() { return assetCategoryImage; }
    public void setAssetCategoryImage(String image) { this.assetCategoryImage = image; }

    public boolean isAndroidBackedUp() { return isAndroidBackedUp; }
    public void setAndroidBackedUp(boolean backedUp) { this.isAndroidBackedUp = backedUp; }

    public boolean isServerBackedUp() { return isServerBackedUp; }
    public void setServerBackedUp(boolean backedUp) { this.isServerBackedUp = backedUp; }

    public String getCreatedDate() { return createdDate; }
    public void setCreatedDate(String createdDate) { this.createdDate = createdDate; }

    public String getUpdatedDate() { return updatedDate; }
    public void setUpdatedDate(String updatedDate) { this.updatedDate = updatedDate; }

    public List<AssetResponseDTO> getAssets() { return assets; }
    public void setAssets(List<AssetResponseDTO> assets) { this.assets = assets; }
}
