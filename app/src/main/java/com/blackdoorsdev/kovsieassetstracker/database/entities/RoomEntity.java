package com.blackdoorsdev.kovsieassetstracker.database.entities;

import androidx.room.ColumnInfo;
import androidx.room.Entity;
import androidx.room.Ignore;
import androidx.room.PrimaryKey;
import androidx.room.TypeConverters;

import com.blackdoorsdev.kovsieassetstracker.helpers.DateConverter;

import java.util.Date;

@Entity(tableName = "rooms")
@TypeConverters({DateConverter.class})
public class RoomEntity {

    @PrimaryKey(autoGenerate = true)
    @ColumnInfo(name = "room_local_id")
    private Integer roomLocalId;

    @ColumnInfo(name = "room_server_id")
    private Integer roomServerId;

    @ColumnInfo(name = "room_name")
    private String roomName;

    @ColumnInfo(name = "room_description")
    private String roomDescription;

    @ColumnInfo(name = "room_image")
    private String roomImage;

    @ColumnInfo(name = "is_android_backed_up")
    private boolean isAndroidBackedUp;

    @ColumnInfo(name = "is_server_backed_up")
    private boolean isServerBackedUp;

    @ColumnInfo(name = "created_date")
    private Date createdDate;

    @ColumnInfo(name = "updated_date")
    private Date updatedDate;

    @ColumnInfo(name = "floor_server_id")
    private Integer floorServerId;

    // === Constructors ===

    // Main Room Constructor (Used by Room ORM)
    public RoomEntity(Integer roomLocalId, Integer roomServerId, String roomName, String roomDescription,
                      String roomImage, boolean isAndroidBackedUp, boolean isServerBackedUp,
                      Date createdDate, Date updatedDate) {
        this.roomLocalId = roomLocalId;
        this.roomServerId = roomServerId;
        this.roomName = roomName;
        this.roomDescription = roomDescription;
        this.roomImage = roomImage;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
        this.updatedDate = updatedDate;
    }

    // Ignore this one (used for inserts or fragments)
    @Ignore
    public RoomEntity(Integer roomServerId, String roomName, String roomDescription,
                      String roomImage, boolean isAndroidBackedUp, boolean isServerBackedUp,
                      Date createdDate) {
        this(null, roomServerId, roomName, roomDescription, roomImage,
                isAndroidBackedUp, isServerBackedUp, createdDate, null);
    }

    // === Getters/Setters ===
    public Integer getRoomLocalId() { return roomLocalId; }
    public void setRoomLocalId(Integer roomLocalId) { this.roomLocalId = roomLocalId; }

    public Integer getRoomServerId() { return roomServerId; }
    public void setRoomServerId(Integer roomServerId) { this.roomServerId = roomServerId; }

    public String getRoomName() { return roomName; }
    public void setRoomName(String roomName) { this.roomName = roomName; }

    public String getRoomDescription() { return roomDescription; }
    public void setRoomDescription(String roomDescription) { this.roomDescription = roomDescription; }

    public String getRoomImage() { return roomImage; }
    public void setRoomImage(String roomImage) { this.roomImage = roomImage; }

    public boolean isAndroidBackedUp() { return isAndroidBackedUp; }
    public void setAndroidBackedUp(boolean androidBackedUp) { isAndroidBackedUp = androidBackedUp; }

    public boolean isServerBackedUp() { return isServerBackedUp; }
    public void setServerBackedUp(boolean serverBackedUp) { isServerBackedUp = serverBackedUp; }

    public Date getCreatedDate() { return createdDate; }
    public void setCreatedDate(Date createdDate) { this.createdDate = createdDate; }

    public Date getUpdatedDate() { return updatedDate; }
    public void setUpdatedDate(Date updatedDate) { this.updatedDate = updatedDate; }

    public Integer getFloorServerId() { return floorServerId; }
    public void setFloorServerId(Integer floorServerId) { this.floorServerId = floorServerId; }

    // UI expansion flag (optional)
    @Ignore
    private boolean isExpanded;
    public boolean isExpanded() { return isExpanded; }
    public void setExpanded(boolean expanded) { isExpanded = expanded; }
}
