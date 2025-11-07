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
import androidx.annotation.Nullable;
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

public class UpdateAssetFragment extends Fragment {

    private static final String ARG_ASSET_ID = "asset_id";

    private EditText edtLocalId, edtServerId, edtName, edtDescription, edtQrText;
    private ImageView imgAsset, btnCapture, imgQrCode, btnScanQr;
    private CheckBox chkAndroidBackup, chkServerBackup;
    private Spinner spinStatus, spinCategory, spinRoom;
    private MaterialButton btnUpdateLocal, btnUpdateServer, btnGenerateQr;
    private CoordinatorLayout coordinator;

    private String selectedStatusValue;
    private String savedAssetImagePath, savedQrImagePath;
    private File currentPhotoFile;
    private Uri cameraPhotoUri;

    private Date createdDate;
    private List<AssetCategoryEntity> categoryList = new ArrayList<>();
    private List<RoomEntity> roomList = new ArrayList<>();

    private AssetViewModel assetViewModel;
    private int assetId = -1;

    // === Factory ===
    public static UpdateAssetFragment newInstance(int assetId) {
        UpdateAssetFragment f = new UpdateAssetFragment();
        Bundle b = new Bundle();
        b.putInt(ARG_ASSET_ID, assetId);
        f.setArguments(b);
        return f;
    }

