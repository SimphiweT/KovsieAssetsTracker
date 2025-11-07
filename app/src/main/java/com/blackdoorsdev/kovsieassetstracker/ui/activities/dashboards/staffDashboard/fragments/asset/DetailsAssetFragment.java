package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset;

import android.net.Uri;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.UpdateAssetFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetViewModel;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;

import java.io.File;
import java.text.DateFormat;

public class DetailsAssetFragment extends Fragment {

    private static final String ARG_ASSET_ID = "asset_id";

    private AssetViewModel assetViewModel;
    private int assetId;
    private AssetEntity asset;

    // UI references
    private MaterialButton btnEdit, btnDelete;
    private ImageView ivAssetImage, ivQrCode, ivAndroidBackup, ivServerBackup;
    private TextView tvName, tvDescription, tvStatus, tvCreated, tvUpdated, tvCategoryId, tvRoomId;

    public static DetailsAssetFragment newInstance(int assetId) {
        DetailsAssetFragment fragment = new DetailsAssetFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_ASSET_ID, assetId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            assetId = getArguments().getInt(ARG_ASSET_ID);
        }
        assetViewModel = new ViewModelProvider(requireActivity()).get(AssetViewModel.class);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
     View view = inflater.inflate(R.layout.fragment_details_asset, container, false);
     initViews(view);
     return  view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);



        // Delete button
        btnDelete.setOnClickListener(v -> {
            if (asset != null) {
                assetViewModel.delete(asset);
                requireActivity().getSupportFragmentManager().popBackStack();
            }
        });

        // Edit button
        btnEdit.setOnClickListener(v -> {
            if (asset != null) {
                Fragment editFragment = UpdateAssetFragment.newInstance(asset.getAssetLocalID());
                requireActivity().getSupportFragmentManager()
                        .beginTransaction()
                        .replace(R.id.frameLayout_dashboard, editFragment)
                        .addToBackStack(null)
                        .commit();
            }
        });
    }
private void initViews(View view){
    // Init views
    ivAssetImage = view.findViewById(R.id.ivAssetImage);
    ivQrCode = view.findViewById(R.id.ivQrCode);
    ivAndroidBackup = view.findViewById(R.id.ivAndroidBackup);
    ivServerBackup = view.findViewById(R.id.ivServerBackup);

    tvName = view.findViewById(R.id.tvName);
    tvDescription = view.findViewById(R.id.tvDescription);
    tvStatus = view.findViewById(R.id.tvStatus);
    tvCreated = view.findViewById(R.id.tvCreatedDate);
    tvUpdated = view.findViewById(R.id.tvUpdatedDate);
    tvCategoryId = view.findViewById(R.id.tvCategoryId);
    tvRoomId = view.findViewById(R.id.tvRoomId);

    btnEdit = view.findViewById(R.id.btnEdit);
    btnDelete = view.findViewById(R.id.btnDelete);

    // Load asset details
    assetViewModel.getAssetById(assetId).observe(getViewLifecycleOwner(), assetEntity -> {
        if (assetEntity != null) {
            bindAssetDetails(assetEntity);
        }
    });
}
    private void bindAssetDetails(AssetEntity asset) {
        this.asset = asset; // store in field

        tvName.setText(asset.getAssetName());
        tvDescription.setText(asset.getAssetDescription());
        tvStatus.setText(asset.getAssetStatus());

        DateFormat df = DateFormat.getDateTimeInstance();

        String createdDateStr = (asset.getCreatedDate() != null)
                ? df.format(asset.getCreatedDate())
                : "Unknown";
        String updatedDateStr = (asset.getUpdatedDate() != null)
                ? df.format(asset.getUpdatedDate())
                : "Never";

        tvCreated.setText("Created: " + createdDateStr);
        tvUpdated.setText("Updated: " + updatedDateStr);

        tvCategoryId.setText("Category ID: " + asset.getAssetCategoryLocalId());
        tvRoomId.setText("Room ID: " + asset.getRoomLocalId());

        // Load Asset Image
        if (asset.getAssetImage() != null && !asset.getAssetImage().isEmpty()) {
            Glide.with(this)
                    .load(Uri.fromFile(new File(asset.getAssetImage())))
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(ivAssetImage);
        } else {
            ivAssetImage.setImageResource(R.drawable.outline_broken_image_24);
        }

        // Load QR Code
        if (asset.getAssetQrCodeImage() != null && !asset.getAssetQrCodeImage().isEmpty()) {
            Glide.with(this)
                    .load(Uri.fromFile(new File(asset.getAssetQrCodeImage())))
                    .placeholder(R.drawable.qrcode_scan)
                    .into(ivQrCode);
        } else {
            ivQrCode.setImageResource(R.drawable.qrcode_scan);
        }

        // Backup indicators
        ivAndroidBackup.setImageResource(asset.isAndroidBackedUp()
                ? R.drawable.android_backup_24px
                : R.drawable.android_no_backup_24px);

        ivServerBackup.setImageResource(asset.isServerBackedUp()
                ? R.drawable.server_backup_24px
                : R.drawable.server_no_backup_24px);
    }

}
