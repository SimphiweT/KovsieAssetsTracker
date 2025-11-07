package com.blackdoorsdev.kovsieassetstracker.database.entities;

import androidx.room.ColumnInfo;
import androidx.room.Entity;
import androidx.room.PrimaryKey;
import androidx.room.TypeConverters;

import com.blackdoorsdev.kovsieassetstracker.helpers.DateConverter;

import java.util.Date;

@Entity(tableName = "floors")
@TypeConverters({DateConverter.class})
public class FloorEntity {

    @PrimaryKey(autoGenerate = true)
    @ColumnInfo(name = "floor_local_id")
    private Integer floorLocalId;

    @ColumnInfo(name = "floor_server_id")
    private Integer floorServerId;

    @ColumnInfo(name = "floor_name")
    private String floorName;

    @ColumnInfo(name = "floor_description")
    private String floorDescription;

    @ColumnInfo(name = "is_android_backed_up")
    private boolean isAndroidBackedUp;

    @ColumnInfo(name = "is_server_backed_up")
    private boolean isServerBackedUp;

    @ColumnInfo(name = "created_date")
    private Date createdDate;

    @ColumnInfo(name = "updated_date")
    private Date updatedDate;

    @ColumnInfo(name = "building_server_id")
    private Integer buildingServerId;

    // === Constructors ===
    public FloorEntity(Integer floorServerId, String floorName, String floorDescription,
                       boolean isAndroidBackedUp, boolean isServerBackedUp,
                       Date createdDate, Date updatedDate, Integer buildingServerId) {
        this.floorServerId = floorServerId;
        this.floorName = floorName;
        this.floorDescription = floorDescription;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.createdDate = createdDate;
        this.updatedDate = updatedDate;
        this.buildingServerId = buildingServerId;
    }

    // === Getters and Setters ===
    public Integer getFloorLocalId() { return floorLocalId; }
    public void setFloorLocalId(Integer floorLocalId) { this.floorLocalId = floorLocalId; }

    public Integer getFloorServerId() { return floorServerId; }
    public void setFloorServerId(Integer floorServerId) { this.floorServerId = floorServerId; }

    public String getFloorName() { return floorName; }
    public void setFloorName(String floorName) { this.floorName = floorName; }

    public String getFloorDescription() { return floorDescription; }
    public void setFloorDescription(String floorDescription) { this.floorDescription = floorDescription; }

    public boolean isAndroidBackedUp() { return isAndroidBackedUp; }
    public void setAndroidBackedUp(boolean androidBackedUp) { isAndroidBackedUp = androidBackedUp; }

    public boolean isServerBackedUp() { return isServerBackedUp; }
    public void setServerBackedUp(boolean serverBackedUp) { isServerBackedUp = serverBackedUp; }

    public Date getCreatedDate() { return createdDate; }
    public void setCreatedDate(Date createdDate) { this.createdDate = createdDate; }

    public Date getUpdatedDate() { return updatedDate; }
    public void setUpdatedDate(Date updatedDate) { this.updatedDate = updatedDate; }

    public Integer getBuildingServerId() { return buildingServerId; }
    public void setBuildingServerId(Integer buildingServerId) { this.buildingServerId = buildingServerId; }
}
