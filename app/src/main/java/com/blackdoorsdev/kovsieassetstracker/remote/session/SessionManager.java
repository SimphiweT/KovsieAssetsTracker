package com.blackdoorsdev.kovsieassetstracker.remote.session;

import android.content.Context;
import android.content.SharedPreferences;

public class SessionManager {
    private static final String PREF_NAME = "kovsie_session";
    private static final String KEY_USER_ID = "user_id";
    private static final String KEY_USER_DISPLAY_NAME = "display_name";
    private static final String KEY_USER_EMAIL = "user_email";
    private static final String KEY_ROLE = "role";
    private static final String KEY_PROFILE_URL = "profile_url";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_TOKEN_EXPIRATION_TIME = "expiration_time";
    private static final String KEY_SERVER_IP = "server_ip";
    private final SharedPreferences prefs;

    public SessionManager(Context context) {
        prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
    }

    public void saveSession(String userId, String displayName, String email, String role,
                            String profileUrl, String token, String expirationTime) {
        prefs.edit()
                .putString(KEY_USER_ID, userId)
                .putString(KEY_USER_DISPLAY_NAME, displayName)
                .putString(KEY_USER_EMAIL, email)
                .putString(KEY_ROLE, role)
                .putString(KEY_PROFILE_URL, profileUrl)
                .putString(KEY_TOKEN, token)
                .putString(KEY_TOKEN_EXPIRATION_TIME, expirationTime)
                .apply();
    }

    public String getUserId() { return prefs.getString(KEY_USER_ID, ""); }
    public String getDisplayName() { return prefs.getString(KEY_USER_DISPLAY_NAME, ""); }
    public String getEmail() { return prefs.getString(KEY_USER_EMAIL, ""); }
    public String getRole() { return prefs.getString(KEY_ROLE, ""); }
    public String getProfileUrl() { return prefs.getString(KEY_PROFILE_URL, ""); }
    public String getToken() { return prefs.getString(KEY_TOKEN, ""); }
    public String getExpirationTime() { return prefs.getString(KEY_TOKEN_EXPIRATION_TIME, ""); }
    public void saveNetworkAddress(String ip) {
        prefs.edit().putString(KEY_SERVER_IP, ip).apply();
    }

    public String getNetworkAddress() {
        return prefs.getString(KEY_SERVER_IP, "N/A");
    }

    public boolean isLoggedIn() { return !getToken().isEmpty(); }
    public void clearToken() {
        prefs.edit().remove(KEY_TOKEN).apply();
    }

    public void clearSession() { prefs.edit().clear().apply(); }
    public void clearAllSessionsAndCache(Context context) {
        // Clear session info
        clearSession();

        // Also clear Retrofit cached IP
        com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient.clearCache(context);
    }



}
