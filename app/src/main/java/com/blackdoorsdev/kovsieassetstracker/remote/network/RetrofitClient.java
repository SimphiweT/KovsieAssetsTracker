package com.blackdoorsdev.kovsieassetstracker.remote.network;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.Toast;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializer;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.URL;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * RetrofitClient with auto-retry, smart IP detection, and tolerant Date parser for .NET timestamps.
 */
public class RetrofitClient {

    private static final String TAG = "RetrofitClient";
    private static final String PREF_NAME = "kovsie_network_prefs";
    private static final String KEY_BASE_URL = "base_url";

    private static volatile Retrofit retrofit;
    private static String lastConnectedIp = null;

    private static final ExecutorService executor = Executors.newSingleThreadExecutor();
    private static final Handler mainHandler = new Handler(Looper.getMainLooper());

    public interface OnBaseUrlDetectedListener {
        void onBaseUrlReady(Retrofit retrofit, String baseUrl);
    }

    // === Public API ===
    public static void initAsync(Context context, OnBaseUrlDetectedListener listener) {
        SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);

        executor.execute(() -> {
            String cachedUrl = prefs.getString(KEY_BASE_URL, null);
            String activeUrl = null;

            if (cachedUrl != null && isReachable(cachedUrl)) {
                activeUrl = cachedUrl;
                Log.i(TAG, "Using cached base URL: " + cachedUrl);
            } else {
                if (cachedUrl != null) {
                    prefs.edit().remove(KEY_BASE_URL).apply();
                    Log.w(TAG, "Cached base URL invalid — clearing cache...");
                }
                activeUrl = detectAndValidateBaseUrl();
            }

            final String finalUrl = activeUrl;

            mainHandler.post(() -> {
                if (finalUrl == null) {
                    Toast.makeText(context, "Could not reach API server.", Toast.LENGTH_LONG).show();
                    return;
                }

                prefs.edit().putString(KEY_BASE_URL, finalUrl).apply();
                retrofit = buildRetrofit(finalUrl);

                Toast.makeText(context, "Connected → " + finalUrl, Toast.LENGTH_SHORT).show();
                Log.i(TAG, "Connected to API base URL: " + finalUrl);

                if (context instanceof android.app.Activity) {
                    com.blackdoorsdev.kovsieassetstracker.helpers.DebugOverlay.show(
                            (android.app.Activity) context, finalUrl
                    );
                }

                listener.onBaseUrlReady(retrofit, finalUrl);
            });
        });
    }

    /** Called when a Retrofit request fails due to unreachable host. */
    public static void handleNetworkFailure(Context context, Runnable onRetryReady) {
        executor.execute(() -> {
            String newUrl = detectAndValidateBaseUrl();
            if (newUrl != null) {
                retrofit = buildRetrofit(newUrl);
                mainHandler.post(() -> {
                    Toast.makeText(context, "Connection restored.", Toast.LENGTH_SHORT).show();

                    if (context instanceof android.app.Activity) {
                        com.blackdoorsdev.kovsieassetstracker.helpers.DebugOverlay.show(
                                (android.app.Activity) context, newUrl
                        );
                    }

                    onRetryReady.run();
                });

            } else {
                mainHandler.post(() ->
                        Toast.makeText(context, "Still unable to reach server.", Toast.LENGTH_LONG).show());
            }
        });
    }

    public static Retrofit getClient() {
        return retrofit;
    }

    public static void clearCache(Context context) {
        context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)
                .edit().remove(KEY_BASE_URL).apply();
        lastConnectedIp = null;
    }

    // === Core Detection ===
    private static String detectAndValidateBaseUrl() {
        final List<String> candidates = new ArrayList<>();

        // Emulator & common LAN candidates
        final String EMULATOR_URL = "http://10.0.2.2:5250/api/";
        candidates.add(EMULATOR_URL);
        candidates.add("http://196.255.132.200:5250/api/"); //UFS_Wireless1
        candidates.add("http://196.255.135.226:5250/api/"); //UFS_Wireless1 My Android
        candidates.add("http://192.168.18.56:5250/api/");
        candidates.add("http://192.168.18.4:5250/api/");
        candidates.add("http://192.168.1.100:5250/api/");
        candidates.add("http://10.246.7.120:5250/api/"); //hotspot
        candidates.add("http://localhost:5250/api/");
        candidates.add("http://192.168.21.93:5250/api/");//UFS_Guest
        candidates.add("http://192.168.18.27:5250/api/");//UFS_Guest My Android

        try {
            boolean isEmu =
                    Build.FINGERPRINT.startsWith("generic") ||
                            Build.HARDWARE.contains("ranchu") ||
                            Build.PRODUCT.contains("sdk") ||
                            Build.MANUFACTURER.contains("Google") ||
                            Build.MODEL.toLowerCase().contains("emulator");

            if (isEmu) {
                Log.d(TAG, "Emulator mode → 10.0.2.2 will be tested FIRST");
            }

            for (NetworkInterface ni : Collections.list(NetworkInterface.getNetworkInterfaces())) {
                for (InetAddress addr : Collections.list(ni.getInetAddresses())) {
                    if (!addr.isLoopbackAddress()
                            && addr.isSiteLocalAddress()
                            && addr.getHostAddress().indexOf(':') < 0) {
                        String candidate = "http://" + addr.getHostAddress() + ":5250/api/";
                        if (!candidates.contains(candidate)) candidates.add(candidate);
                        Log.d(TAG, "Detected local IPv4: " + candidate);
                    }
                }
            }

        } catch (Exception e) {
            Log.e(TAG, "Network detection error: " + e.getMessage());
        }

        for (String base : candidates) {
            Log.d(TAG, "Probing " + base);
            if (isReachable(base)) {
                Log.i(TAG, "Connected successfully: " + base);
                cacheNetworkIp(base);
                return base;
            }
        }

        Log.w(TAG, "All servers unreachable after testing " + candidates.size() + " candidates.");
        return null;
    }

    private static boolean isReachable(String baseUrl) {
        try {
            URL url = new URL(baseUrl + "Health/ping");
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(4000);
            conn.setReadTimeout(4000);
            conn.setRequestMethod("GET");
            int code = conn.getResponseCode();
            conn.disconnect();
            boolean ok = (code >= 200 && code < 400);
            Log.d(TAG, "Checked " + url + " -> " + ok + " (HTTP " + code + ")");
            return ok;
        } catch (IOException e) {
            Log.d(TAG, "Checked " + baseUrl + " -> false (" + e.getMessage() + ")");
            return false;
        }
    }

    // === Retrofit Builder with custom Gson for .NET Dates ===
    private static Retrofit buildRetrofit(String baseUrl) {
        HttpLoggingInterceptor interceptor = new HttpLoggingInterceptor(msg -> Log.d("OkHttp", msg));
        interceptor.setLevel(HttpLoggingInterceptor.Level.BASIC);

        OkHttpClient client = new OkHttpClient.Builder()
                .retryOnConnectionFailure(true)
                .connectTimeout(5, TimeUnit.SECONDS)
                .readTimeout(5, TimeUnit.SECONDS)
                .writeTimeout(5, TimeUnit.SECONDS)
                .addInterceptor(interceptor)
                .build();

        // Custom Gson handles "2025-10-28T08:23:18.0887169"
        JsonDeserializer<Date> safeDateDeserializer = (json, typeOfT, context) -> {
            String value = json.getAsString();
            if (value == null || value.trim().isEmpty()) return null;
            List<String> patterns = List.of(
                    "yyyy-MM-dd'T'HH:mm:ss.SSSSSSS",
                    "yyyy-MM-dd'T'HH:mm:ss.SSS",
                    "yyyy-MM-dd'T'HH:mm:ss"
            );
            for (String pattern : patterns) {
                try {
                    return new SimpleDateFormat(pattern, Locale.getDefault()).parse(value);
                } catch (ParseException ignored) {}
            }
            // Last fallback
            try {
                return DateFormat.getDateTimeInstance().parse(value);
            } catch (Exception e) {
                Log.w(TAG, "Failed to parse date: " + value);
                return null;
            }
        };

        Gson gson = new GsonBuilder()
                .registerTypeAdapter(Date.class, safeDateDeserializer)
                .create();

        return new Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build();
    }

    private static void cacheNetworkIp(String base) {
        try {
            String ip = base.replace("http://", "").split(":")[0];
            if (!ip.equals(lastConnectedIp)) {
                lastConnectedIp = ip;
                new SessionManager(MyApp.getAppContext()).saveNetworkAddress(ip);
                Log.i(TAG, "Saved network IP: " + ip);
            }
        } catch (Exception e) {
            Log.e(TAG, "Failed to cache IP: " + e.getMessage());
        }
    }
}