    // === Permissions ===
    private final ActivityResultLauncher<String> cameraPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) captureAssetPhotoInternal();
                else Snackbar.make(coordinator, "Camera permission required", Snackbar.LENGTH_SHORT).show();
            });

    // === Pick / Capture Image ===
    private final ActivityResultLauncher<String> pickAssetImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    savedAssetImagePath = saveImageFromUri(uri, "asset_");
                    if (savedAssetImagePath != null)
                        imgAsset.setImageBitmap(BitmapFactory.decodeFile(savedAssetImagePath));
                }
            });

    private final ActivityResultLauncher<Uri> takePhotoLauncher =
            registerForActivityResult(new ActivityResultContracts.TakePicture(), result -> {
                if (result && currentPhotoFile != null && currentPhotoFile.exists()) {
                    savedAssetImagePath = currentPhotoFile.getAbsolutePath();
                    imgAsset.setImageBitmap(BitmapFactory.decodeFile(savedAssetImagePath));
                } else Snackbar.make(coordinator, "Failed to save photo", Snackbar.LENGTH_SHORT).show();
            });

    // === QR Handling ===
    private final ActivityResultLauncher<String> pickQrImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), this::decodeQrFromImage);

    private final ActivityResultLauncher<ScanOptions> qrCodeLauncher =
            registerForActivityResult(new ScanContract(), result -> {
                if (result != null && result.getContents() != null) {
                    edtQrText.setText(result.getContents());
                    try {
                        BarcodeEncoder encoder = new BarcodeEncoder();
                        Bitmap bmp = encoder.encodeBitmap(result.getContents(), BarcodeFormat.QR_CODE, 500, 500);
                        handleQrBitmap(bmp);
                    } catch (Exception e) {
                        Snackbar.make(coordinator, "Error generating QR image", Snackbar.LENGTH_SHORT).show();
                    }
                }
            });

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null)
            assetId = getArguments().getInt(ARG_ASSET_ID, -1);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_update_asset, container, false);
        initViews(view);

        assetViewModel = new ViewModelProvider(requireActivity()).get(AssetViewModel.class);
        loadSpinners();

        if (assetId != -1)
            assetViewModel.getAssetById(assetId).observe(getViewLifecycleOwner(), this::preloadAssetData);

        // === Click listeners ===
        imgAsset.setOnClickListener(v -> pickAssetImageLauncher.launch("image/*"));
        btnCapture.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));
        imgQrCode.setOnClickListener(v -> pickQrImageLauncher.launch("image/*"));
        btnScanQr.setOnClickListener(v -> startQrScanner());
        btnGenerateQr.setOnClickListener(v -> generateQrCode());
        btnUpdateLocal.setOnClickListener(v -> updateAsset(true));
        btnUpdateServer.setOnClickListener(v -> updateAsset(false));

        return view;
    }

    private void initViews(View view) {
        edtLocalId = view.findViewById(R.id.txtAssetLocalId);
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
        btnUpdateLocal = view.findViewById(R.id.btnUpdateAssetLocally);
        btnUpdateServer = view.findViewById(R.id.btnUpdateAssetServer);
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
            public void onItemSelected(AdapterView<?> p, View v, int pos, long id) {
                selectedStatusValue = values[pos];
            }
            public void onNothingSelected(AdapterView<?> p) { selectedStatusValue = null; }
        });
    }

    private void preloadAssetData(AssetEntity asset) {
        if (asset == null) return;

        edtLocalId.setText(String.valueOf(asset.getAssetLocalID()));
        edtServerId.setText(asset.getAssetServerId() != null ? String.valueOf(asset.getAssetServerId()) : "");
        edtName.setText(asset.getAssetName());
        edtDescription.setText(asset.getAssetDescription());
        edtQrText.setText(asset.getAssetQrCodeSecretText());
        createdDate = asset.getCreatedDate();

        // Load images
        if (asset.getAssetImage() != null && !asset.getAssetImage().isEmpty()) {
            savedAssetImagePath = asset.getAssetImage();
            imgAsset.setImageURI(Uri.fromFile(new File(savedAssetImagePath)));
        }
        if (asset.getAssetQrCodeImage() != null && !asset.getAssetQrCodeImage().isEmpty()) {
            savedQrImagePath = asset.getAssetQrCodeImage();
            imgQrCode.setImageURI(Uri.fromFile(new File(savedQrImagePath)));
        }

        chkAndroidBackup.setChecked(asset.isAndroidBackedUp());
        chkServerBackup.setChecked(asset.isServerBackedUp());

        // Set spinner selections once loaded
        spinCategory.post(() -> {
            for (int i = 0; i < categoryList.size(); i++)
                if (categoryList.get(i).getAssetCategoryLocalId() == asset.getAssetCategoryLocalId())
                    spinCategory.setSelection(i);
        });

        spinRoom.post(() -> {
            for (int i = 0; i < roomList.size(); i++)
                if (roomList.get(i).getRoomLocalId().equals(asset.getRoomLocalId()))
                    spinRoom.setSelection(i);
        });

        spinStatus.post(() -> {
            String[] vals = getResources().getStringArray(R.array.asset_status_values);
            for (int i = 0; i < vals.length; i++)
                if (vals[i].equals(asset.getAssetStatus())) {
                    spinStatus.setSelection(i);
                    selectedStatusValue = vals[i];
                }
        });
    }

    private void loadSpinners() {
        AssetCategoryViewModel catVM = new ViewModelProvider(this).get(AssetCategoryViewModel.class);
        catVM.getAllCategories().observe(getViewLifecycleOwner(), categories -> {
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

    private void decodeQrFromImage(Uri uri) {
        try {
            ContentResolver resolver = requireActivity().getContentResolver();
            InputStream input = resolver.openInputStream(uri);
            Bitmap bmp = BitmapFactory.decodeStream(input);
            input.close();
            int[] px = new int[bmp.getWidth() * bmp.getHeight()];
            bmp.getPixels(px, 0, bmp.getWidth(), 0, 0, bmp.getWidth(), bmp.getHeight());
            com.google.zxing.RGBLuminanceSource src = new com.google.zxing.RGBLuminanceSource(bmp.getWidth(), bmp.getHeight(), px);
            Result qrResult = new MultiFormatReader().decode(new com.google.zxing.BinaryBitmap(new HybridBinarizer(src)));
            if (qrResult != null) {
                edtQrText.setText(qrResult.getText());
                handleQrBitmap(bmp);
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
        String qrText = edtQrText.getText().toString().trim();
        if (qrText.isEmpty()) {
            Snackbar.make(coordinator, "Enter QR text", Snackbar.LENGTH_SHORT).show();
            return;
        }
        try {
            BarcodeEncoder encoder = new BarcodeEncoder();
            Bitmap bmp = encoder.encodeBitmap(qrText, BarcodeFormat.QR_CODE, 500, 500);
            handleQrBitmap(bmp);
        } catch (Exception e) {
            Snackbar.make(coordinator, "Error generating QR", Snackbar.LENGTH_SHORT).show();
        }
    }

    private String saveBitmap(Bitmap bmp, String prefix) {
        try {
            File dir = new File(requireContext().getFilesDir(), "assets");
            if (!dir.exists()) dir.mkdirs();
            File f = new File(dir, prefix + System.currentTimeMillis() + ".jpg");
            try (FileOutputStream fos = new FileOutputStream(f)) {
                bmp.compress(Bitmap.CompressFormat.JPEG, 90, fos);
            }
            return f.getAbsolutePath();
        } catch (Exception e) {
            Log.e("UpdateAssetFragment", "Save bitmap failed", e);
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
            return null;
        }
    }

    private void startQrScanner() {
        ScanOptions options = new ScanOptions();
        options.setDesiredBarcodeFormats(ScanOptions.QR_CODE);
        options.setPrompt("Align QR code inside the square");
        options.setBeepEnabled(true);
        options.setBarcodeImageEnabled(true);
        options.setOrientationLocked(false);
        options.setCaptureActivity(CustomScannerActivity.class);
        qrCodeLauncher.launch(options);
    }

    private void updateAsset(boolean local) {
        Integer localId = parseIntSafe(edtLocalId.getText().toString());
        Integer serverId = parseIntSafe(edtServerId.getText().toString());

        if (localId == null) {
            edtLocalId.setError("Local ID required");
            return;
        }

        String name = edtName.getText().toString().trim();
        if (name.isEmpty()) { edtName.setError("Enter name"); return; }

        String desc = edtDescription.getText().toString().trim();
        if (desc.isEmpty()) { edtDescription.setError("Enter description"); return; }

        Integer catId = (spinCategory.getSelectedItemPosition() >= 0 && !categoryList.isEmpty())
                ? categoryList.get(spinCategory.getSelectedItemPosition()).getAssetCategoryLocalId() : null;

        Integer roomId = (spinRoom.getSelectedItemPosition() >= 0 && !roomList.isEmpty())
                ? roomList.get(spinRoom.getSelectedItemPosition()).getRoomLocalId() : null;

        AssetEntity asset = new AssetEntity(
                localId,
                serverId,
                name,
                desc,
                savedAssetImagePath,
                savedQrImagePath,
                edtQrText.getText().toString().trim(),
                selectedStatusValue,
                chkAndroidBackup.isChecked(),
                chkServerBackup.isChecked(),
                createdDate != null ? createdDate : new Date(),
                new Date(),
                catId,
                roomId
        );

        AssetRepository repo = AssetRepository.getInstance(requireContext());

        if (local) {
            repo.update(asset);
            Snackbar.make(coordinator, "Asset updated locally", Snackbar.LENGTH_SHORT).show();
        } else {
            if (serverId != null && serverId > 0) {
                repo.updateAssetOnServer(asset).observe(getViewLifecycleOwner(), msg ->
                        Snackbar.make(coordinator, msg, Snackbar.LENGTH_LONG).show());
            } else {
                repo.uploadAssetToServer(asset).observe(getViewLifecycleOwner(), msg ->
                        Snackbar.make(coordinator, msg, Snackbar.LENGTH_LONG).show());
            }
        }
    }

    private Integer parseIntSafe(String val) {
        try {
            return val.isEmpty() ? null : Integer.parseInt(val);
        } catch (NumberFormatException e) {
            return null;
        }
    }
}
