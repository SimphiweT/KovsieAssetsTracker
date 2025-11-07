package com.blackdoorsdev.kovsieassetstracker.ui.activities.startups;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Bundle;
import android.provider.Settings;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications.LoginActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.StaffDashboardActivity;
import com.google.android.material.button.MaterialButton;

import java.util.ArrayList;
import java.util.List;

public class StartupActivity extends AppCompatActivity {

    private static final int PERMISSION_REQUEST_CODE = 100;

    private MaterialButton btnLogin;
    private MaterialButton btnOffline;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_startup);


        btnLogin = findViewById(R.id.btnLogin);
        btnOffline = findViewById(R.id.btnOffline);


        btnLogin.setOnClickListener(v -> {

            startActivity(new Intent(this, LoginActivity.class));
        });

        btnOffline.setOnClickListener(v -> {
            startActivity(new Intent(this, StaffDashboardActivity.class));
        });

        // Check permissions at startup
        checkAndRequestPermissions();
    }

    // 🔹 Check and request required permissions
    private void checkAndRequestPermissions() {
        String[] permissions = {
                Manifest.permission.CAMERA,                  // QR scanning
                Manifest.permission.READ_EXTERNAL_STORAGE,   // Offline mode DB / files
                Manifest.permission.WRITE_EXTERNAL_STORAGE   // Saving reports
        };

        List<String> permissionsToRequest = new ArrayList<>();

        for (String perm : permissions) {
            if (ContextCompat.checkSelfPermission(this, perm) != PackageManager.PERMISSION_GRANTED) {
                permissionsToRequest.add(perm);
            }
        }

        if (!permissionsToRequest.isEmpty()) {
            ActivityCompat.requestPermissions(
                    this,
                    permissionsToRequest.toArray(new String[0]),
                    PERMISSION_REQUEST_CODE
            );
        }
    }

    // 🔹 Handle permission results
    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions,
                                           @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == PERMISSION_REQUEST_CODE) {
            boolean allGranted = true;

            for (int result : grantResults) {
                if (result != PackageManager.PERMISSION_GRANTED) {
                    allGranted = false;
                    break;
                }
            }

            if (!allGranted) {
                // Show dialog to guide user to settings
                new AlertDialog.Builder(this)
                        .setTitle("Permissions Required")
                        .setMessage("Kovsie Assets Tracker needs permissions for Login (online mode) and Offline Mode. Please grant them in Settings.")
                        .setPositiveButton("Open Settings", (dialog, which) -> {
                            Intent intent = new Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
                            Uri uri = Uri.fromParts("package", getPackageName(), null);
                            intent.setData(uri);
                            startActivity(intent);
                        })
                        .setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss())
                        .show();
            }
        }
    }
}
