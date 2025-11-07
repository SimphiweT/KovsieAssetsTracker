package com.blackdoorsdev.kovsieassetstracker.ui.viewmodels;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.RoomRepository;

import java.util.List;

public class RoomViewModel extends AndroidViewModel {
    private final RoomRepository roomRepository;
    private final LiveData<List<RoomEntity>> allRooms;

    public RoomViewModel(@NonNull Application application) {
        super(application);
        roomRepository = RoomRepository.getInstance(application.getApplicationContext());
        allRooms = roomRepository.getAllRooms();
    }

    public LiveData<List<RoomEntity>> getAllRooms() {
        return allRooms;
    }

    public LiveData<RoomEntity> getRoomById(int id) {
        return roomRepository.getRoomById(id);
    }

    public void insert(RoomEntity room) {
        roomRepository.insert(room);
    }

    public void update(RoomEntity room) {
        roomRepository.update(room);
    }

    public void delete(RoomEntity room) {
        roomRepository.delete(room);
    }

    public LiveData<String> syncRoomsFromServer() {
        return roomRepository.syncRoomsFromServer();
    }

    public LiveData<String> uploadRoom(RoomEntity room) {
        return roomRepository.uploadRoomToServer(room);
    }
}

