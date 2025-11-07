package com.blackdoorsdev.kovsieassetstracker.database.dao;

import androidx.lifecycle.LiveData;
import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;

import java.util.List;

@Dao
public interface IAssetCategoryDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    long create(AssetCategoryEntity assetCategoryEntity);
    @Update
    int update(AssetCategoryEntity assetCategoryEntity);
    @Delete
    int delete(AssetCategoryEntity room);
    @Query("SELECT * FROM assets_categories ORDER BY asset_category_name")
    LiveData<List<AssetCategoryEntity>> getAllCategories();
    @Query("SELECT * FROM assets_categories WHERE asset_category_local_id = :id")
    LiveData<AssetCategoryEntity> getCategoryById(int id);
}
