package com.blackdoorsdev.kovsieassetstracker.database.repositories;

import android.content.Context;
import android.util.Log;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.database.AppDatabase;
import com.blackdoorsdev.kovsieassetstracker.database.dao.IFloorDao;
import com.blackdoorsdev.kovsieassetstracker.database.entities.FloorEntity;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiFloorService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.floor.FloorResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.room.RoomResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import retrofit2.Call;
import retrofit2.Response;
import retrofit2.Retrofit;

public class FloorRepository {

    private static final String TAG = "FloorRepository";

    private static FloorRepository instance;
    private final IFloorDao floorDao;
    private final ExecutorService executor;
    private final ApiFloorService api;
    private final Context context;

    private FloorRepository(Context ctx) {
        this.context = ctx.getApplicationContext();
        AppDatabase db = MyApp.getDatabase();
        floorDao = db.floorDao();
        executor = Executors.newSingleThreadExecutor();

        Retrofit retrofit = MyApp.getRetrofit();
        if (retrofit != null) {
            api = retrofit.create(ApiFloorService.class);
        } else {
            final ApiFloorService[] temp = new ApiFloorService[1];
            RetrofitClient.initAsync(context, (r, baseUrl) -> temp[0] = r.create(ApiFloorService.class));
            api = temp[0];
        }
    }

    public static synchronized FloorRepository getInstance(Context ctx) {
        if (instance == null) instance = new FloorRepository(ctx);
        return instance;
    }

    // --------------------------------------------------
    // LOCAL CRUD (Optional)
    // --------------------------------------------------
    public void insert(FloorEntity floor) { executor.execute(() -> floorDao.create(floor)); }
    public void update(FloorEntity floor) { executor.execute(() -> floorDao.update(floor)); }
    public void delete(FloorEntity floor) { executor.execute(() -> floorDao.delete(floor)); }

    public LiveData<List<FloorEntity>> getAllFloors() { return floorDao.getAllFloors(); }
    public LiveData<FloorEntity> getFloorById(int id) { return floorDao.getFloorById(id); }

    // --------------------------------------------------
    // SYNC FLOORS (WITH ROOMS)
    // --------------------------------------------------
    public LiveData<String> syncFloorsFromServer() {
        MutableLiveData<String> result = new MutableLiveData<>();
        result.postValue("Syncing floors from server...");

        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        if (token == null || token.isEmpty()) {
            result.postValue("Session expired. Please login again.");
            return result;
        }

        executor.execute(() -> {
            try {
                Call<ApiResponseDTO<List<FloorResponseDTO>>> call =
                        api.getAllFloorsWithRooms("Bearer " + token);
                Response<ApiResponseDTO<List<FloorResponseDTO>>> response = call.execute();

                if (!response.isSuccessful() || response.body() == null) {
                    result.postValue("Server error (" + response.code() + ")");
                    return;
                }

                ApiResponseDTO<List<FloorResponseDTO>> body = response.body();
                if (!body.isSuccess() || body.getData() == null) {
                    result.postValue("No floors received from server.");
                    return;
                }

                List<FloorEntity> floorEntities = new ArrayList<>();
                int updated = 0;

                for (FloorResponseDTO dto : body.getData()) {
                    FloorEntity floor = new FloorEntity(
                            dto.floorServerId,
                            dto.floorName,
                            dto.floorDescription,
                            dto.isAndroidBackedUp,
                            dto.isServerBackedUp,
                            dto.createdDate,
                            dto.updatedDate,
                            dto.buildingServerId
                    );

                    // Map rooms if needed (optional local cache)
                    if (dto.rooms != null) {
                        for (RoomResponseDTO r : dto.rooms) {
                            Log.d(TAG, "Room: " + r.roomName + " (FloorId: " + dto.floorServerId + ")");
                        }
                    }

                    floorEntities.add(floor);
                    updated++;
                }

                floorDao.deleteAll();
                floorDao.insertAll(floorEntities);

                result.postValue("Synced " + updated + " floors successfully.");

            } catch (Exception e) {
                Log.e(TAG, "Sync failed", e);
                result.postValue("Sync failed: " + e.getMessage());
            }
        });

        return result;
    }
}
