package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category;

import android.net.Uri;
import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetCategoryViewModel;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;

import java.io.File;
import java.text.DateFormat;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link DetailsCategoryFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class DetailsCategoryFragment extends Fragment {

    private static final String ARG_CATEGORY_ID = "category_id";

    private AssetCategoryViewModel categoryViewModel;
    private int categoryId;
    private AssetCategoryEntity category;

    // UI references
    private MaterialButton btnEdit, btnDelete;
    private ImageView ivCategoryImage, ivAndroidBackup, ivServerBackup;
    private TextView tvName, tvDescription, tvCreated, tvUpdated;

    public static DetailsCategoryFragment newInstance(int categoryId) {
        DetailsCategoryFragment fragment = new DetailsCategoryFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_CATEGORY_ID, categoryId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            categoryId = getArguments().getInt(ARG_CATEGORY_ID);
        }
        categoryViewModel = new ViewModelProvider(requireActivity()).get(AssetCategoryViewModel.class);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_details_category, container, false);
        initViews(view);
        return  view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);



        // Delete button
        btnDelete.setOnClickListener(v -> {
            if (category != null) {
                categoryViewModel.delete(category);
                requireActivity().getSupportFragmentManager().popBackStack();
            }
        });

        // Edit button
        btnEdit.setOnClickListener(v -> {
            if (category != null) {
                Fragment editFragment = UpdateCategoryFragment.newInstance(category.getAssetCategoryLocalId());
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
        ivCategoryImage = view.findViewById(R.id.ivCategoryImage);
        ivAndroidBackup = view.findViewById(R.id.ivAndroidBackup);
        ivServerBackup = view.findViewById(R.id.ivServerBackup);

        tvName = view.findViewById(R.id.tvCategoryName);
        tvDescription = view.findViewById(R.id.tvCategoryDescription);
        tvCreated = view.findViewById(R.id.tvCreatedDate);
        tvUpdated = view.findViewById(R.id.tvUpdatedDate);

        btnEdit = view.findViewById(R.id.btnEdit);
        btnDelete = view.findViewById(R.id.btnDelete);

        // Load asset details
        categoryViewModel.getCategoryById(categoryId).observe(getViewLifecycleOwner(), categoryEntity -> {
            if (categoryEntity != null) {
                bindCategoryDetails(categoryEntity);
            }
        });
    }
    private void bindCategoryDetails(AssetCategoryEntity category) {
        this.category = category; // store in field

        tvName.setText(category.getAssetCategoryName());
        tvDescription.setText(category.getAssetCategoryDescription());

        DateFormat df = DateFormat.getDateTimeInstance();

        String createdDateStr = (category.getCreatedDate() != null)
                ? df.format(category.getCreatedDate())
                : "Unknown";
        String updatedDateStr = (category.getUpdatedDate() != null)
                ? df.format(category.getUpdatedDate())
                : "Never";

        tvCreated.setText("Created: " + createdDateStr);
        tvUpdated.setText("Updated: " + updatedDateStr);

        // Load Asset Image
        if (category.getAssetCategoryImage() != null && !category.getAssetCategoryImage().isEmpty()) {
            Glide.with(this)
                    .load(Uri.fromFile(new File(category.getAssetCategoryImage())))
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(ivCategoryImage);
        } else {
            ivCategoryImage.setImageResource(R.drawable.outline_broken_image_24);
        }

        // Backup indicators
        ivAndroidBackup.setImageResource(category.isAndroidBackedUp()
                ? R.drawable.android_backup_24px
                : R.drawable.android_no_backup_24px);

        ivServerBackup.setImageResource(category.isServerBackedUp()
                ? R.drawable.server_backup_24px
                : R.drawable.server_no_backup_24px);
    }
}