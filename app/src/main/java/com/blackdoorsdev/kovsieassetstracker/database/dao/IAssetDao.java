package com.blackdoorsdev.kovsieassetstracker.database.dao;

import androidx.lifecycle.LiveData;
import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;

import java.util.List;

@Dao
public interface IAssetDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    long create(AssetEntity asset);
    @Update
    int update(AssetEntity asset);
    @Delete
    int delete(AssetEntity asset);
    @Query("SELECT * FROM assets ORDER BY asset_name")
    LiveData<List<AssetEntity>> getAllAssets();
    @Query("SELECT * FROM assets WHERE assetLocal_id = :id")
    LiveData<AssetEntity> getAssetById(int id);
    @Query("SELECT * FROM assets WHERE asset_qrcode_text = :qrSecret LIMIT 1")
    AssetEntity findByQrSecret(String qrSecret);

}
