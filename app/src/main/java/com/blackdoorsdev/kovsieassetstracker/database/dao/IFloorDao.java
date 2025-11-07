package com.blackdoorsdev.kovsieassetstracker.database.dao;

import androidx.lifecycle.LiveData;
import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import com.blackdoorsdev.kovsieassetstracker.database.entities.FloorEntity;

import java.util.List;

@Dao
public interface IFloorDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void insertAll(List<FloorEntity> floors);

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    long create(FloorEntity floor);

    @Update
    int update(FloorEntity floor);

    @Delete
    int delete(FloorEntity floor);

    @Query("DELETE FROM floors")
    void deleteAll();

    @Query("SELECT * FROM floors ORDER BY floor_name ASC")
    LiveData<List<FloorEntity>> getAllFloors();

    @Query("SELECT * FROM floors WHERE floor_local_id = :id LIMIT 1")
    LiveData<FloorEntity> getFloorById(int id);

    @Query("SELECT * FROM floors WHERE floor_server_id = :serverId LIMIT 1")
    FloorEntity getByServerId(int serverId);
}
