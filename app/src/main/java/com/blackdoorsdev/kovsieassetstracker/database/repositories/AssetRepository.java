package com.blackdoorsdev.kovsieassetstracker.database.repositories;

import android.content.Context;
import android.util.Log;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.database.AppDatabase;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IAssetDao;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiAssetsService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.asset.AssetResponseDTO;
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

public class AssetRepository {
    private static final String TAG = "AssetRepository";

    private static volatile AssetRepository instance;
    private final IAssetDao assetDao;
    private final ExecutorService executor;
    private final Context context;
    private final ApiAssetsService api;

    private AssetRepository(Context ctx) {
        this.context = ctx.getApplicationContext();
        AppDatabase db = MyApp.getDatabase();
        assetDao = db.assetDao();
        executor = Executors.newSingleThreadExecutor();

        Retrofit retrofit = MyApp.getRetrofit();
        if (retrofit != null) {
            api = retrofit.create(ApiAssetsService.class);
        } else {
            final ApiAssetsService[] temp = new ApiAssetsService[1];
            RetrofitClient.initAsync(context, (r, baseUrl) -> temp[0] = r.create(ApiAssetsService.class));
            api = temp[0];
        }
    }

    public static synchronized AssetRepository getInstance(Context ctx) {
        if (instance == null) {
            instance = new AssetRepository(ctx);
        }
        return instance;
    }

    // === LOCAL CRUD ===
    public void insert(AssetEntity asset) {
        executor.execute(() -> assetDao.create(asset));
    }

    public void update(AssetEntity asset) {
        executor.execute(() -> assetDao.update(asset));
    }

    public void delete(AssetEntity asset) {
        executor.execute(() -> assetDao.delete(asset));
    }

    public LiveData<List<AssetEntity>> getAllAssets() {
        return assetDao.getAllAssets();
    }

    public LiveData<AssetEntity> getAssetById(int id) {
        return assetDao.getAssetById(id);
    }

    // === CREATE to Server ===
    public LiveData<String> uploadAssetToServer(AssetEntity asset) {
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

        RequestBody localId = MultipartUtils.toRequestBody(String.valueOf(asset.getAssetLocalID()));
        RequestBody name = MultipartUtils.toRequestBody(asset.getAssetName());
        RequestBody description = MultipartUtils.toRequestBody(asset.getAssetDescription());
        RequestBody qrSecret = MultipartUtils.toRequestBody(asset.getAssetQrCodeSecretText());
        RequestBody status = MultipartUtils.toRequestBody(asset.getAssetStatus() != null ? asset.getAssetStatus() : "Available");
        RequestBody roomId = MultipartUtils.toRequestBody(asset.getRoomLocalId() != null ? String.valueOf(asset.getRoomLocalId()) : "0");
        RequestBody catId = MultipartUtils.toRequestBody(asset.getAssetCategoryLocalId() != null ? String.valueOf(asset.getAssetCategoryLocalId()) : "0");
        RequestBody isAndroid = MultipartUtils.toRequestBody(asset.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(asset.isServerBackedUp());

        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFileAsset", asset.getAssetImage());
        MultipartBody.Part qrPart = MultipartUtils.prepareFilePart("ImageFileQrCode", asset.getAssetQrCodeImage());

        result.postValue("Uploading " + asset.getAssetName() + "...");

        api.createAsset("Bearer " + token, localId, name, description, qrSecret, status,
                        isAndroid, isServer, roomId, catId, imagePart, qrPart)
                .enqueue(new Callback<ApiResponseDTO<AssetResponseDTO>>() {
                    @Override
                    public void onResponse(Call<ApiResponseDTO<AssetResponseDTO>> call, Response<ApiResponseDTO<AssetResponseDTO>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            asset.setServerBackedUp(true);
                            update(asset);
                            result.postValue(response.body().getMessage());
                        } else {
                            result.postValue("Upload failed (" + response.code() + ")");
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponseDTO<AssetResponseDTO>> call, Throwable t) {
                        if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                            RetrofitClient.handleNetworkFailure(context, () -> uploadAssetToServer(asset));
                        }
                        result.postValue("Upload failed: " + t.getMessage());
                    }
                });

        return result;
    }

