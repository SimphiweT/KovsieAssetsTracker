package com.blackdoorsdev.kovsieassetstracker.ui.viewmodels;

import android.app.Application;

import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;

import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetCategoryRepository;

import java.util.List;

public class AssetCategoryViewModel extends AndroidViewModel {

    private final AssetCategoryRepository categoryRepository;
    private final LiveData<List<AssetCategoryEntity>> allCategories;

    public AssetCategoryViewModel(@NonNull Application application) {
        super(application);

        // Pass context (the application) to repository
        categoryRepository = AssetCategoryRepository.getInstance(application.getApplicationContext());

        // Fetch LiveData from repository
        allCategories = categoryRepository.getAllCategories();
    }

    public LiveData<List<AssetCategoryEntity>> getAllCategories() {
        return allCategories;
    }

    public LiveData<AssetCategoryEntity> getCategoryById(int id) {
        return categoryRepository.getCategoryById(id);
    }

    public void insert(AssetCategoryEntity category) {
        categoryRepository.insert(category);
    }

    public void update(AssetCategoryEntity category) {
        categoryRepository.update(category);
    }

    public void delete(AssetCategoryEntity category) {
        categoryRepository.delete(category);
    }
}
