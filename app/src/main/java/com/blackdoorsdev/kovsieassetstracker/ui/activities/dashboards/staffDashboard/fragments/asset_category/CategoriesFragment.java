package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category;

import android.annotation.SuppressLint;
import android.os.Bundle;

import androidx.appcompat.widget.SearchView;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.adapter.AssetCategoryAdapter;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.AssetCategoryRepository;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.AssetCategoryViewModel;
import com.google.android.material.snackbar.Snackbar;

public class CategoriesFragment extends Fragment {

    private static final String TAG = "CategoriesFragment";

    private SearchView searchViewCategories;
    private RecyclerView recyclerView;
    private CoordinatorLayout coordinatorLayout;
    private TextView tvNoCategories;
    private ImageButton btnSyncCategories;
    private AssetCategoryAdapter categoriesAdapter;
    private AssetCategoryViewModel categoryViewModel;
    private AssetCategoryRepository categoryRepository;

    public CategoriesFragment() {
        // Required empty public constructor
    }

    @SuppressLint("ClickableViewAccessibility")
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_categories, container, false);

        initAndSetupViews(view);

        // Repository + ViewModel setup
        categoryRepository = AssetCategoryRepository.getInstance(requireContext());
        categoryViewModel = new ViewModelProvider(requireActivity()).get(AssetCategoryViewModel.class);

        // Setup RecyclerView
        recyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        categoriesAdapter = new AssetCategoryAdapter(requireContext(), coordinatorLayout,
                new AssetCategoryAdapter.OnCategoryActionListener() {
                    @Override
                    public void onDeleteCategory(AssetCategoryEntity category) {
                        categoryViewModel.delete(category);
                        Snackbar.make(coordinatorLayout, "Category deleted locally", Snackbar.LENGTH_SHORT).show();
                    }

                    @Override
                    public void onSaveToServer(AssetCategoryEntity category) {
                        if (category.getAssetCategoryServerId() != null && category.getAssetCategoryServerId() > 0) {
                            categoryRepository.updateCategoryOnServer(category)
                                    .observe(getViewLifecycleOwner(), msg ->
                                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
                        } else {
                            categoryRepository.uploadCategoryToServer(category)
                                    .observe(getViewLifecycleOwner(), msg ->
                                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
                        }
                    }

                    @Override
                    public void onCategoryClicked(AssetCategoryEntity category) {
                        Fragment fragment = DetailsCategoryFragment.newInstance(category.getAssetCategoryLocalId());
                        requireActivity().getSupportFragmentManager()
                                .beginTransaction()
                                .replace(R.id.frameLayout_dashboard, fragment)
                                .addToBackStack(null)
                                .commit();
                        Snackbar.make(coordinatorLayout,
                                "Opening category details: " + category.getAssetCategoryName(),
                                Snackbar.LENGTH_SHORT).show();
                    }
                });

        recyclerView.setAdapter(categoriesAdapter);

        // Observe LiveData from ViewModel
        categoryViewModel.getAllCategories().observe(getViewLifecycleOwner(), categories -> {
            if (categories != null) {
                categoriesAdapter.setCategoryList(categories);
                updateEmptyView();
            }
        });

        // SearchView listener
        searchViewCategories.setOnQueryTextListener(new SearchView.OnQueryTextListener() {
            @Override
            public boolean onQueryTextSubmit(String query) {
                categoriesAdapter.setSearchQuery(query);
                updateEmptyView();
                return true;
            }

            @Override
            public boolean onQueryTextChange(String newText) {
                categoriesAdapter.setSearchQuery(newText);
                updateEmptyView();
                return true;
            }
        });

        // Handle Sync button click
        btnSyncCategories.setOnClickListener(v -> {
            Snackbar.make(coordinatorLayout, "Syncing categories...", Snackbar.LENGTH_SHORT).show();
            categoryRepository.syncCategoriesFromServer()
                    .observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
        });

        return view;
    }

    private void updateEmptyView() {
        if (categoriesAdapter.getItemCount() == 0) {
            tvNoCategories.setVisibility(View.VISIBLE);
            recyclerView.setVisibility(View.GONE);
        } else {
            tvNoCategories.setVisibility(View.GONE);
            recyclerView.setVisibility(View.VISIBLE);
        }
    }

    private void initAndSetupViews(View view) {
        recyclerView = view.findViewById(R.id.recyclerViewCategories);
        coordinatorLayout = requireActivity().findViewById(R.id.coordinator_dashboard);
        searchViewCategories = view.findViewById(R.id.searchViewCategories);
        tvNoCategories = view.findViewById(R.id.tvNoCategories);
        btnSyncCategories = view.findViewById(R.id.btnSyncCategories);
    }
}
