package com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.airbnb.lottie.LottieAnimationView;
import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiAuthService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;

public class ResetPasswordActivity extends AppCompatActivity {

    private TextInputEditText etEmail;
    private MaterialButton btnResetPassword, btnBackLogin;
    private LottieAnimationView lottieReset;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_reset_password);

        etEmail = findViewById(R.id.etEmail);
        btnResetPassword = findViewById(R.id.btnResetPassword);
        btnBackLogin = findViewById(R.id.btnBackLogin);
        lottieReset = findViewById(R.id.lottieReset);

        btnResetPassword.setOnClickListener(v -> handlePasswordReset());
        btnBackLogin.setOnClickListener(v -> {
            startActivity(new Intent(this, LoginActivity.class));
            finish();
        });
    }

    private void handlePasswordReset() {
        String email = etEmail.getText() != null ? etEmail.getText().toString().trim() : "";

        if (email.isEmpty()) {
            Toast.makeText(this, "Please enter your email", Toast.LENGTH_SHORT).show();
            return;
        }

        ProgressDialog progressDialog = new ProgressDialog(this);
        progressDialog.setMessage("Sending reset link...");
        progressDialog.setCancelable(false);
        progressDialog.show();

        Retrofit retrofit = RetrofitClient.getClient();
        ApiAuthService api = retrofit.create(ApiAuthService.class);

        // Create simple JSON body
        com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.ForgotPasswordRequestDTO dto =
                new com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.ForgotPasswordRequestDTO(email);

        Call<ApiResponseDTO<String>> call = api.forgotPassword(dto);

        call.enqueue(new Callback<ApiResponseDTO<String>>() {
            @Override
            public void onResponse(Call<ApiResponseDTO<String>> call, Response<ApiResponseDTO<String>> response) {
                progressDialog.dismiss();
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponseDTO<String> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        Toast.makeText(
                                ResetPasswordActivity.this,
                                apiResponse.getMessage() != null
                                        ? apiResponse.getMessage()
                                        : "If an account exists, a reset link has been sent to your email.",
                                Toast.LENGTH_LONG
                        ).show();
                        etEmail.setText("");
                    } else {
                        Toast.makeText(
                                ResetPasswordActivity.this,
                                "Error: " + apiResponse.getError(),
                                Toast.LENGTH_LONG
                        ).show();
                    }
                } else {
                    Toast.makeText(
                            ResetPasswordActivity.this,
                            "Server error: " + response.code(),
                            Toast.LENGTH_LONG
                    ).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponseDTO<String>> call, Throwable t) {
                progressDialog.dismiss();
                Toast.makeText(
                        ResetPasswordActivity.this,
                        "Network error: " + t.getMessage(),
                        Toast.LENGTH_LONG
                ).show();
            }
        });
    }
}
