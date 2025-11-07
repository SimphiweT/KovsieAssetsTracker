package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.SearchView;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.adapter.AssetsAdapter;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetRepository;
import com.blackdoorsdev.kovsieassetstracker.remote.network.NetworkUtils;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetViewModel;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.snackbar.Snackbar;

public class AssetsFragment extends Fragment {

    private static final String TAG = "AssetsFragment";

    private SearchView searchViewAssets;
    private RecyclerView recyclerView;
    private CoordinatorLayout coordinatorLayout;
    private TextView tvNoAssets;
    private FloatingActionButton fabSyncAssets;

    private AssetsAdapter assetsAdapter;
    private AssetViewModel assetViewModel;

    public AssetsFragment() {}

    @SuppressLint("ClickableViewAccessibility")
    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_assets, container, false);
        initAndSetupViews(view);

        recyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        AssetRepository assetRepo = AssetRepository.getInstance(requireContext());

        // === Adapter setup ===
        assetsAdapter = new AssetsAdapter(requireContext(), coordinatorLayout, new AssetsAdapter.OnAssetActionListener() {
            @Override
            public void onDeleteAsset(AssetEntity asset) {
                assetRepo.delete(asset);
                Snackbar.make(coordinatorLayout, "Asset deleted locally", Snackbar.LENGTH_SHORT).show();
            }

            @Override
            public void onSaveToServer(AssetEntity asset) {
                if (!NetworkUtils.isNetworkAvailable(requireContext())) {
                    Snackbar.make(coordinatorLayout, "No internet connection", Snackbar.LENGTH_LONG).show();
                    return;
                }

                if (asset.getAssetServerId() != null && asset.getAssetServerId() > 0) {
                    // Update
                    assetRepo.updateAssetOnServer(asset).observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
                } else {
                    // Upload new asset
                    assetRepo.uploadAssetToServer(asset).observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
                }
            }

            @Override
            public void onAssetClicked(AssetEntity asset) {
                Fragment fragment = DetailsAssetFragment.newInstance(asset.getAssetLocalID());
                requireActivity().getSupportFragmentManager()
                        .beginTransaction()
                        .replace(R.id.frameLayout_dashboard, fragment)
                        .addToBackStack(null)
                        .commit();
            }
        });

        recyclerView.setAdapter(assetsAdapter);

        // === ViewModel setup ===
        assetViewModel = new ViewModelProvider(requireActivity()).get(AssetViewModel.class);
        assetViewModel.getAllAssets().observe(getViewLifecycleOwner(), assets -> {
            assetsAdapter.setAssetList(assets);
            updateEmptyView();
        });

        // === Search ===
        searchViewAssets.setOnQueryTextListener(new SearchView.OnQueryTextListener() {
            @Override public boolean onQueryTextSubmit(String query) {
                assetsAdapter.setSearchQuery(query);
                updateEmptyView();
                return true;
            }
            @Override public boolean onQueryTextChange(String newText) {
                assetsAdapter.setSearchQuery(newText);
                updateEmptyView();
                return true;
            }
        });

        // === FAB sync ===
        fabSyncAssets.setOnClickListener(v -> {
            if (!NetworkUtils.isNetworkAvailable(requireContext())) {
                Snackbar.make(coordinatorLayout, "No internet connection", Snackbar.LENGTH_LONG).show();
                return;
            }

            Snackbar loading = Snackbar.make(coordinatorLayout, "Syncing assets from server...", Snackbar.LENGTH_INDEFINITE);
            loading.show();

            assetViewModel.syncWithServer().observe(getViewLifecycleOwner(), msg -> {
                loading.dismiss();
                Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show();
            });
        });

        return view;
    }

    private void updateEmptyView() {
        if (assetsAdapter.getItemCount() == 0) {
            tvNoAssets.setVisibility(View.VISIBLE);
            recyclerView.setVisibility(View.GONE);
        } else {
            tvNoAssets.setVisibility(View.GONE);
            recyclerView.setVisibility(View.VISIBLE);
        }
    }

    private void initAndSetupViews(View view) {
        recyclerView = view.findViewById(R.id.recyclerViewAssets);
        coordinatorLayout = requireActivity().findViewById(R.id.coordinator_dashboard);
        searchViewAssets = view.findViewById(R.id.searchViewAssets);
        tvNoAssets = view.findViewById(R.id.tvNoAssets);
        fabSyncAssets = view.findViewById(R.id.fabSyncAssets);
    }
}
