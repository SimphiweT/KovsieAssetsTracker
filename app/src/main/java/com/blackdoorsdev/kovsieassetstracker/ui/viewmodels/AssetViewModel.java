package com.blackdoorsdev.kovsieassetstracker.ui.viewmodels;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetRepository;
import java.util.List;

public class AssetViewModel extends AndroidViewModel {

    private final AssetRepository assetRepository;
    private final LiveData<List<AssetEntity>> allAssets;

    public AssetViewModel(@NonNull Application application) {
        super(application);
        assetRepository = AssetRepository.getInstance(application.getApplicationContext());
        allAssets = assetRepository.getAllAssets();
    }

    public LiveData<List<AssetEntity>> getAllAssets() {
        return allAssets;
    }

    public LiveData<AssetEntity> getAssetById(int id) {
        return assetRepository.getAssetById(id);
    }

    public void insert(AssetEntity asset) {
        assetRepository.insert(asset);
    }

    public void update(AssetEntity asset) {
        assetRepository.update(asset);
    }

    public void delete(AssetEntity asset) {
        assetRepository.delete(asset);
    }

    public LiveData<String> uploadToServer(AssetEntity asset) {
        return assetRepository.uploadAssetToServer(asset);
    }

    public LiveData<String> updateToServer(AssetEntity asset) {
        return assetRepository.updateAssetOnServer(asset);
    }

    public LiveData<String> syncWithServer() {
        return assetRepository.syncAssetsFromServer();
    }
}
