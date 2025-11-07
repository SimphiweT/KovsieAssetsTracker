package com.blackdoorsdev.kovsieassetstracker.database.repositories;

import android.content.Context;
import android.util.Log;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.database.AppDatabase;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IAssetCategoryDao;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiCategoryService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.category.AssetCategoryResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.network.MultipartUtils;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;

import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;

public class AssetCategoryRepository {

    private static final String TAG = "AssetCategoryRepository";

    private static AssetCategoryRepository instance;
    private final IAssetCategoryDao categoryDao;
    private final ExecutorService executor;
    private final ApiCategoryService api;
    private final Context context;

    private AssetCategoryRepository(Context ctx) {
        this.context = ctx.getApplicationContext();
        AppDatabase db = MyApp.getDatabase();
        categoryDao = db.assetCategoryDao();
        executor = Executors.newSingleThreadExecutor();

        Retrofit retrofit = MyApp.getRetrofit();
        if (retrofit != null) {
            api = retrofit.create(ApiCategoryService.class);
        } else {
            final ApiCategoryService[] temp = new ApiCategoryService[1];
            RetrofitClient.initAsync(context, (r, baseUrl) -> temp[0] = r.create(ApiCategoryService.class));
            api = temp[0];
        }
    }

    public static synchronized AssetCategoryRepository getInstance(Context ctx) {
        if (instance == null) instance = new AssetCategoryRepository(ctx);
        return instance;
    }

    // --------------------------------------------------
    // LOCAL CRUD
    // --------------------------------------------------
    public void insert(AssetCategoryEntity category) {
        executor.execute(() -> categoryDao.create(category));
    }

    public void update(AssetCategoryEntity category) {
        executor.execute(() -> categoryDao.update(category));
    }

    public void delete(AssetCategoryEntity category) {
        executor.execute(() -> categoryDao.delete(category));
    }

    public LiveData<List<AssetCategoryEntity>> getAllCategories() {
        return categoryDao.getAllCategories();
    }

    public LiveData<AssetCategoryEntity> getCategoryById(int id) {
        return categoryDao.getCategoryById(id);
    }

    // --------------------------------------------------
    // UPLOAD to SERVER
    // --------------------------------------------------
    public LiveData<String> uploadCategoryToServer(AssetCategoryEntity category) {
        MutableLiveData<String> result = new MutableLiveData<>();

        if (api == null) {
            result.postValue("API not ready. Please wait...");
            return result;
        }

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        RequestBody localId = MultipartUtils.toRequestBody(category.getAssetCategoryLocalId());
        RequestBody name = MultipartUtils.toRequestBody(category.getAssetCategoryName());
        RequestBody description = MultipartUtils.toRequestBody(category.getAssetCategoryDescription());
        RequestBody isAndroid = MultipartUtils.toRequestBody(category.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(category.isServerBackedUp());
        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFile", category.getAssetCategoryImage());

        result.postValue("Uploading category " + category.getAssetCategoryName() + "...");

        api.createCategory("Bearer " + token, localId, name, description, isAndroid, isServer, imagePart)
                .enqueue(new Callback<ApiResponseDTO<AssetCategoryResponseDTO>>() {
                    @Override
                    public void onResponse(Call<ApiResponseDTO<AssetCategoryResponseDTO>> call,
                                           Response<ApiResponseDTO<AssetCategoryResponseDTO>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            category.setServerBackedUp(true);
                            update(category);
                            result.postValue(response.body().getMessage());
                        } else {
                            result.postValue("Upload failed (" + response.code() + ")");
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponseDTO<AssetCategoryResponseDTO>> call, Throwable t) {
                        if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                            RetrofitClient.handleNetworkFailure(context, () -> uploadCategoryToServer(category));
                        }
                        result.postValue("Upload failed: " + t.getMessage());
                    }
                });

        return result;
    }

    // --------------------------------------------------
    // UPDATE on SERVER
    // --------------------------------------------------
    public LiveData<String> updateCategoryOnServer(AssetCategoryEntity category) {
        MutableLiveData<String> result = new MutableLiveData<>();

        if (api == null) {
            result.postValue("API not ready. Please wait...");
            return result;
        }

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        if (category.getAssetCategoryServerId() == null || category.getAssetCategoryServerId() <= 0) {
            result.postValue("Category missing valid Server ID.");
            return result;
        }

        RequestBody serverId = MultipartUtils.toRequestBody(category.getAssetCategoryServerId());
        RequestBody localId = MultipartUtils.toRequestBody(category.getAssetCategoryLocalId());
        RequestBody name = MultipartUtils.toRequestBody(category.getAssetCategoryName());
        RequestBody description = MultipartUtils.toRequestBody(category.getAssetCategoryDescription());
        RequestBody isAndroid = MultipartUtils.toRequestBody(category.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(category.isServerBackedUp());
        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFile", category.getAssetCategoryImage());

        result.postValue("Updating category " + category.getAssetCategoryName() + "...");

        api.updateCategory("Bearer " + token, serverId, localId, name, description, isAndroid, isServer, imagePart)
                .enqueue(new Callback<ApiResponseDTO<AssetCategoryResponseDTO>>() {
                    @Override
                    public void onResponse(Call<ApiResponseDTO<AssetCategoryResponseDTO>> call,
                                           Response<ApiResponseDTO<AssetCategoryResponseDTO>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            category.setServerBackedUp(true);
                            update(category);
                            result.postValue(response.body().getMessage());
                        } else {
                            result.postValue("Update failed (" + response.code() + ")");
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponseDTO<AssetCategoryResponseDTO>> call, Throwable t) {
                        if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                            RetrofitClient.handleNetworkFailure(context, () -> updateCategoryOnServer(category));
                        }
                        result.postValue("Update failed: " + t.getMessage());
                    }
                });

        return result;
    }

    // --------------------------------------------------
    // SYNC from SERVER
    // --------------------------------------------------
    public LiveData<String> syncCategoriesFromServer() {
        MutableLiveData<String> result = new MutableLiveData<>();
        result.postValue("Syncing categories from server...");

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        executor.execute(() -> {
            try {
                Call<ApiResponseDTO<List<AssetCategoryResponseDTO>>> call =
                        api.getAllCategories("Bearer " + token);
                Response<ApiResponseDTO<List<AssetCategoryResponseDTO>>> response = call.execute();

                if (!response.isSuccessful() || response.body() == null) {
                    result.postValue("Server error (" + response.code() + ")");
                    return;
                }

                ApiResponseDTO<List<AssetCategoryResponseDTO>> body = response.body();
                if (!body.isSuccess() || body.getData() == null) {
                    result.postValue("No categories received from server.");
                    return;
                }

                int updated = 0;
                for (AssetCategoryResponseDTO dto : body.getData()) {
                    AssetCategoryEntity entity = new AssetCategoryEntity(
                            dto.getAssetCategoryServerId(),
                            dto.getAssetCategoryName(),
                            dto.getAssetCategoryDescription(),
                            dto.getAssetCategoryImage(),
                            dto.isAndroidBackedUp(),
                            dto.isServerBackedUp(),
                            new java.util.Date()
                    );
                    categoryDao.create(entity);
                    updated++;
                }

                result.postValue("Synced " + updated + " categories successfully.");

            } catch (Exception e) {
                Log.e(TAG, "Sync failed", e);
                result.postValue("Sync failed: " + e.getMessage());
            }
        });

        return result;
    }
}
