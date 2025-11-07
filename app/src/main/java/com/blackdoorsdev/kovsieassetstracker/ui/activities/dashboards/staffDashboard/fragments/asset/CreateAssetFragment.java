package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset;

import android.Manifest;
import android.content.ContentResolver;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.Spinner;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.core.content.FileProvider;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetRepository;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.others.CustomScannerActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetCategoryViewModel;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetViewModel;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.RoomViewModel;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.zxing.BarcodeFormat;
import com.google.zxing.MultiFormatReader;
import com.google.zxing.Result;
import com.google.zxing.common.HybridBinarizer;
import com.journeyapps.barcodescanner.BarcodeEncoder;
import com.journeyapps.barcodescanner.ScanContract;
import com.journeyapps.barcodescanner.ScanOptions;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class CreateAssetFragment extends Fragment {

    private EditText edtServerId, edtName, edtDescription, edtQrText;
    private ImageView imgAsset, btnCapture, imgQrCode, btnScanQr;
    private CheckBox chkAndroidBackup, chkServerBackup;
    private Spinner spinStatus, spinCategory, spinRoom;
    private MaterialButton btnSaveLocal, btnSaveServer, btnGenerateQr;
    private CoordinatorLayout coordinator;

    private String selectedStatusValue;
    private String savedAssetImagePath, savedQrImagePath;
    private File currentPhotoFile;
    private Uri cameraPhotoUri;

    private List<AssetCategoryEntity> categoryList = new ArrayList<>();
    private List<RoomEntity> roomList = new ArrayList<>();

    private AssetViewModel assetViewModel;

    // === Permission launcher ===
    private final ActivityResultLauncher<String> cameraPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) captureAssetPhotoInternal();
                else Snackbar.make(coordinator, "Camera permission required", Snackbar.LENGTH_SHORT).show();
            });

    // === Pick Asset image ===
    private final ActivityResultLauncher<String> pickAssetImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    savedAssetImagePath = saveImageFromUri(uri, "asset_");
                    if (savedAssetImagePath != null)
                        imgAsset.setImageBitmap(BitmapFactory.decodeFile(savedAssetImagePath));
                }
            });

    // === Capture Asset photo ===
    private final ActivityResultLauncher<Uri> takePhotoLauncher =
            registerForActivityResult(new ActivityResultContracts.TakePicture(), result -> {
                if (result && currentPhotoFile != null && currentPhotoFile.exists()) {
                    savedAssetImagePath = currentPhotoFile.getAbsolutePath();
                    imgAsset.setImageBitmap(BitmapFactory.decodeFile(savedAssetImagePath));
                } else Snackbar.make(coordinator, "Failed to save photo", Snackbar.LENGTH_SHORT).show();
            });

    // === Pick QR image ===
    private final ActivityResultLauncher<String> pickQrImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), this::decodeQrFromImage);

    // === Scan QR ===
    private final ActivityResultLauncher<ScanOptions> qrCodeLauncher =
            registerForActivityResult(new ScanContract(), result -> {
                if (result != null && result.getContents() != null) {
                    edtQrText.setText(result.getContents());
                    try {
                        BarcodeEncoder encoder = new BarcodeEncoder();
                        Bitmap qrBitmap = encoder.encodeBitmap(result.getContents(), BarcodeFormat.QR_CODE, 500, 500);
                        handleQrBitmap(qrBitmap);
                    } catch (Exception e) {
                        Snackbar.make(coordinator, "Error generating QR image", Snackbar.LENGTH_SHORT).show();
                    }
                }
            });

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_create_asset, container, false);
        initViews(view);

        assetViewModel = new ViewModelProvider(requireActivity()).get(AssetViewModel.class);
        loadSpinners();

        // === Listeners ===
        imgAsset.setOnClickListener(v -> pickAssetImageLauncher.launch("image/*"));
        btnCapture.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));
        imgQrCode.setOnClickListener(v -> pickQrImageLauncher.launch("image/*"));
        btnScanQr.setOnClickListener(v -> startQrScanner());
        btnGenerateQr.setOnClickListener(v -> generateQrCode());
        btnSaveLocal.setOnClickListener(v -> saveAsset(true));
        btnSaveServer.setOnClickListener(v -> saveAsset(false));

        return view;
    }

    private void initViews(View view) {
        edtServerId = view.findViewById(R.id.txtAssetServerId);
        edtName = view.findViewById(R.id.txtAssetName);
        edtDescription = view.findViewById(R.id.txtAssetDescription);
        edtQrText = view.findViewById(R.id.assetQrCodeSecretText);
        imgAsset = view.findViewById(R.id.assetImageView);
        btnCapture = view.findViewById(R.id.assetCaptureImageView);
        imgQrCode = view.findViewById(R.id.assetImageViewQrCode);
        btnScanQr = view.findViewById(R.id.assetScanImageView);
        chkAndroidBackup = view.findViewById(R.id.isAndroidBackedUp);
        chkServerBackup = view.findViewById(R.id.isServerBackedUp);
        spinStatus = view.findViewById(R.id.statusSpinner);
        spinCategory = view.findViewById(R.id.assetCategorySpinner);
        spinRoom = view.findViewById(R.id.roomSpinner);
        btnSaveLocal = view.findViewById(R.id.btnCreateAssetLocally);
        btnSaveServer = view.findViewById(R.id.btnCreateAssetServer);
        btnGenerateQr = view.findViewById(R.id.btnGenerateQrCode);
        coordinator = requireActivity().findViewById(R.id.coordinator_dashboard);
        setupStatusSpinner();
    }

    private void setupStatusSpinner() {
        String[] statuses = getResources().getStringArray(R.array.asset_statuses);
        String[] values = getResources().getStringArray(R.array.asset_status_values);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_item, statuses);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinStatus.setAdapter(adapter);
        spinStatus.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                selectedStatusValue = values[position];
            }
            public void onNothingSelected(AdapterView<?> parent) { selectedStatusValue = null; }
        });
    }

    private void loadSpinners() {
        AssetCategoryViewModel categoryVM = new ViewModelProvider(this).get(AssetCategoryViewModel.class);
        categoryVM.getAllCategories().observe(getViewLifecycleOwner(), categories -> {
            categoryList = categories;
            List<String> names = new ArrayList<>();
            for (AssetCategoryEntity c : categories) names.add(c.getAssetCategoryName());
            spinCategory.setAdapter(new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, names));
        });

        RoomViewModel roomVM = new ViewModelProvider(this).get(RoomViewModel.class);
        roomVM.getAllRooms().observe(getViewLifecycleOwner(), rooms -> {
            roomList = rooms;
            List<String> names = new ArrayList<>();
            for (RoomEntity r : rooms) names.add(r.getRoomName());
            spinRoom.setAdapter(new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, names));
        });
    }

    private void startQrScanner() {
        ScanOptions options = new ScanOptions();
        options.setDesiredBarcodeFormats(ScanOptions.QR_CODE);
        options.setPrompt("Align QR code inside the square");
        options.setCaptureActivity(CustomScannerActivity.class);
        options.setOrientationLocked(false);
        qrCodeLauncher.launch(options);
    }

    private void captureAssetPhotoInternal() {
        try {
            File dir = new File(requireContext().getFilesDir(), "assets");
            if (!dir.exists()) dir.mkdirs();
            currentPhotoFile = new File(dir, "asset_" + System.currentTimeMillis() + ".jpg");
            cameraPhotoUri = FileProvider.getUriForFile(requireContext(),
                    requireContext().getPackageName() + ".provider", currentPhotoFile);
            takePhotoLauncher.launch(cameraPhotoUri);
        } catch (Exception e) {
            Snackbar.make(coordinator, "Camera error: " + e.getMessage(), Snackbar.LENGTH_LONG).show();
        }
    }

    private String saveBitmap(Bitmap bitmap, String prefix) {
        try {
            File dir = new File(requireContext().getFilesDir(), "assets");
            if (!dir.exists()) dir.mkdirs();
            File file = new File(dir, prefix + System.currentTimeMillis() + ".jpg");
            try (FileOutputStream fos = new FileOutputStream(file)) {
                bitmap.compress(Bitmap.CompressFormat.JPEG, 90, fos);
            }
            return file.getAbsolutePath();
        } catch (Exception e) {
            Log.e("CreateAssetFragment", "Error saving image", e);
            return null;
        }
    }

    private String saveImageFromUri(Uri uri, String prefix) {
        try {
            ContentResolver resolver = requireContext().getContentResolver();
            InputStream input = resolver.openInputStream(uri);
            Bitmap bmp = BitmapFactory.decodeStream(input);
            input.close();
            return saveBitmap(bmp, prefix);
        } catch (Exception e) {
            Log.e("CreateAssetFragment", "Error loading image", e);
            return null;
        }
    }

    private void decodeQrFromImage(Uri uri) {
        try {
            ContentResolver resolver = requireActivity().getContentResolver();
            InputStream input = resolver.openInputStream(uri);
            Bitmap bitmap = BitmapFactory.decodeStream(input);
            input.close();
            int[] pixels = new int[bitmap.getWidth() * bitmap.getHeight()];
            bitmap.getPixels(pixels, 0, bitmap.getWidth(), 0, 0, bitmap.getWidth(), bitmap.getHeight());
            com.google.zxing.RGBLuminanceSource src = new com.google.zxing.RGBLuminanceSource(bitmap.getWidth(), bitmap.getHeight(), pixels);
            Result qrResult = new MultiFormatReader().decode(new com.google.zxing.BinaryBitmap(new HybridBinarizer(src)));
            if (qrResult != null) {
                edtQrText.setText(qrResult.getText());
                handleQrBitmap(bitmap);
            }
        } catch (Exception e) {
            Snackbar.make(coordinator, "Error decoding QR", Snackbar.LENGTH_SHORT).show();
        }
    }

    private void handleQrBitmap(Bitmap bmp) {
        imgQrCode.setImageBitmap(bmp);
        savedQrImagePath = saveBitmap(bmp, "qr_");
    }

    private void generateQrCode() {
        String text = edtQrText.getText().toString().trim();
        if (text.isEmpty()) {
            Snackbar.make(coordinator, "Enter text to generate QR", Snackbar.LENGTH_SHORT).show();
            return;
        }
        try {
            BarcodeEncoder encoder = new BarcodeEncoder();
            Bitmap bmp = encoder.encodeBitmap(text, BarcodeFormat.QR_CODE, 500, 500);
            handleQrBitmap(bmp);
        } catch (Exception e) {
            Snackbar.make(coordinator, "QR generation failed", Snackbar.LENGTH_SHORT).show();
        }
    }

    private void saveAsset(boolean local) {
        String name = edtName.getText().toString().trim();
        if (name.isEmpty()) { edtName.setError("Enter asset name"); return; }

        String desc = edtDescription.getText().toString().trim();
        if (desc.isEmpty()) { edtDescription.setError("Enter description"); return; }

        Integer categoryId = (spinCategory.getSelectedItemPosition() >= 0 && !categoryList.isEmpty())
                ? categoryList.get(spinCategory.getSelectedItemPosition()).getAssetCategoryLocalId() : null;

        Integer roomId = (spinRoom.getSelectedItemPosition() >= 0 && !roomList.isEmpty())
                ? roomList.get(spinRoom.getSelectedItemPosition()).getRoomLocalId() : null;

        AssetEntity asset = new AssetEntity(
                null, name, desc,
                savedAssetImagePath, savedQrImagePath,
                edtQrText.getText().toString().trim(),
                selectedStatusValue,
                chkAndroidBackup.isChecked(),
                chkServerBackup.isChecked(),
                new Date(),
                categoryId,
                roomId
        );

        AssetRepository repo = AssetRepository.getInstance(requireContext());
        if (local) {
            repo.insert(asset);
            Snackbar.make(coordinator, "Saved locally", Snackbar.LENGTH_SHORT).show();
        } else {
            repo.uploadAssetToServer(asset).observe(getViewLifecycleOwner(), msg ->
                    Snackbar.make(coordinator, msg, Snackbar.LENGTH_LONG).show());
        }
        clearForm();
    }

    private void clearForm() {
        edtServerId.setText("");
        edtName.setText("");
        edtDescription.setText("");
        edtQrText.setText("");
        imgAsset.setImageResource(R.drawable.outline_broken_image_24);
        imgQrCode.setImageResource(R.drawable.outline_broken_image_24);
        chkAndroidBackup.setChecked(false);
        chkServerBackup.setChecked(false);
        savedAssetImagePath = savedQrImagePath = null;
    }
}
