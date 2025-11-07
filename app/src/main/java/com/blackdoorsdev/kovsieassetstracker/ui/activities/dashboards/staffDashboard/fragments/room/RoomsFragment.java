package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room;

import android.annotation.SuppressLint;
import android.os.Bundle;

import androidx.appcompat.widget.SearchView;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.adapter.RoomAdapter;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.RoomViewModel;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.snackbar.Snackbar;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class RoomsFragment extends Fragment {

    private RecyclerView recyclerViewRooms;
    private TextView tvNoRooms, tvLastSynced;
    private SearchView searchViewRooms;
    private CoordinatorLayout coordinatorLayout;
    private SwipeRefreshLayout swipeRefreshRooms;
    private FloatingActionButton btnSyncRooms;

    private RoomAdapter roomAdapter;
    private RoomViewModel roomViewModel;

    public RoomsFragment() {}

    @SuppressLint("ClickableViewAccessibility")
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_rooms, container, false);
        initAndSetupViews(view);

        // === Setup RecyclerView ===
        recyclerViewRooms.setLayoutManager(new LinearLayoutManager(requireContext()));
        roomAdapter = new RoomAdapter(requireContext(), coordinatorLayout, new RoomAdapter.OnRoomActionListener() {
            @Override
            public void onDeleteRoom(RoomEntity room) {
                roomViewModel.delete(room);
                Snackbar.make(coordinatorLayout, "Room deleted locally", Snackbar.LENGTH_SHORT).show();
            }

            @Override
            public void onSaveToServer(RoomEntity room) {
                roomViewModel.uploadRoom(room).observe(getViewLifecycleOwner(), msg ->
                        Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_SHORT).show());
            }

            @Override
            public void onRoomClicked(RoomEntity room) {
                Fragment fragment = DetailsRoomFragment.newInstance(room.getRoomLocalId());
                requireActivity().getSupportFragmentManager()
                        .beginTransaction()
                        .replace(R.id.frameLayout_dashboard, fragment)
                        .addToBackStack(null)
                        .commit();
            }
        });

        recyclerViewRooms.setAdapter(roomAdapter);
        recyclerViewRooms.setOnTouchListener((v, event) -> roomAdapter.getItemCount() == 0);

        // === Setup ViewModel ===
        roomViewModel = new ViewModelProvider(requireActivity()).get(RoomViewModel.class);

        roomViewModel.getAllRooms().observe(getViewLifecycleOwner(), rooms -> {
            roomAdapter.setRoomList(rooms);
            updateEmptyView();
        });

        // === SearchView Logic ===
        searchViewRooms.setOnQueryTextListener(new SearchView.OnQueryTextListener() {
            @Override
            public boolean onQueryTextSubmit(String query) {
                roomAdapter.setSearchQuery(query);
                updateEmptyView();
                return true;
            }

            @Override
            public boolean onQueryTextChange(String newText) {
                roomAdapter.setSearchQuery(newText);
                updateEmptyView();
                return true;
            }
        });

        // === Swipe-to-Refresh ===
        swipeRefreshRooms.setOnRefreshListener(() -> {
            roomViewModel.syncRoomsFromServer().observe(getViewLifecycleOwner(), message -> {
                swipeRefreshRooms.setRefreshing(false);
                tvLastSynced.setText("Last synced: " + new SimpleDateFormat("dd MMM yyyy, HH:mm", Locale.getDefault()).format(new Date()));
                Snackbar.make(coordinatorLayout, message, Snackbar.LENGTH_SHORT).show();
            });
        });

        // === Floating Action Button Sync ===
        btnSyncRooms.setOnClickListener(v -> {
            swipeRefreshRooms.setRefreshing(true);
            roomViewModel.syncRoomsFromServer().observe(getViewLifecycleOwner(), message -> {
                swipeRefreshRooms.setRefreshing(false);
                tvLastSynced.setText("Last synced: " + new SimpleDateFormat("dd MMM yyyy, HH:mm", Locale.getDefault()).format(new Date()));
                Snackbar.make(coordinatorLayout, message, Snackbar.LENGTH_SHORT).show();
            });
        });

        return view;
    }

    private void updateEmptyView() {
        if (roomAdapter.getItemCount() == 0) {
            tvNoRooms.setVisibility(View.VISIBLE);
            recyclerViewRooms.setVisibility(View.GONE);
        } else {
            tvNoRooms.setVisibility(View.GONE);
            recyclerViewRooms.setVisibility(View.VISIBLE);
        }
    }

    private void initAndSetupViews(View view) {
        recyclerViewRooms = view.findViewById(R.id.recyclerViewRooms);
        tvNoRooms = view.findViewById(R.id.tvNoRooms);
        searchViewRooms = view.findViewById(R.id.searchViewRooms);
        swipeRefreshRooms = view.findViewById(R.id.swipeRefreshRooms);
        coordinatorLayout = view.findViewById(R.id.coordinator_rooms);
        btnSyncRooms = view.findViewById(R.id.btnSyncRooms);
        tvLastSynced = view.findViewById(R.id.tvLastSynced);
    }
}
