package com.blackdoorsdev.kovsieassetstracker.adapter;

import android.annotation.SuppressLint;
import android.app.AlertDialog;
import android.content.Context;
import android.net.Uri;
import android.text.Spannable;
import android.text.SpannableString;
import android.text.style.ForegroundColorSpan;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;
import androidx.transition.TransitionManager;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class RoomAdapter extends RecyclerView.Adapter<RoomAdapter.RoomViewHolder> {
    private static final String TAG ="RoomAdapter";
    public interface OnRoomActionListener {
        void onDeleteRoom(RoomEntity room);
        void onSaveToServer(RoomEntity room);
        void onRoomClicked(RoomEntity room);
    }

    private List<RoomEntity> roomList = new ArrayList<>();
    private List<RoomEntity> roomListFull = new ArrayList<>();
    private OnRoomActionListener listener;
    private String currentQuery = "";
    private Context context;
    private CoordinatorLayout coordinatorLayout;

    public RoomAdapter(Context context, CoordinatorLayout coordinatorLayout, OnRoomActionListener listener) {
        this.context = context;
        this.coordinatorLayout = coordinatorLayout;
        this.listener = listener;
    }

    @NonNull
    @Override
    public RoomViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_room,parent,false);
        return new RoomViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull RoomViewHolder holder, @SuppressLint("RecyclerView") final int position) {
        RoomEntity currentRoom = roomList.get(position);

        // Debug log each bound item
        Log.d(TAG, "onBindViewHolder: position=" + position
                + " | id=" + currentRoom.getRoomLocalId()
                + " | name=" + currentRoom.getRoomName());

        // Click Item
        holder.cardRoomItem.setOnClickListener(v -> {
            listener.onRoomClicked(currentRoom);
            Snackbar.make(coordinatorLayout, "Getting Details...", Snackbar.LENGTH_SHORT).show();
        });

        // Expand / Collapse
        if (currentRoom.isExpanded()) {
            TransitionManager.beginDelayedTransition(holder.cardRoomItem);
            holder.expandedRelLayoutRoomItem.setVisibility(View.VISIBLE);
            holder.btnDownArrowRoomItem.setVisibility(View.GONE);

            // Delete Room
            holder.btnDeleteRoomItem.setOnClickListener(v -> {
                new AlertDialog.Builder(context)
                        .setMessage("Are you sure you want to delete " + currentRoom.getRoomName() + "?")
                        .setPositiveButton("YES", (dialog, which) -> listener.onDeleteRoom(currentRoom))
                        .setNegativeButton("NO", (dialog, which) -> dialog.dismiss())
                        .create()
                        .show();
            });

            // Push to server later
            holder.btnPushToServerRoomItem.setOnClickListener(v -> listener.onSaveToServer(currentRoom));

        } else {
            TransitionManager.beginDelayedTransition(holder.cardRoomItem);
            holder.expandedRelLayoutRoomItem.setVisibility(View.GONE);
            holder.btnDownArrowRoomItem.setVisibility(View.VISIBLE);
        }

        // Room Image
        String roomImage = currentRoom.getRoomImage();
        if (roomImage != null && !roomImage.trim().isEmpty()) {
            File imageFile = new File(roomImage);
            Glide.with(context)
                    .load(imageFile.exists() ? imageFile : Uri.parse(roomImage))
                    .error(R.drawable.outline_broken_image_24)
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(holder.ivRoomImageItem);
        } else {
            holder.ivRoomImageItem.setImageResource(R.drawable.outline_broken_image_24);
        }


        // Room Name
        String roomName = currentRoom.getRoomName();
        if (roomName == null || roomName.isEmpty()) {
            roomName = "No Name";
        }
        holder.tvRoomNameItem.setText(getHighlightedText(roomName, currentQuery, R.color.red));

        // Created Date
        Date createdDate = currentRoom.getCreatedDate();

        holder.tvDatetimeRoomItem.setText(getHighlightedText(String.valueOf(createdDate), currentQuery, R.color.red));

        // Android Backup
        holder.ivIsAndroidBackedUp.setImageResource(
                currentRoom.isAndroidBackedUp()
                        ? R.drawable.android_backup_24px
                        : R.drawable.android_no_backup_24px
        );

        // Server Backup
        holder.ivIsServerBackedUp.setImageResource(
                currentRoom.isServerBackedUp()
                        ? R.drawable.server_backup_24px
                        : R.drawable.server_no_backup_24px
        );

    }

    @Override
    public int getItemCount() {
        return roomList.size();
    }

    @SuppressLint("NotifyDataSetChanged")
    public void setRoomList(List<RoomEntity> roomList) {
        this.roomList = new ArrayList<>(roomList);
        this.roomListFull = new  ArrayList<>(roomList);
        notifyDataSetChanged();

        Log.d(TAG, "setAssetList: total=" + roomList.size());
        for (int i = 0; i < roomList.size(); i++) {
            Log.d(TAG, "Index " + i + " -> " + roomList.get(i));
        }
    }

    public void setSearchQuery(String query) {
        currentQuery = query != null ? query.toLowerCase() : "";
        filter(currentQuery); // optional: keep filtering in sync
    }
    private SpannableString getHighlightedText(String text, String query, int colorRes) {
        SpannableString spannable = new SpannableString(text);
        if (!query.isEmpty()) {
            String lowerText = text.toLowerCase();
            String lowerQuery = query.toLowerCase();
            int start = 0;

            while ((start = lowerText.indexOf(lowerQuery, start)) >= 0) {
                int end = start + lowerQuery.length();
                int color = ContextCompat.getColor(context, colorRes);
                spannable.setSpan(new ForegroundColorSpan(color), start, end, Spannable.SPAN_EXCLUSIVE_EXCLUSIVE);
                start = end; // move past this match
            }
        }
        return spannable;
    }


    @SuppressLint("NotifyDataSetChanged")
    public void filter(String query) {
        if (query == null || query.trim().isEmpty()) {
            roomList = new ArrayList<>(roomListFull);
        } else {
            String lowerQuery = query.toLowerCase();
            List<RoomEntity> filtered = new ArrayList<>();
            for (RoomEntity room : roomListFull) {
                String assetName = room.getRoomName() != null ? room.getRoomImage().toLowerCase() : "";
                String createdDate = room.getCreatedDate() != null ? room.getCreatedDate().toString().toLowerCase() : "";
                if (assetName.contains(lowerQuery) || createdDate.contains(lowerQuery)) {
                    filtered.add(room);
                }
            }
            roomList = filtered;
        }
        notifyDataSetChanged();
    }


    public class RoomViewHolder extends RecyclerView.ViewHolder{

        private MaterialCardView cardRoomItem;
        private LinearLayout expandedRelLayoutRoomItem;
        private ImageView ivRoomImageItem;
        private TextView tvRoomNameItem;
        private ImageView btnDownArrowRoomItem, btnUpArrowRoomItem;
        private TextView tvDatetimeRoomItem;
        private ImageView ivIsAndroidBackedUp,ivIsServerBackedUp;
        private MaterialButton btnDeleteRoomItem, btnPushToServerRoomItem;
        public RoomViewHolder(@NonNull View itemView) {
            super(itemView);
            cardRoomItem = itemView.findViewById(R.id.cardRoomItem);
            expandedRelLayoutRoomItem = itemView.findViewById(R.id.expandedRelLayoutRoomItem);
            ivRoomImageItem = itemView.findViewById(R.id.ivRoomImageItem);
            tvRoomNameItem = itemView.findViewById(R.id.tvRoomNameItem);
            btnDownArrowRoomItem = itemView.findViewById(R.id.btnDownArrowRoomItem);
            btnUpArrowRoomItem = itemView.findViewById(R.id.btnUpArrowRoomItem);
            tvDatetimeRoomItem = itemView.findViewById(R.id.tvDatetimeRoomItem);
            ivIsAndroidBackedUp = itemView.findViewById(R.id.ivIsAndroidBackedUp);
            ivIsServerBackedUp = itemView.findViewById(R.id.ivIsServerBackedUp);
            btnDeleteRoomItem = itemView.findViewById(R.id.btnDeleteRoomItem);
            btnPushToServerRoomItem = itemView.findViewById(R.id.btnPushToServerRoomItem);

            //Expand
            btnDownArrowRoomItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    RoomEntity room = roomList.get(getAbsoluteAdapterPosition());
                    room.setExpanded(!room.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

            //Collapse
            btnUpArrowRoomItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    RoomEntity room = roomList.get(getAbsoluteAdapterPosition());
                    room.setExpanded(!room.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

        }
    }

}
