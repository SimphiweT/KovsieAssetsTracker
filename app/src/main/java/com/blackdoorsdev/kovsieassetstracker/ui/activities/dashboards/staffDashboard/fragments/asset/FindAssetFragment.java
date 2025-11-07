package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset;

import android.graphics.Bitmap;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.annotation.NonNull;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.fragment.app.Fragment;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.others.CustomScannerActivity;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.zxing.BarcodeFormat;
import com.journeyapps.barcodescanner.BarcodeEncoder;
import com.journeyapps.barcodescanner.ScanContract;
import com.journeyapps.barcodescanner.ScanOptions;

public class FindAssetFragment extends Fragment {

    private CoordinatorLayout coordinatorLayout;
    private ImageView ivQrPreview;
    private TextView tvName, tvDescription, tvStatus, tvCategory, tvRoom;
    private MaterialButton btnScanQr;

    private final ActivityResultLauncher<ScanOptions> qrScannerLauncher =
            registerForActivityResult(new ScanContract(), result -> {
                if (result != null && result.getContents() != null) {
                    String scannedCode = result.getContents();
                    showQrPreview(scannedCode);
                    fetchAssetDetails(scannedCode);
                } else {
                    Snackbar.make(coordinatorLayout, "Scan cancelled", Snackbar.LENGTH_SHORT).show();
                }
            });

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_find_asset, container, false);
        initViews(view);

        btnScanQr.setOnClickListener(v -> startQrScanner());

        return view;
    }

    private void initViews(View view) {
        coordinatorLayout = view.findViewById(R.id.coordinator_find_asset);
        btnScanQr = view.findViewById(R.id.btnScanQr);
        ivQrPreview = view.findViewById(R.id.ivQrPreview);

        tvName = view.findViewById(R.id.tvAssetName);
        tvDescription = view.findViewById(R.id.tvAssetDescription);
        tvStatus = view.findViewById(R.id.tvAssetStatus);
        tvCategory = view.findViewById(R.id.tvAssetCategory);
        tvRoom = view.findViewById(R.id.tvAssetRoom);
    }

    private void startQrScanner() {
        ScanOptions options = new ScanOptions();
        options.setDesiredBarcodeFormats(ScanOptions.QR_CODE);
        options.setPrompt("Align QR code inside the frame");
        options.setCameraId(0);
        options.setBeepEnabled(true);
        options.setOrientationLocked(false);
        options.setCaptureActivity(CustomScannerActivity.class);
        qrScannerLauncher.launch(options);
    }

    private void showQrPreview(String data) {
        try {
            BarcodeEncoder encoder = new BarcodeEncoder();
            Bitmap bitmap = encoder.encodeBitmap(data, BarcodeFormat.QR_CODE, 400, 400);
            ivQrPreview.setImageBitmap(bitmap);
        } catch (Exception e) {
            Snackbar.make(coordinatorLayout, "Error generating QR preview", Snackbar.LENGTH_SHORT).show();
        }
    }

    private void fetchAssetDetails(String qrSecret) {
        AssetEntity asset = MyApp.getDatabase().assetDao().findByQrSecret(qrSecret);
        if (asset != null) {
            tvName.setText(asset.getAssetName());
            tvDescription.setText(asset.getAssetDescription());
            tvStatus.setText("Status: " + asset.getAssetStatus());
            tvCategory.setText("Category ID: " + asset.getAssetCategoryLocalId());
            tvRoom.setText("Room ID: " + asset.getRoomLocalId());
        } else {
            Snackbar.make(coordinatorLayout, "No asset found for this QR", Snackbar.LENGTH_SHORT).show();
        }
    }
}
