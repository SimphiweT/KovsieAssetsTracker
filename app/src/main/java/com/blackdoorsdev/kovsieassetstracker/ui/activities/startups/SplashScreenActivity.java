package com.blackdoorsdev.kovsieassetstracker.ui.activities.startups;

import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications.LoginActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.StaffDashboardActivity;

public class SplashScreenActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_splash_screen);

        new Handler().postDelayed(() -> {
            SessionManager session = new SessionManager(SplashScreenActivity.this);

            String token = session.getToken();
            boolean hasNetwork = com.blackdoorsdev.kovsieassetstracker.remote.network.NetworkUtils.isNetworkAvailable(this);

            if (hasNetwork && token != null && !token.isEmpty()) {
                // Logged in and online
                startActivity(new Intent(SplashScreenActivity.this, StaffDashboardActivity.class));
            } else {
                // Token null or offline → reset
                session.clearToken();
                startActivity(new Intent(SplashScreenActivity.this, StartupActivity.class));
            }

            finish();
        }, 3000);


    }
}