package com.blackdoorsdev.kovsieassetstracker.database;

import androidx.room.Database;
import androidx.room.RoomDatabase;
import androidx.room.TypeConverters;

import com.blackdoorsdev.kovsieassetstracker.database.dao.IAssetCategoryDao;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IAssetDao;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IFloorDao;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IRoomDao;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.FloorEntity;
import com.blackdoorsdev.kovsieassetstracker.helpers.Converters;

@Database(
        entities = {
                AssetCategoryEntity.class,
                RoomEntity.class,
                AssetEntity.class,
                FloorEntity.class
        },
        version = 7,
        exportSchema = true
)
@TypeConverters({Converters.class})
public abstract class AppDatabase extends RoomDatabase {
    public abstract IRoomDao roomDao();
    public abstract IAssetCategoryDao assetCategoryDao();
    public abstract IAssetDao assetDao();
    public abstract IFloorDao floorDao();
}
