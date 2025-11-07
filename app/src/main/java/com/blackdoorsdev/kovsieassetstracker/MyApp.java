package com.blackdoorsdev.kovsieassetstracker;

import android.app.Application;
import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.os.StrictMode;
import android.util.Log;

import androidx.room.Room;

import com.blackdoorsdev.kovsieassetstracker.database.AppDatabase;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;

import retrofit2.Retrofit;

public class MyApp extends Application {

    private static MyApp instance;
    private static AppDatabase database;
    private static Retrofit retrofit;

    @Override
    public void onCreate() {
        super.onCreate();
        instance = this;

        // === Initialize Room Database ===
        database = Room.databaseBuilder(
                        getApplicationContext(),
                        AppDatabase.class,
                        "KovsieAssetsTracker_Local_DB"
                )
                .fallbackToDestructiveMigration()
                .allowMainThreadQueries()
                .build();

        // === Initialize Retrofit Asynchronously ===
        RetrofitClient.initAsync(getApplicationContext(), (r, baseUrl) -> {
            retrofit = r;
            Log.i("MyApp", "Retrofit initialized at " + baseUrl);
        });

        // === Enable StrictMode ONLY in Debug Builds ===
//        if (isDebugBuild()) {
//            StrictMode.setThreadPolicy(
//                    new StrictMode.ThreadPolicy.Builder()
//                            .detectAll()
//                            .penaltyLog()
//                            .build()
//            );
//            Log.d("MyApp", "StrictMode enabled (Debug Build)");
//        } else {
//            Log.d("MyApp", "StrictMode disabled (Release Build)");
//        }
    }

    /** Safely detects if this is a Debug build without needing BuildConfig */
    private boolean isDebugBuild() {
        return (getApplicationInfo().flags & ApplicationInfo.FLAG_DEBUGGABLE) != 0;
    }

    // === Global Getters ===
    public static Context getAppContext() {
        return instance.getApplicationContext();
    }

    public static AppDatabase getDatabase() {
        return database;
    }

    public static Retrofit getRetrofit() {
        return retrofit;
    }
}
