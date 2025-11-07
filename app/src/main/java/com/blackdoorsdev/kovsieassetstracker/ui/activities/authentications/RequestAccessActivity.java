package com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.remote.api.ApiAuthService;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;

import java.io.File;
import java.io.InputStream;
import java.io.OutputStream;

import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;

public class RequestAccessActivity extends AppCompatActivity {

    private static final int PICK_PDF_REQUEST = 100;

    private TextInputEditText etName, etEmail, etMessage;
    private Spinner spinnerRole;
    private MaterialButton btnUpload, btnSubmit;
    private Uri selectedPdfUri;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_request_access);

        etName = findViewById(R.id.etName);
        etEmail = findViewById(R.id.etEmail);
        etMessage = findViewById(R.id.etMessage);
        spinnerRole = findViewById(R.id.spinnerRole);
        btnUpload = findViewById(R.id.btnUploadPdf);
        btnSubmit = findViewById(R.id.btnSubmit);

        ArrayAdapter<CharSequence> adapter = ArrayAdapter.createFromResource(
                this, R.array.roles_array, android.R.layout.simple_spinner_item
        );
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerRole.setAdapter(adapter);

        btnUpload.setOnClickListener(v -> openFileChooser());
        btnSubmit.setOnClickListener(v -> handleSubmit());
    }

    private void openFileChooser() {
        Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("application/pdf");
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        startActivityForResult(Intent.createChooser(intent, "Select PDF"), PICK_PDF_REQUEST);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == PICK_PDF_REQUEST && resultCode == RESULT_OK && data != null) {
            selectedPdfUri = data.getData();
            Toast.makeText(this, "PDF Selected ", Toast.LENGTH_SHORT).show();
        }
    }

    private void handleSubmit() {
        String name = etName.getText() != null ? etName.getText().toString().trim() : "";
        String email = etEmail.getText() != null ? etEmail.getText().toString().trim() : "";
        String role = spinnerRole.getSelectedItem().toString();
        String message = etMessage.getText() != null ? etMessage.getText().toString().trim() : "";


        if (name.isEmpty() || email.isEmpty()) {
            Toast.makeText(this, "Please enter your name and email", Toast.LENGTH_SHORT).show();
            return;
        }

        try {
            RequestBody nameBody = RequestBody.create(MediaType.parse("text/plain"), name);
            RequestBody emailBody = RequestBody.create(MediaType.parse("text/plain"), email);
            RequestBody roleBody = RequestBody.create(MediaType.parse("text/plain"), role);
            RequestBody msgBody = RequestBody.create(MediaType.parse("text/plain"), message);

            MultipartBody.Part pdfPart = null;
            if (selectedPdfUri != null) {
                File tempFile = createTempFileFromUri(selectedPdfUri);
                RequestBody fileBody = RequestBody.create(MediaType.parse("application/pdf"), tempFile);
                pdfPart = MultipartBody.Part.createFormData("PdfDocument", tempFile.getName(), fileBody);
            }

            Retrofit retrofit = RetrofitClient.getClient();
            ApiAuthService api = retrofit.create(ApiAuthService.class);

            Call<ApiResponseDTO<String>> call = api.requestAccess(nameBody, emailBody, roleBody, msgBody, pdfPart);

            Toast.makeText(this, "Submitting request...", Toast.LENGTH_SHORT).show();

            call.enqueue(new Callback<ApiResponseDTO<String>>() {
                @Override
                public void onResponse(Call<ApiResponseDTO<String>> call, Response<ApiResponseDTO<String>> response) {
                    if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                        Toast.makeText(RequestAccessActivity.this,
                                response.body().getMessage(),
                                Toast.LENGTH_LONG).show();
                        finish();
                    } else {
                        Toast.makeText(RequestAccessActivity.this,
                                "Failed: " + (response.body() != null ? response.body().getError() : "Server error"),
                                Toast.LENGTH_LONG).show();
                    }
                }

                @Override
                public void onFailure(Call<ApiResponseDTO<String>> call, Throwable t) {
                    Toast.makeText(RequestAccessActivity.this,
                            "Upload failed: " + t.getMessage(),
                            Toast.LENGTH_LONG).show();
                }
            });

        } catch (Exception e) {
            Toast.makeText(this, "Error preparing file: " + e.getMessage(), Toast.LENGTH_LONG).show();
        }
    }

    private File createTempFileFromUri(Uri uri) throws Exception {
        String fileName = "access_request.pdf";
        File tempFile = new File(getCacheDir(), fileName);
        try (InputStream in = getContentResolver().openInputStream(uri);
             OutputStream out = new java.io.FileOutputStream(tempFile)) {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = in.read(buffer)) != -1) {
                out.write(buffer, 0, bytesRead);
            }
            out.flush();
        }
        return tempFile;
    }
}
