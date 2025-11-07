package com.blackdoorsdev.kovsieassetstracker.ui.viewmodels;

import android.app.Application;

import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;

import com.blackdoorsdev.kovsieassetstracker.database.entities.FloorEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.FloorRepository;

import java.util.List;

public class FloorViewModel extends AndroidViewModel {

    private final FloorRepository repo;
    private final LiveData<List<FloorEntity>> allFloors;

    public FloorViewModel(@NonNull Application application) {
        super(application);
        repo = FloorRepository.getInstance(application);
        allFloors = repo.getAllFloors();
    }

    public LiveData<List<FloorEntity>> getAllFloors() {
        return allFloors;
    }

    public LiveData<FloorEntity> getFloorById(int id) {
        return repo.getFloorById(id);
    }

    public LiveData<String> syncFloorsFromServer() {
        return repo.syncFloorsFromServer();
    }
}
