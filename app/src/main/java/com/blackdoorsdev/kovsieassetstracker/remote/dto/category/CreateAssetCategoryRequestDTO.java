package com.blackdoorsdev.kovsieassetstracker.remote.dto.category;

public class CreateAssetCategoryRequestDTO {
    private Integer assetCategoryLocalId;
    private String assetCategoryName;
    private String assetCategoryDescription;
    private boolean isAndroidBackedUp;
    private boolean isServerBackedUp;
    private String imageFilePath; // path to image on device

    // Getters/Setters
    public Integer getAssetCategoryLocalId() { return assetCategoryLocalId; }
    public void setAssetCategoryLocalId(Integer id) { this.assetCategoryLocalId = id; }

    public String getAssetCategoryName() { return assetCategoryName; }
    public void setAssetCategoryName(String name) { this.assetCategoryName = name; }

    public String getAssetCategoryDescription() { return assetCategoryDescription; }
    public void setAssetCategoryDescription(String description) { this.assetCategoryDescription = description; }

    public boolean isAndroidBackedUp() { return isAndroidBackedUp; }
    public void setAndroidBackedUp(boolean backedUp) { this.isAndroidBackedUp = backedUp; }

    public boolean isServerBackedUp() { return isServerBackedUp; }
    public void setServerBackedUp(boolean backedUp) { this.isServerBackedUp = backedUp; }

    public String getImageFilePath() { return imageFilePath; }
    public void setImageFilePath(String path) { this.imageFilePath = path; }
}
