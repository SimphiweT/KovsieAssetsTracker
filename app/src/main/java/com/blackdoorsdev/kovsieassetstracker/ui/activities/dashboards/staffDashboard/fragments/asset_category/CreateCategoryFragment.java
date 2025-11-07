package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category;

import android.Manifest;
import android.content.ContentResolver;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.core.content.FileProvider;
import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageView;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetCategoryRepository;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.Date;

public class CreateCategoryFragment extends Fragment {

    private EditText categoryServerIdEdt, categoryNameEdt, categoryDescriptionEdt;
    private ImageView categoryImageView, categoryCaptureBtn;
    private CheckBox isAndroidBackedUp, isServerBackedUp;
    private MaterialButton btnSaveCategoryToLocalDB, btnSaveCategoryToServerDB;
    private CoordinatorLayout coordinatorLayout;
    private String savedCategoryImagePath = null;
    private Uri cameraPhotoUri;
    private AssetCategoryRepository categoryRepository;

    // Permissions
    private final ActivityResultLauncher<String> cameraPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) captureAssetPhotoInternal();
                else Snackbar.make(coordinatorLayout, "Camera permission required", Snackbar.LENGTH_SHORT).show();
            });

    // Pick image from gallery
    private final ActivityResultLauncher<String> pickCategoryImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    savedCategoryImagePath = saveImageFromUri(uri, "category_");
                    if (savedCategoryImagePath != null) {
                        Bitmap bitmap = BitmapFactory.decodeFile(savedCategoryImagePath);
                        categoryImageView.setImageBitmap(bitmap);
                    } else {
                        Snackbar.make(coordinatorLayout, "Failed to load image", Snackbar.LENGTH_SHORT).show();
                    }
                }
            });

    // Take photo with camera
    private File currentPhotoFile;
    private final ActivityResultLauncher<Uri> takePhotoLauncher =
            registerForActivityResult(new ActivityResultContracts.TakePicture(), result -> {
                if (result && currentPhotoFile != null && currentPhotoFile.exists()) {
                    savedCategoryImagePath = currentPhotoFile.getAbsolutePath();
                    Bitmap bitmap = BitmapFactory.decodeFile(savedCategoryImagePath);
                    categoryImageView.setImageBitmap(bitmap);
                } else {
                    Snackbar.make(coordinatorLayout, "Failed to save photo", Snackbar.LENGTH_SHORT).show();
                }
            });

    public CreateCategoryFragment() {}

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_create_category, container, false);
        initViews(view);

        categoryRepository = AssetCategoryRepository.getInstance(requireContext());

        categoryImageView.setOnClickListener(v -> pickCategoryImageLauncher.launch("image/*"));
        categoryCaptureBtn.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));
        btnSaveCategoryToLocalDB.setOnClickListener(v -> saveCategory(true));
        btnSaveCategoryToServerDB.setOnClickListener(v -> saveCategory(false));

        return view;
    }

    private void initViews(View view) {
        categoryServerIdEdt = view.findViewById(R.id.txtCategoryServerId);
        categoryNameEdt = view.findViewById(R.id.txtCategoryName);
        categoryDescriptionEdt = view.findViewById(R.id.txtCategoryDescription);
        categoryImageView = view.findViewById(R.id.categoryImageView);
        categoryCaptureBtn = view.findViewById(R.id.categoryCaptureImageView);
        isAndroidBackedUp = view.findViewById(R.id.isAndroidBackedUp);
        isServerBackedUp = view.findViewById(R.id.isServerBackedUp);
        btnSaveCategoryToLocalDB = view.findViewById(R.id.btnCreateCategoryLocally);
        btnSaveCategoryToServerDB = view.findViewById(R.id.btnCreateCategoryServer);
        coordinatorLayout = requireActivity().findViewById(R.id.coordinator_dashboard);
    }

    private void captureAssetPhotoInternal() {
        try {
            File photoDir = new File(requireContext().getFilesDir(), "categories");
            if (!photoDir.exists()) photoDir.mkdirs();

            currentPhotoFile = new File(photoDir, "category_" + System.currentTimeMillis() + ".jpg");
            cameraPhotoUri = FileProvider.getUriForFile(
                    requireContext(),
                    requireContext().getPackageName() + ".provider",
                    currentPhotoFile
            );
            takePhotoLauncher.launch(cameraPhotoUri);
        } catch (Exception e) {
            Snackbar.make(coordinatorLayout, "Error capturing photo: " + e.getMessage(), Snackbar.LENGTH_LONG).show();
        }
    }

    private String saveBitmap(Bitmap bitmap, String prefix) {
        try {
            File dir = new File(requireContext().getFilesDir(), "categories");
            if (!dir.exists()) dir.mkdirs();

            File file = new File(dir, prefix + System.currentTimeMillis() + ".jpg");
            FileOutputStream fos = new FileOutputStream(file);
            bitmap.compress(Bitmap.CompressFormat.JPEG, 90, fos);
            fos.flush();
            fos.close();
            return file.getAbsolutePath();
        } catch (Exception e) {
            Snackbar.make(requireView(), "Failed to save image", Snackbar.LENGTH_LONG).show();
            return null;
        }
    }

    private String saveImageFromUri(Uri uri, String prefix) {
        try {
            ContentResolver resolver = requireActivity().getContentResolver();
            InputStream inputStream = resolver.openInputStream(uri);
            if (inputStream == null) return null;

            Bitmap bitmap = BitmapFactory.decodeStream(inputStream);
            inputStream.close();
            if (bitmap == null) return null;

            return saveBitmap(bitmap, prefix);
        } catch (Exception e) {
            return null;
        }
    }

    private void saveCategory(boolean local) {
        String serverId = categoryServerIdEdt.getText().toString().trim();
        Integer iServerId = null;
        if (!serverId.isEmpty()) {
            try {
                iServerId = Integer.parseInt(serverId);
            } catch (NumberFormatException e) {
                categoryServerIdEdt.setError("Invalid ID");
                return;
            }
        }

        String name = categoryNameEdt.getText().toString().trim();
        if (name.isEmpty()) {
            categoryNameEdt.setError("Enter category name");
            return;
        }

        String description = categoryDescriptionEdt.getText().toString().trim();
        if (description.isEmpty()) {
            categoryDescriptionEdt.setError("Enter description");
            return;
        }

        AssetCategoryEntity category = new AssetCategoryEntity(
                iServerId,
                name,
                description,
                savedCategoryImagePath != null ? savedCategoryImagePath : "",
                isAndroidBackedUp.isChecked(),
                isServerBackedUp.isChecked(),
                new Date()
        );

        if (local) {
            MyApp.getDatabase().assetCategoryDao().create(category);
            Snackbar.make(coordinatorLayout, "Category saved locally", Snackbar.LENGTH_SHORT).show();
        } else {
            categoryRepository.uploadCategoryToServer(category)
                    .observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
        }

        // reset form
        categoryServerIdEdt.setText("");
        categoryNameEdt.setText("");
        categoryDescriptionEdt.setText("");
        categoryImageView.setImageResource(R.drawable.outline_broken_image_24);
        savedCategoryImagePath = null;
        isAndroidBackedUp.setChecked(false);
        isServerBackedUp.setChecked(false);
    }
}
