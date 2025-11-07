package com.blackdoorsdev.kovsieassetstracker.database.repositories;

import android.content.Context;
import android.util.Log;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.database.AppDatabase;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IRoomDao;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiRoomService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.room.RoomResponseDTO;
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

public class RoomRepository {

    private static final String TAG = "RoomRepository";

    private static RoomRepository instance;
    private final IRoomDao roomDao;
    private final ExecutorService executor;
    private final ApiRoomService api;
    private final Context context;

    private RoomRepository(Context ctx) {
        this.context = ctx.getApplicationContext();
        AppDatabase db = MyApp.getDatabase();
        roomDao = db.roomDao();
        executor = Executors.newSingleThreadExecutor();

        Retrofit retrofit = MyApp.getRetrofit();
        if (retrofit != null) {
            api = retrofit.create(ApiRoomService.class);
        } else {
            final ApiRoomService[] temp = new ApiRoomService[1];
            RetrofitClient.initAsync(context, (r, baseUrl) -> temp[0] = r.create(ApiRoomService.class));
            api = temp[0];
        }
    }

    public static synchronized RoomRepository getInstance(Context ctx) {
        if (instance == null) instance = new RoomRepository(ctx);
        return instance;
    }

    // --------------------------------------------------
    // LOCAL CRUD
    // --------------------------------------------------
    public void insert(RoomEntity room) {
        executor.execute(() -> roomDao.create(room));
    }

    public void update(RoomEntity room) {
        executor.execute(() -> roomDao.update(room));
    }

    public void delete(RoomEntity room) {
        executor.execute(() -> roomDao.delete(room));
    }

    public LiveData<List<RoomEntity>> getAllRooms() {
        return roomDao.getAllRooms();
    }

    public LiveData<RoomEntity> getRoomById(int id) {
        return roomDao.getRoomById(id);
    }

    // --------------------------------------------------
    // UPLOAD ROOM TO SERVER
    // --------------------------------------------------
    public LiveData<String> uploadRoomToServer(RoomEntity room) {
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

        RequestBody localId = MultipartUtils.toRequestBody(room.getRoomLocalId());
        RequestBody name = MultipartUtils.toRequestBody(room.getRoomName());
        RequestBody desc = MultipartUtils.toRequestBody(room.getRoomDescription());
        RequestBody isAndroid = MultipartUtils.toRequestBody(room.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(room.isServerBackedUp());
        RequestBody floorServerId = MultipartUtils.toRequestBody(room.getFloorServerId());
        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFile", room.getRoomImage());

        result.postValue("Uploading room " + room.getRoomName() + "...");

        api.createRoom(
                "Bearer " + token,
                localId, name, desc, imagePart, isAndroid, isServer, floorServerId
        ).enqueue(new Callback<ApiResponseDTO<RoomResponseDTO>>() {
            @Override
            public void onResponse(Call<ApiResponseDTO<RoomResponseDTO>> call,
                                   Response<ApiResponseDTO<RoomResponseDTO>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    RoomResponseDTO dto = response.body().getData();
                    room.setRoomServerId(dto.roomServerId);
                    room.setServerBackedUp(true);
                    update(room);
                    result.postValue("Room uploaded successfully.");
                } else {
                    Log.e(TAG, "Upload failed: " + response.code() + " " + response.message());
                    result.postValue("Upload failed (" + response.code() + ")");
                }
            }

            @Override
            public void onFailure(Call<ApiResponseDTO<RoomResponseDTO>> call, Throwable t) {
                Log.e(TAG, "Upload failed: ", t);
                if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                    RetrofitClient.handleNetworkFailure(context, () -> uploadRoomToServer(room));
                }
                result.postValue("Upload failed: " + t.getMessage());
            }
        });

        return result;
    }

    // --------------------------------------------------
    // UPDATE ROOM ON SERVER
    // --------------------------------------------------
    public LiveData<String> updateRoomOnServer(RoomEntity room) {
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

        if (room.getRoomServerId() == null || room.getRoomServerId() <= 0) {
            result.postValue("Room missing valid Server ID.");
            return result;
        }

        RequestBody serverId = MultipartUtils.toRequestBody(room.getRoomServerId());
        RequestBody localId = MultipartUtils.toRequestBody(room.getRoomLocalId());
        RequestBody name = MultipartUtils.toRequestBody(room.getRoomName());
        RequestBody desc = MultipartUtils.toRequestBody(room.getRoomDescription());
        RequestBody isAndroid = MultipartUtils.toRequestBody(room.isAndroidBackedUp());
        RequestBody isServer = MultipartUtils.toRequestBody(room.isServerBackedUp());
        RequestBody floorServerId = MultipartUtils.toRequestBody(room.getFloorServerId());
        MultipartBody.Part imagePart = MultipartUtils.prepareFilePart("ImageFile", room.getRoomImage());

        result.postValue("Updating room " + room.getRoomName() + "...");

        api.updateRoom(
                "Bearer " + token,
                serverId, localId, name, desc, imagePart, isAndroid, isServer, floorServerId
        ).enqueue(new Callback<ApiResponseDTO<RoomResponseDTO>>() {
            @Override
            public void onResponse(Call<ApiResponseDTO<RoomResponseDTO>> call,
                                   Response<ApiResponseDTO<RoomResponseDTO>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    room.setServerBackedUp(true);
                    update(room);
                    result.postValue("Room updated successfully.");
                } else {
                    Log.e(TAG, "Update failed: " + response.code() + " " + response.message());
                    result.postValue("Update failed (" + response.code() + ")");
                }
            }

            @Override
            public void onFailure(Call<ApiResponseDTO<RoomResponseDTO>> call, Throwable t) {
                Log.e(TAG, "Update failed: ", t);
                if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                    RetrofitClient.handleNetworkFailure(context, () -> updateRoomOnServer(room));
                }
                result.postValue("Update failed: " + t.getMessage());
            }
        });

        return result;
    }

    // --------------------------------------------------
    // SYNC ROOMS FROM SERVER
    // --------------------------------------------------
    public LiveData<String> syncRoomsFromServer() {
        MutableLiveData<String> result = new MutableLiveData<>();
        result.postValue("Syncing rooms from server...");

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        executor.execute(() -> {
            try {
                Call<ApiResponseDTO<List<RoomResponseDTO>>> call = api.getAllRooms("Bearer " + token);
                Response<ApiResponseDTO<List<RoomResponseDTO>>> response = call.execute();

                if (!response.isSuccessful() || response.body() == null) {
                    result.postValue("Server error (" + response.code() + ")");
                    return;
                }

                ApiResponseDTO<List<RoomResponseDTO>> body = response.body();
                if (!body.isSuccess() || body.getData() == null) {
                    result.postValue("No rooms received from server.");
                    return;
                }

                int updated = 0;
                for (RoomResponseDTO dto : body.getData()) {
                    RoomEntity entity = new RoomEntity(
                            null,
                            dto.roomServerId,
                            dto.roomName,
                            dto.roomDescription,
                            dto.roomImage,
                            dto.isAndroidBackedUp,
                            dto.isServerBackedUp,
                            dto.createdDate,
                            dto.updatedDate
                    );
                    entity.setFloorServerId(dto.floorServerId);
                    roomDao.create(entity);
                    updated++;
                }

                result.postValue("Synced " + updated + " rooms successfully.");

            } catch (Exception e) {
                Log.e(TAG, "Sync failed", e);
                result.postValue("Sync failed: " + e.getMessage());
            }
        });

        return result;
    }
}
