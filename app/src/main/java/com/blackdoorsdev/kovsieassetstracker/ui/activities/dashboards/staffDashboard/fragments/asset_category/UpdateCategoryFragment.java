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
import androidx.lifecycle.ViewModelProvider;

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
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetCategoryViewModel;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.Date;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link UpdateCategoryFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class UpdateCategoryFragment extends Fragment {

    private EditText txtCategoryLocalId,txtCategoryServerId, txtCategoryName, txtCategoryDescription;
    private ImageView categoryImageView, categoryCaptureImageView;
    private CheckBox isAndroidBackedUp, isServerBackedUp;
    private MaterialButton btnUpdateCategoryLocally, btnUpdateCategoryServer;
    private CoordinatorLayout coordinatorLayout;
    private String savedCategoryImagePath = null;
    private Uri cameraPhotoUri;

    private Date createdDate;
    private AssetCategoryRepository categoryRepository;
    // Permissions
    private final ActivityResultLauncher<String> cameraPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) captureAssetPhotoInternal();
                else Snackbar.make(coordinatorLayout, "Camera permission required", Snackbar.LENGTH_SHORT).show();
            });

    // Pick category image from gallery
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
    private File currentPhotoFile; // store real file
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

    private static final String ARG_CATEGORY_ID = "category_id";
    private int categoryId = -1;

    public UpdateCategoryFragment() {
        // Required empty public constructor
    }

    public static UpdateCategoryFragment newInstance(int categoryId) {
        UpdateCategoryFragment fragment = new UpdateCategoryFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_CATEGORY_ID, categoryId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            categoryId = getArguments().getInt(ARG_CATEGORY_ID, -1);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_update_category, container, false);
        initViews(view);

        AssetCategoryViewModel categoryViewModel = new ViewModelProvider(requireActivity()).get(AssetCategoryViewModel.class);
        categoryRepository = AssetCategoryRepository.getInstance(requireContext());

        if(categoryId != -1){
            categoryViewModel.getCategoryById(categoryId).observe(getViewLifecycleOwner(),category -> {
                if(category != null){
                    preloadCategoryData(category);
                }
            });
        }

        categoryImageView.setOnClickListener(v -> pickCategoryImageLauncher.launch("image/*"));
        categoryCaptureImageView.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));

        btnUpdateCategoryLocally.setOnClickListener(v -> UpdateCategory(true));
        btnUpdateCategoryServer.setOnClickListener(v -> UpdateCategory(false));

        return view;
    }
    private void initViews(View view) {
        txtCategoryLocalId = view.findViewById(R.id.txtCategoryLocalId);
        txtCategoryServerId = view.findViewById(R.id.txtCategoryServerId);
        txtCategoryName = view.findViewById(R.id.txtCategoryName);
        txtCategoryDescription = view.findViewById(R.id.txtCategoryDescription);
        categoryImageView = view.findViewById(R.id.categoryImageView);
        categoryCaptureImageView = view.findViewById(R.id.categoryCaptureImageView);
        isAndroidBackedUp = view.findViewById(R.id.isAndroidBackedUp);
        isServerBackedUp = view.findViewById(R.id.isServerBackedUp);
        btnUpdateCategoryLocally = view.findViewById(R.id.btnUpdateCategoryLocally);
        btnUpdateCategoryServer = view.findViewById(R.id.btnUpdateCategoryServer);
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

            Log.d("UpdateCategoryFragment", "Saved image: " + file.getAbsolutePath());
            return file.getAbsolutePath();
        } catch (Exception e) {
            Log.e("UpdateCategoryFragment", "Error saving bitmap", e);
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
            Log.e("UpdateCategoryFragment", "Error saving image from URI", e);
            return null;
        }
    }
    private void preloadCategoryData(AssetCategoryEntity category) {
        txtCategoryLocalId.setText(String.valueOf(category.getAssetCategoryLocalId()));
        txtCategoryServerId.setText(category.getAssetCategoryServerId() !=null ? String.valueOf(category.getAssetCategoryServerId()) : "");
        txtCategoryName.setText(category.getAssetCategoryName());
        txtCategoryDescription.setText(category.getAssetCategoryDescription());

        //Load Image
        if(category.getAssetCategoryImage() != null && !category.getAssetCategoryImage().isEmpty()){
            savedCategoryImagePath = category.getAssetCategoryImage();
            categoryImageView.setImageURI(Uri.fromFile(new File(savedCategoryImagePath)));
        }

        isAndroidBackedUp.setChecked(category.isAndroidBackedUp());
        isServerBackedUp.setChecked(category.isServerBackedUp());
        createdDate = category.getCreatedDate();
    }
    private void UpdateCategory(boolean local) {

        String localId = txtCategoryLocalId.getText().toString().trim();
        Integer iLocalId = null;
        if(!localId.isEmpty())
        {
            try {
                iLocalId = Integer.parseInt(localId);
            } catch (NumberFormatException e) {
                txtCategoryLocalId.setError("Provided value is not an Integer. Please provide valid Id");
                return;
            }
        }

        String serverId = txtCategoryServerId.getText().toString().trim();
        Integer iServerId = null;
        if(!serverId.isEmpty())
        {
            try {
                iServerId = Integer.parseInt(serverId);
            } catch (NumberFormatException e) {
                txtCategoryServerId.setError("Provided value is not an Integer. Please provide valid Id");
                return;
            }
        }

        String name = txtCategoryName.getText().toString().trim();
        if(name.isEmpty()){
            txtCategoryName.setError("Enter category name");
            return;
        }

        String description = txtCategoryDescription.getText().toString().trim();
        if(description.isEmpty()){
            txtCategoryDescription.setError("Enter category description");
            return;
        }

        AssetCategoryEntity category = new AssetCategoryEntity(
                iLocalId,
                iServerId,
                name,
                description,
                savedCategoryImagePath != null? savedCategoryImagePath : "",
                isAndroidBackedUp.isChecked(),
                isServerBackedUp.isChecked(),
                createdDate,
                new Date()
        );

        if(local){
            MyApp.getDatabase().assetCategoryDao().update(category);
            Snackbar.make(coordinatorLayout, "Category updated locally", Snackbar.LENGTH_SHORT).show();
        }else {
            categoryRepository.updateCategoryOnServer(category)
                    .observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
        }

        if(categoryId == -1) {
            txtCategoryLocalId.setText("");
            txtCategoryServerId.setText("");
            txtCategoryName.setText("");
            txtCategoryDescription.setText("");
            categoryImageView.setImageResource(R.drawable.outline_broken_image_24);
            savedCategoryImagePath = null;
            isAndroidBackedUp.setChecked(false);
            isServerBackedUp.setChecked(false);
        }
    }
}