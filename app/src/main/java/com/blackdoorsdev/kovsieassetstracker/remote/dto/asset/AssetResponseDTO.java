package com.blackdoorsdev.kovsieassetstracker.remote.dto.asset;

import com.google.gson.annotations.SerializedName;
import java.util.Date;

/**
 * Represents a single asset response from the API.
 * Matches KovsieAssetsTracker.Api.Controllers.AssetsController output.
 */
public class AssetResponseDTO {

    @SerializedName("assetServerId")
    private int assetServerId;

    @SerializedName("assetLocalId")
    private Integer assetLocalId;

    @SerializedName("assetName")
    private String assetName;

    @SerializedName("assetDescription")
    private String assetDescription;

    @SerializedName("assetImage")
    private String assetImage;

    @SerializedName("assetQRCodeImage")
    private String assetQRCodeImage;

    @SerializedName("qRCodeSecretText")
    private String qRCodeSecretText;

    @SerializedName("status")
    private String status;

    @SerializedName("isAndroidBackedUp")
    private boolean isAndroidBackedUp;

    @SerializedName("isServerBackedUp")
    private boolean isServerBackedUp;

    @SerializedName("createdDate")
    private Date createdDate;

    @SerializedName("updatedDate")
    private Date updatedDate;

    @SerializedName("roomServerId")
    private Integer roomServerId;

    @SerializedName("roomName")
    private String roomName;

    @SerializedName("assetCategoryServerId")
    private Integer assetCategoryServerId;

    @SerializedName("assetCategoryName")
    private String assetCategoryName;

    // === Getters ===
    public int getAssetServerId() { return assetServerId; }
    public Integer getAssetLocalId() { return assetLocalId; }
    public String getAssetName() { return assetName; }
    public String getAssetDescription() { return assetDescription; }
    public String getAssetImage() { return assetImage; }
    public String getAssetQRCodeImage() { return assetQRCodeImage; }
    public String getQRCodeSecretText() { return qRCodeSecretText; }
    public String getStatus() { return status; }
    public boolean isAndroidBackedUp() { return isAndroidBackedUp; }
    public boolean isServerBackedUp() { return isServerBackedUp; }
    public Date getCreatedDate() { return createdDate; }
    public Date getUpdatedDate() { return updatedDate; }
    public Integer getRoomServerId() { return roomServerId; }
    public String getRoomName() { return roomName; }
    public Integer getAssetCategoryServerId() { return assetCategoryServerId; }
    public String getAssetCategoryName() { return assetCategoryName; }

    @Override
    public String toString() {
        return "AssetResponseDTO{" +
                "assetServerId=" + assetServerId +
                ", assetLocalId=" + assetLocalId +
                ", assetName='" + assetName + '\'' +
                ", assetDescription='" + assetDescription + '\'' +
                ", assetImage='" + assetImage + '\'' +
                ", assetQRCodeImage='" + assetQRCodeImage + '\'' +
                ", qRCodeSecretText='" + qRCodeSecretText + '\'' +
                ", status='" + status + '\'' +
                ", isAndroidBackedUp=" + isAndroidBackedUp +
                ", isServerBackedUp=" + isServerBackedUp +
                ", createdDate=" + createdDate +
                ", updatedDate=" + updatedDate +
                ", roomServerId=" + roomServerId +
                ", roomName='" + roomName + '\'' +
                ", assetCategoryServerId=" + assetCategoryServerId +
                ", assetCategoryName='" + assetCategoryName + '\'' +
                '}';
    }
}