    // === UPDATE to Server ===
    public LiveData<String> updateAssetOnServer(AssetEntity asset) {
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

        if (asset.getAssetServerId() == null || asset.getAssetServerId() <= 0) {
            result.postValue("Invalid Server ID.");
            return result;
        }

        RequestBody name = MultipartUtils.toRequestBody(asset.getAssetName());
        RequestBody description = MultipartUtils.toRequestBody(asset.getAssetDescription());
        RequestBody localId = MultipartUtils.toRequestBody(String.valueOf(asset.getAssetLocalID()));
        RequestBody qrSecret = MultipartUtils.toRequestBody(asset.getAssetQrCodeSecretText());
        RequestBody status = MultipartUtils.toRequestBody(asset.getAssetStatus() != null ? asset.getAssetStatus() : "Available");
        RequestBody isAndroid = MultipartUtils.toRequestBody(asset.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(asset.isServerBackedUp());
        RequestBody roomId = MultipartUtils.toRequestBody(asset.getRoomLocalId() != null ? String.valueOf(asset.getRoomLocalId()) : "0");
        RequestBody catId = MultipartUtils.toRequestBody(asset.getAssetCategoryLocalId() != null ? String.valueOf(asset.getAssetCategoryLocalId()) : "0");
        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFileAsset", asset.getAssetImage());
        MultipartBody.Part qrPart = MultipartUtils.prepareFilePart("ImageFileQrCode", asset.getAssetQrCodeImage());

        result.postValue("Updating " + asset.getAssetName() + "...");

        api.updateAsset("Bearer " + token, asset.getAssetServerId(), localId, name, description,
                        qrSecret, status, isAndroid, isServer, roomId, catId, imagePart, qrPart)
                .enqueue(new Callback<ApiResponseDTO<AssetResponseDTO>>() {
                    @Override
                    public void onResponse(Call<ApiResponseDTO<AssetResponseDTO>> call, Response<ApiResponseDTO<AssetResponseDTO>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            asset.setServerBackedUp(true);
                            update(asset);
                            result.postValue(response.body().getMessage());
                        } else {
                            result.postValue("Update failed (" + response.code() + ")");
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponseDTO<AssetResponseDTO>> call, Throwable t) {
                        if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                            RetrofitClient.handleNetworkFailure(context, () -> updateAssetOnServer(asset));
                        }
                        result.postValue("Update failed: " + t.getMessage());
                    }
                });

        return result;
    }

    // === SYNC from Server ===
    public LiveData<String> syncAssetsFromServer() {
        MutableLiveData<String> result = new MutableLiveData<>();
        result.postValue("Syncing assets...");

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        executor.execute(() -> {
            try {
                Call<ApiResponseDTO<List<AssetResponseDTO>>> call = api.getAllAssets("Bearer " + token);
                Response<ApiResponseDTO<List<AssetResponseDTO>>> response = call.execute();

                if (!response.isSuccessful() || response.body() == null) {
                    result.postValue("Server error (" + response.code() + ")");
                    return;
                }

                ApiResponseDTO<List<AssetResponseDTO>> body = response.body();
                if (!body.isSuccess() || body.getData() == null) {
                    result.postValue("No assets found.");
                    return;
                }

                List<AssetResponseDTO> remoteAssets = body.getData();
                for (AssetResponseDTO remote : remoteAssets) {
                    AssetEntity entity = new AssetEntity(
                            remote.getAssetServerId(),
                            remote.getAssetName(),
                            remote.getAssetDescription(),
                            remote.getAssetImage(),
                            remote.getAssetQRCodeImage(),
                            remote.getQRCodeSecretText(),
                            remote.getStatus(),
                            remote.isAndroidBackedUp(),
                            remote.isServerBackedUp(),
                            remote.getUpdatedDate() != null ? remote.getUpdatedDate() : remote.getCreatedDate(),
                            remote.getAssetCategoryServerId(),
                            remote.getRoomServerId()
                    );
                    assetDao.create(entity);
                }

                result.postValue("Sync complete: " + remoteAssets.size() + " assets.");

            } catch (Exception e) {
                Log.e(TAG, "Sync failed", e);
                result.postValue("Sync failed: " + e.getMessage());
            }
        });

        return result;
    }
}
