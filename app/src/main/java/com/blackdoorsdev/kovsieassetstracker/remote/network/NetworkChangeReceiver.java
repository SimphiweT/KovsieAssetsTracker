package com.blackdoorsdev.kovsieassetstracker.remote.network;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.util.Log;

public class NetworkChangeReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        ConnectivityManager cm = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
        NetworkInfo active = cm.getActiveNetworkInfo();

        if (active != null && active.isConnected()) {
            Log.i("NetworkChangeReceiver", "Network connected → revalidating API base URL...");
            RetrofitClient.clearCache(context);
            RetrofitClient.initAsync(context, (retrofit, baseUrl) ->
                    Log.i("NetworkChangeReceiver", "Connected to " + baseUrl));
        } else {
            Log.w("NetworkChangeReceiver", "Network disconnected");
        }
    }
}
