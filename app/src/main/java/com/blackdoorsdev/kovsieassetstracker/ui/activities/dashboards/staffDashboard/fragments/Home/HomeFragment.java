package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.Home;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.CreateAssetFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.FindAssetFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room.CreateRoomFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.user.UserProfileFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetCategoryViewModel;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetViewModel;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.RoomViewModel;
import com.google.android.material.card.MaterialCardView;

public class HomeFragment extends Fragment {

    private TextView tvTotalAssets, tvTotalCategories, tvTotalRooms;
    private MaterialCardView addAssetQuickLayout, findAssetQuickLayout,
            addRoomQuickLayout, gotoUserProfileQuickLayout;

    private AssetViewModel assetViewModel;
    private AssetCategoryViewModel categoryViewModel;
    private RoomViewModel roomViewModel;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        initViews(view);
        initViewModels();
        setupQuickActions();

        return view;
    }

    private void initViews(View view) {
        tvTotalAssets = view.findViewById(R.id.tv_total_assets);
        tvTotalCategories = view.findViewById(R.id.tv_total_category);
        tvTotalRooms = view.findViewById(R.id.tv_total_rooms);

        addAssetQuickLayout = view.findViewById(R.id.addAssetQuickLayout);
        findAssetQuickLayout = view.findViewById(R.id.findAssetQuickLayout);
        addRoomQuickLayout = view.findViewById(R.id.addRoomQuickLayout);
        gotoUserProfileQuickLayout = view.findViewById(R.id.gotoUserProfileQuickLayout);
    }

    private void initViewModels() {
        assetViewModel = new ViewModelProvider(requireActivity()).get(AssetViewModel.class);
        categoryViewModel = new ViewModelProvider(requireActivity()).get(AssetCategoryViewModel.class);
        roomViewModel = new ViewModelProvider(requireActivity()).get(RoomViewModel.class);

        // Observe total assets
        assetViewModel.getAllAssets().observe(getViewLifecycleOwner(), assets ->
                tvTotalAssets.setText(String.valueOf(assets != null ? assets.size() : 0)));

        // Observe total categories
        categoryViewModel.getAllCategories().observe(getViewLifecycleOwner(), categories ->
                tvTotalCategories.setText(String.valueOf(categories != null ? categories.size() : 0)));

        // Observe total rooms
        roomViewModel.getAllRooms().observe(getViewLifecycleOwner(), rooms ->
                tvTotalRooms.setText(String.valueOf(rooms != null ? rooms.size() : 0)));
    }

    private void setupQuickActions() {
        addAssetQuickLayout.setOnClickListener(v -> switchFragment(new CreateAssetFragment()));
        findAssetQuickLayout.setOnClickListener(v -> switchFragment(new FindAssetFragment()));
        addRoomQuickLayout.setOnClickListener(v -> switchFragment(new CreateRoomFragment()));
        gotoUserProfileQuickLayout.setOnClickListener(v -> switchFragment(new UserProfileFragment()));
    }

    private void switchFragment(Fragment fragment) {
        requireActivity().getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.frameLayout_dashboard, fragment)
                .addToBackStack(null)
                .commit();
    }
}
