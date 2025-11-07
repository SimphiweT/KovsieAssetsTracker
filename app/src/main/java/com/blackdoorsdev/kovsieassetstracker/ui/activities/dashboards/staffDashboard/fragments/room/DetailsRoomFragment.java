package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room;

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
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.RoomViewModel;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;

import java.io.File;
import java.text.DateFormat;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link DetailsRoomFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class DetailsRoomFragment extends Fragment {

    private static final String ARG_ROOM_ID = "room_id";

    private RoomViewModel roomViewModel;
    private int roomId;
    private RoomEntity room;

    // UI references
    private MaterialButton btnEdit, btnDelete;
    private ImageView ivRoomImage, ivAndroidBackup, ivServerBackup;
    private TextView tvRoomName, tvRoomDescription, tvCreatedDate, tvUpdatedDate;

    public static DetailsRoomFragment newInstance(int roomId) {
        DetailsRoomFragment fragment = new DetailsRoomFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_ROOM_ID, roomId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            roomId = getArguments().getInt(ARG_ROOM_ID);
        }
        roomViewModel = new ViewModelProvider(requireActivity()).get(RoomViewModel.class);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_details_room, container, false);
        initViews(view);
        return  view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);



        // Delete button
        btnDelete.setOnClickListener(v -> {
            if (room != null) {
                roomViewModel.delete(room);
                requireActivity().getSupportFragmentManager().popBackStack();
            }
        });

        // Edit button
        btnEdit.setOnClickListener(v -> {
            if (room != null) {
                Fragment editFragment = UpdateRoomFragment.newInstance(room.getRoomLocalId());
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
        ivRoomImage = view.findViewById(R.id.ivRoomImage);
        ivAndroidBackup = view.findViewById(R.id.ivAndroidBackup);
        ivServerBackup = view.findViewById(R.id.ivServerBackup);

        tvRoomName = view.findViewById(R.id.tvRoomName);
        tvRoomDescription = view.findViewById(R.id.tvRoomDescription);
        tvCreatedDate = view.findViewById(R.id.tvCreatedDate);
        tvUpdatedDate = view.findViewById(R.id.tvUpdatedDate);

        btnEdit = view.findViewById(R.id.btnEdit);
        btnDelete = view.findViewById(R.id.btnDelete);

        // Load asset details
        roomViewModel.getRoomById(roomId).observe(getViewLifecycleOwner(), roomEntity -> {
            if (roomEntity != null) {
                bindRoomDetails(roomEntity);
            }
        });
    }
    private void bindRoomDetails(RoomEntity room) {
        this.room = room; // store in field

        tvRoomName.setText(room.getRoomName());
        tvRoomDescription.setText(room.getRoomDescription());

        DateFormat df = DateFormat.getDateTimeInstance();

        String createdDateStr = (room.getCreatedDate() != null)
                ? df.format(room.getCreatedDate())
                : "Unknown";
        String updatedDateStr = (room.getUpdatedDate() != null)
                ? df.format(room.getUpdatedDate())
                : "Never";

        tvCreatedDate.setText("Created: " + createdDateStr);
        tvUpdatedDate.setText("Updated: " + updatedDateStr);

        // Load Asset Image
        if (room.getRoomImage() != null && !room.getRoomImage().isEmpty()) {
            Glide.with(this)
                    .load(Uri.fromFile(new File(room.getRoomImage())))
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(ivRoomImage);
        } else {
            ivRoomImage.setImageResource(R.drawable.outline_broken_image_24);
        }

        // Backup indicators
        ivAndroidBackup.setImageResource(room.isAndroidBackedUp()
                ? R.drawable.android_backup_24px
                : R.drawable.android_no_backup_24px);

        ivServerBackup.setImageResource(room.isServerBackedUp()
                ? R.drawable.server_backup_24px
                : R.drawable.server_no_backup_24px);
    }
}