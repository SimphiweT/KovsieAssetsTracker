package com.blackdoorsdev.kovsieassetstracker.remote.dto.floor;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.room.RoomResponseDTO;
import java.util.Date;
import java.util.List;

public class FloorResponseDTO {
    public int floorServerId;
    public Integer floorLocalId;
    public String floorName;
    public String floorDescription;
    public boolean isAndroidBackedUp;
    public boolean isServerBackedUp;
    public Date createdDate;
    public Date updatedDate;
    public Integer buildingServerId;
    public String buildingName;

    // Added to match API structure
    public List<RoomResponseDTO> rooms;
}
