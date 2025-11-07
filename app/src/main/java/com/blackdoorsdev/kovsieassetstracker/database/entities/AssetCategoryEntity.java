package com.blackdoorsdev.kovsieassetstracker.database.entities;

import androidx.room.ColumnInfo;
import androidx.room.Entity;
import androidx.room.Ignore;
import androidx.room.PrimaryKey;

import java.util.Date;

@Entity(tableName = "assets_categories")
public class AssetCategoryEntity {
    @PrimaryKey(autoGenerate = true)
    @ColumnInfo(name = "asset_category_local_id")
    private int assetCategoryLocalId;
    @ColumnInfo(name = "asset_category_server_id")
    private Integer assetCategoryServerId;
    @ColumnInfo(name = "asset_category_name")
    private String assetCategoryName;
    @ColumnInfo(name = "asset_category_description")
    private String assetCategoryDescription;
    @ColumnInfo(name = "asset_category_image")
    private String assetCategoryImage;
    @ColumnInfo(name = "is_android_backed_up")
    private boolean isAndroidBackedUp;
    @ColumnInfo(name = "is_server_backed_up")
    private boolean isServerBackedUp;
    @ColumnInfo(name = "created_date")
    private Date createdDate;
    @ColumnInfo(name = "updated_date")
    private Date updatedDate;
    @Ignore
    private boolean isExpanded;

    //For RoomDB
    public AssetCategoryEntity(int assetCategoryLocalId, Integer assetCategoryServerId, String assetCategoryName, String assetCategoryDescription, String assetCategoryImage, boolean isAndroidBackedUp, boolean isServerBackedUp, Date createdDate, Date updatedDate) {
        this.assetCategoryLocalId = assetCategoryLocalId;
        this.assetCategoryServerId = assetCategoryServerId;
        this.assetCategoryName = assetCategoryName;
        this.assetCategoryDescription = assetCategoryDescription;
        this.assetCategoryImage = assetCategoryImage;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
        this.updatedDate = updatedDate;
    }
    //

    //For Creating
    @Ignore
    public AssetCategoryEntity(Integer assetCategoryServerId, String assetCategoryName, String assetCategoryDescription, String assetCategoryImage, boolean isAndroidBackedUp, boolean isServerBackedUp, Date createdDate) {
        this.assetCategoryServerId = assetCategoryServerId;
        this.assetCategoryName = assetCategoryName;
        this.assetCategoryDescription = assetCategoryDescription;
        this.assetCategoryImage = assetCategoryImage;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
    }


    public int getAssetCategoryLocalId() {
        return assetCategoryLocalId;
    }

    public void setAssetCategoryLocalId(int assetCategoryLocalId) {
        this.assetCategoryLocalId = assetCategoryLocalId;
    }

    public Integer getAssetCategoryServerId() {
        return assetCategoryServerId;
    }

    public void setAssetCategoryServerId(Integer assetCategoryServerId) {
        this.assetCategoryServerId = assetCategoryServerId;
    }

    public String getAssetCategoryName() {
        return assetCategoryName;
    }

    public void setAssetCategoryName(String assetCategoryName) {
        this.assetCategoryName = assetCategoryName;
    }

    public String getAssetCategoryDescription() {
        return assetCategoryDescription;
    }

    public void setAssetCategoryDescription(String assetCategoryDescription) {
        this.assetCategoryDescription = assetCategoryDescription;
    }

    public String getAssetCategoryImage() {
        return assetCategoryImage;
    }

    public void setAssetCategoryImage(String assetCategoryImage) {
        this.assetCategoryImage = assetCategoryImage;
    }

    public boolean isAndroidBackedUp() {
        return isAndroidBackedUp;
    }

    public void setAndroidBackedUp(boolean androidBackedUp) {
        isAndroidBackedUp = androidBackedUp;
    }

    public boolean isServerBackedUp() {
        return isServerBackedUp;
    }

    public void setServerBackedUp(boolean serverBackedUp) {
        isServerBackedUp = serverBackedUp;
    }

    public Date getCreatedDate() {
        return createdDate;
    }

    public void setCreatedDate(Date createdDate) {
        this.createdDate = createdDate;
    }

    public Date getUpdatedDate() {
        return updatedDate;
    }

    public void setUpdatedDate(Date updatedDate) {
        this.updatedDate = updatedDate;
    }

    public boolean isExpanded() {
        return isExpanded;
    }

    public void setExpanded(boolean expanded) {
        isExpanded = expanded;
    }

    @Override
    public String toString() {
        return "AssetCategoryEntity{" +
                "assetCategoryLocalId=" + assetCategoryLocalId +
                ", assetCategoryServerId=" + assetCategoryServerId +
                ", assetCategoryName='" + assetCategoryName + '\'' +
                ", assetCategoryDescription='" + assetCategoryDescription + '\'' +
                ", assetCategoryImage='" + assetCategoryImage + '\'' +
                ", isAndroidBackedUp=" + isAndroidBackedUp +
                ", isServerBackedUp=" + isServerBackedUp +
                ", createdDate=" + createdDate +
                ", updatedDate=" + updatedDate +
                ", isExpanded=" + isExpanded +
                '}';
    }
}
