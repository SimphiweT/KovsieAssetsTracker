package com.blackdoorsdev.kovsieassetstracker.database.entities;

import androidx.room.ColumnInfo;
import androidx.room.Entity;
import androidx.room.ForeignKey;
import androidx.room.Ignore;
import androidx.room.PrimaryKey;
import androidx.room.TypeConverters;

import com.blackdoorsdev.kovsieassetstracker.helpers.DateConverter;

import java.util.Date;

@Entity(
        tableName = "assets",
        foreignKeys = {
                @ForeignKey(
                        entity = AssetCategoryEntity.class,
                        parentColumns = "asset_category_local_id",
                        childColumns = "assetCategoryLocalId",
                        onDelete = ForeignKey.CASCADE
                ),
                @ForeignKey(
                        entity = RoomEntity.class,
                        parentColumns = "room_local_id",
                        childColumns = "roomLocalId",
                        onDelete = ForeignKey.CASCADE
                )

        }
)
@TypeConverters({DateConverter.class})
public class AssetEntity {

    @PrimaryKey(autoGenerate = true)
    @ColumnInfo(name = "assetLocal_id")
    private int assetLocalID;

    @ColumnInfo(name = "assetServer_id")
    private Integer assetServerId; // nullable for new local-only assets

    @ColumnInfo(name = "asset_name")
    private String assetName;

    @ColumnInfo(name = "asset_description")
    private String assetDescription;

    @ColumnInfo(name = "asset_image")
    private String assetImage;

    @ColumnInfo(name = "asset_qrcode_image")
    private String assetQrCodeImage;

    @ColumnInfo(name = "asset_qrcode_text")
    private String assetQrCodeSecretText;

    @ColumnInfo(name = "asset_status")
    private String assetStatus;

    @ColumnInfo(name = "is_android_backed_up")
    private boolean isAndroidBackedUp;

    @ColumnInfo(name = "is_server_backed_up")
    private boolean isServerBackedUp;

    @ColumnInfo(name = "created_date")
    private Date createdDate;

    @ColumnInfo(name = "updated_date")
    private Date updatedDate;

    @ColumnInfo(index = true)
    private Integer assetCategoryLocalId; // nullable

    @ColumnInfo(index = true)
    private Integer roomLocalId; // nullable

    @Ignore
    private boolean isExpanded; // for RecyclerView UI


    public AssetEntity(int assetLocalID, Integer assetServerId, String assetName, String assetDescription,
                       String assetImage, String assetQrCodeImage, String assetQrCodeSecretText,
                       String assetStatus, boolean isAndroidBackedUp, boolean isServerBackedUp,
                       Date createdDate, Date updatedDate, Integer assetCategoryLocalId, Integer roomLocalId) {
        this.assetLocalID = assetLocalID;
        this.assetServerId = assetServerId;
        this.assetName = assetName;
        this.assetDescription = assetDescription;
        this.assetImage = assetImage;
        this.assetQrCodeImage = assetQrCodeImage;
        this.assetQrCodeSecretText = assetQrCodeSecretText;
        this.assetStatus = assetStatus;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
        this.updatedDate = updatedDate;
        this.assetCategoryLocalId = assetCategoryLocalId;
        this.roomLocalId = roomLocalId;
    }
    @Ignore
    //For Insert

    public AssetEntity(Integer assetServerId, String assetName, String assetDescription, String assetImage, String assetQrCodeImage, String assetQrCodeSecretText, String assetStatus, boolean isAndroidBackedUp, boolean isServerBackedUp, Date createdDate, Integer assetCategoryLocalId, Integer roomLocalId) {
        this.assetServerId = assetServerId;
        this.assetName = assetName;
        this.assetDescription = assetDescription;
        this.assetImage = assetImage;
        this.assetQrCodeImage = assetQrCodeImage;
        this.assetQrCodeSecretText = assetQrCodeSecretText;
        this.assetStatus = assetStatus;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
        this.assetCategoryLocalId = assetCategoryLocalId;
        this.roomLocalId = roomLocalId;
    }

    public int getAssetLocalID() {
        return assetLocalID;
    }

    public void setAssetLocalID(int assetLocalID) {
        this.assetLocalID = assetLocalID;
    }

    public Integer getAssetServerId() {
        return assetServerId;
    }

    public void setAssetServerId(Integer assetServerId) {
        this.assetServerId = assetServerId;
    }

    public String getAssetName() {
        return assetName;
    }

    public void setAssetName(String assetName) {
        this.assetName = assetName;
    }

    public String getAssetDescription() {
        return assetDescription;
    }

    public void setAssetDescription(String assetDescription) {
        this.assetDescription = assetDescription;
    }

    public String getAssetImage() {
        return assetImage;
    }

    public void setAssetImage(String assetImage) {
        this.assetImage = assetImage;
    }

    public String getAssetQrCodeImage() {
        return assetQrCodeImage;
    }

    public void setAssetQrCodeImage(String assetQrCodeImage) {
        this.assetQrCodeImage = assetQrCodeImage;
    }

    public String getAssetQrCodeSecretText() {
        return assetQrCodeSecretText;
    }

    public void setAssetQrCodeSecretText(String assetQrCodeSecretText) {
        this.assetQrCodeSecretText = assetQrCodeSecretText;
    }

    public String getAssetStatus() {
        return assetStatus;
    }

    public void setAssetStatus(String assetStatus) {
        this.assetStatus = assetStatus;
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

    public Integer getAssetCategoryLocalId() {
        return assetCategoryLocalId;
    }

    public void setAssetCategoryLocalId(Integer assetCategoryLocalId) {
        this.assetCategoryLocalId = assetCategoryLocalId;
    }

    public Integer getRoomLocalId() {
        return roomLocalId;
    }

    public void setRoomLocalId(Integer roomLocalId) {
        this.roomLocalId = roomLocalId;
    }

    public boolean isExpanded() {
        return isExpanded;
    }

    public void setExpanded(boolean expanded) {
        isExpanded = expanded;
    }

    @Override
    public String toString() {
        return "AssetEntity{" +
                "assetLocalID=" + assetLocalID +
                ", assetServerId=" + assetServerId +
                ", assetName='" + assetName + '\'' +
                ", assetDescription='" + assetDescription + '\'' +
                ", assetImage='" + assetImage + '\'' +
                ", assetQrCodeImage='" + assetQrCodeImage + '\'' +
                ", assetQrCodeSecretText='" + assetQrCodeSecretText + '\'' +
                ", assetStatus='" + assetStatus + '\'' +
                ", isAndroidBackedUp=" + isAndroidBackedUp +
                ", isServerBackedUp=" + isServerBackedUp +
                ", createdDate=" + createdDate +
                ", updatedDate=" + updatedDate +
                ", assetCategoryLocalId=" + assetCategoryLocalId +
                ", roomLocalId=" + roomLocalId +
                ", isExpanded=" + isExpanded +
                '}';
    }
}
