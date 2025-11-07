package com.blackdoorsdev.kovsieassetstracker.remote.dto.room;

import java.util.Date;
import java.util.List;

public class RoomResponseDTO {
    public int roomServerId;
    public Integer roomLocalId;
    public String roomName;
    public String roomDescription;
    public String roomImage;
    public boolean isAndroidBackedUp;
    public boolean isServerBackedUp;
    public Date createdDate;
    public Date updatedDate;
    public Integer floorServerId;
    public String floorName;
    public List<Object> assets; // optional if you don’t need nested data
}
