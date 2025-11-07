package com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications;

import static androidx.constraintlayout.helper.widget.MotionEffect.TAG;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.helpers.DebugOverlay;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiAuthService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.LoginRequestDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.LoginResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.StaffDashboardActivity;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;

public class LoginActivity extends AppCompatActivity {

    private MaterialButton btnConfirmLogin, btnRequestAccess;
    private TextInputEditText etEmail, etPassword;
    private ApiAuthService api;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        btnConfirmLogin = findViewById(R.id.btnConfirmLogin);
        btnRequestAccess = findViewById(R.id.btnRequestAccess);
        etEmail = findViewById(R.id.txtLoginEmail);
        etPassword = findViewById(R.id.txtLoginPassword);

        SessionManager session = new SessionManager(this);
        if (!session.isLoggedIn()) session.clearAllSessionsAndCache(this);

        // === Initialize API from global Retrofit ===
        Retrofit retrofit = MyApp.getRetrofit();
        if (retrofit != null) {
            api = retrofit.create(ApiAuthService.class);
            DebugOverlay.show(this, retrofit.baseUrl().toString());
        } else {
            // Fallback async detection with overlay when done
            RetrofitClient.initAsync(this, (r, baseUrl) -> {
                api = r.create(ApiAuthService.class);
                Log.i(TAG, "Using base URL: " + baseUrl);
                DebugOverlay.show(this, baseUrl);
            });
        }

        btnConfirmLogin.setOnClickListener(v -> performLogin());
        btnRequestAccess.setOnClickListener(v ->
                startActivity(new Intent(LoginActivity.this, RequestAccessActivity.class)));
    }

    private void performLogin() {
        if (api == null) {
            Toast.makeText(this, "Please wait... detecting server.", Toast.LENGTH_SHORT).show();
            return;
        }

        String email = etEmail.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (email.isEmpty() || password.isEmpty()) {
            Toast.makeText(this, "Enter email and password.", Toast.LENGTH_SHORT).show();
            return;
        }

        Toast.makeText(this, "Signing in...", Toast.LENGTH_SHORT).show();

        api.login(new LoginRequestDTO(email, password)).enqueue(new Callback<ApiResponseDTO<LoginResponseDTO>>() {
            @Override
            public void onResponse(Call<ApiResponseDTO<LoginResponseDTO>> call,
                                   Response<ApiResponseDTO<LoginResponseDTO>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    LoginResponseDTO data = response.body().getData();

                    if (data == null || !"StaffMember".equalsIgnoreCase(data.getUserRole())) {
                        Toast.makeText(LoginActivity.this, "Access denied. Only staff allowed.", Toast.LENGTH_LONG).show();
                        return;
                    }

                    new SessionManager(LoginActivity.this).saveSession(
                            data.getUserId(),
                            data.getUserDisplayName(),
                            data.getUserEmail(),
                            data.getUserRole(),
                            data.getProfilePictureUrl(),
                            data.getToken(),
                            data.getTokenExpirationTime().toString()
                    );

                    Toast.makeText(LoginActivity.this, "Welcome " + data.getUserDisplayName(), Toast.LENGTH_SHORT).show();
                    startActivity(new Intent(LoginActivity.this, StaffDashboardActivity.class));
                    finish();
                } else {
                    Toast.makeText(LoginActivity.this, "Invalid credentials or server error.", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponseDTO<LoginResponseDTO>> call, Throwable t) {
                if (t.getMessage() != null && t.getMessage().contains("Failed to connect")) {
                    RetrofitClient.handleNetworkFailure(LoginActivity.this, () -> {
                        DebugOverlay.show(LoginActivity.this, "Reconnected to server");
                        performLogin();
                    });
                } else {
                    Toast.makeText(LoginActivity.this, "Login failed: " + t.getMessage(), Toast.LENGTH_LONG).show();
                }
            }
        });
    }
}
