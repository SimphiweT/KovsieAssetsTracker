package com.blackdoorsdev.kovsieassetstracker.remote.dto.room;

import java.io.File;

public class CreateRoomRequestDTO {
    public Integer roomLocalId;
    public String roomName;
    public String roomDescription;
    public File imageFile;
    public boolean isAndroidBackedUp;
    public boolean isServerBackedUp;
    public Integer floorServerId;

    public CreateRoomRequestDTO(Integer roomLocalId, String roomName, String roomDescription,
                                File imageFile, boolean isAndroidBackedUp,
                                boolean isServerBackedUp, Integer floorServerId) {
        this.roomLocalId = roomLocalId;
        this.roomName = roomName;
        this.roomDescription = roomDescription;
        this.imageFile = imageFile;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.floorServerId = floorServerId;
    }
}
