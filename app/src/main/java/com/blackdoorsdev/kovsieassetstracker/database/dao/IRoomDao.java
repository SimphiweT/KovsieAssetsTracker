package com.blackdoorsdev.kovsieassetstracker.database.dao;

import androidx.lifecycle.LiveData;
import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;

import java.util.List;

@Dao
public interface IRoomDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    long create(RoomEntity room);

    @Update
    int update(RoomEntity room);

    @Delete
    int delete(RoomEntity room);

    @Query("SELECT * FROM rooms ORDER BY room_name ASC")
    LiveData<List<RoomEntity>> getAllRooms();

    @Query("SELECT * FROM rooms WHERE room_local_id = :id")
    LiveData<RoomEntity> getRoomById(int id);

    @Query("DELETE FROM rooms")
    void deleteAll();
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void insertAll(List<RoomEntity> rooms);


}
