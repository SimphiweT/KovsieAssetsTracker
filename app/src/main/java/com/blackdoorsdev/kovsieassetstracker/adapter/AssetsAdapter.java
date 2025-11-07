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
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class AssetsAdapter extends RecyclerView.Adapter<AssetsAdapter.AssetViewHolder> {
    private static final String TAG ="AssetAdapter";
    public interface OnAssetActionListener {
        void onDeleteAsset(AssetEntity asset);
        void onSaveToServer(AssetEntity asset);
        void onAssetClicked(AssetEntity asset);
    }

    private List<AssetEntity> assetList = new ArrayList<>();
    private List<AssetEntity> assetListFull = new ArrayList<>();
    private OnAssetActionListener listener;
    private String currentQuery = "";
    private Context context;
    private CoordinatorLayout coordinatorLayout;

    public AssetsAdapter(Context context, CoordinatorLayout coordinatorLayout, OnAssetActionListener listener) {
        this.context = context;
        this.coordinatorLayout = coordinatorLayout;
        this.listener = listener;
    }

    @NonNull
    @Override
    public AssetViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_asset,parent,false);
        return new AssetViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull AssetViewHolder holder, @SuppressLint("RecyclerView") final int position) {
        AssetEntity currentAsset = assetList.get(position);

        // Debug log each bound item
        Log.d(TAG, "onBindViewHolder: position=" + position
                + " | id=" + currentAsset.getAssetLocalID()
                + " | name=" + currentAsset.getAssetName());

        // Click Item
        holder.cardAssetItem.setOnClickListener(v -> {
            listener.onAssetClicked(currentAsset);
            Snackbar.make(coordinatorLayout, "Getting Details...", Snackbar.LENGTH_SHORT).show();
        });

        // Expand / Collapse
        if (currentAsset.isExpanded()) {
            TransitionManager.beginDelayedTransition(holder.cardAssetItem);
            holder.expandedRelLayoutAssetItem.setVisibility(View.VISIBLE);
            holder.btnDownArrowAssetItem.setVisibility(View.GONE);

            // Delete Asset
            holder.btnDeleteAssetItem.setOnClickListener(v -> {
                new AlertDialog.Builder(context)
                        .setMessage("Are you sure you want to delete " + currentAsset.getAssetName() + "?")
                        .setPositiveButton("YES", (dialog, which) -> listener.onDeleteAsset(currentAsset))
                        .setNegativeButton("NO", (dialog, which) -> dialog.dismiss())
                        .create()
                        .show();
            });

            // Push to server later
            holder.btnPushToServerAssetItem.setOnClickListener(v -> listener.onSaveToServer(currentAsset));

        } else {
            TransitionManager.beginDelayedTransition(holder.cardAssetItem);
            holder.expandedRelLayoutAssetItem.setVisibility(View.GONE);
            holder.btnDownArrowAssetItem.setVisibility(View.VISIBLE);
        }

        // Asset Image
        String assetImage = currentAsset.getAssetImage();
        if (assetImage != null && !assetImage.trim().isEmpty()) {
            File imageFile = new File(assetImage);
            Glide.with(context)
                    .load(imageFile.exists() ? imageFile : Uri.parse(assetImage))
                    .error(R.drawable.outline_broken_image_24)
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(holder.ivAssetImageItem);
        } else {
            holder.ivAssetImageItem.setImageResource(R.drawable.outline_broken_image_24);
        }


        // Asset Name
        String assetName = currentAsset.getAssetName();
        if (assetName == null || assetName.isEmpty()) {
            assetName = "No Name";
        }
        holder.tvAssetNameItem.setText(getHighlightedText(assetName, currentQuery, R.color.red));

        // Created Date
        Date createdDate = currentAsset.getCreatedDate();

        holder.tvDatetimeAssetItem.setText(getHighlightedText(String.valueOf(createdDate), currentQuery, R.color.red));

        // Android Backup
        holder.ivIsAndroidBackedUp.setImageResource(
                currentAsset.isAndroidBackedUp()
                        ? R.drawable.android_backup_24px
                        : R.drawable.android_no_backup_24px
        );

        // Server Backup
        holder.ivIsServerBackedUp.setImageResource(
                currentAsset.isServerBackedUp()
                        ? R.drawable.server_backup_24px
                        : R.drawable.server_no_backup_24px
        );

    }

    @Override
    public int getItemCount() {
        return assetList.size();
    }

    @SuppressLint("NotifyDataSetChanged")
    public void setAssetList(List<AssetEntity> assetList) {
        this.assetList = new ArrayList<>(assetList);
        this.assetListFull = new  ArrayList<>(assetList);
        notifyDataSetChanged();

        Log.d(TAG, "setAssetList: total=" + assetList.size());
        for (int i = 0; i < assetList.size(); i++) {
            Log.d(TAG, "Index " + i + " -> " + assetList.get(i));
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
            assetList = new ArrayList<>(assetListFull);
        } else {
            String lowerQuery = query.toLowerCase();
            List<AssetEntity> filtered = new ArrayList<>();
            for (AssetEntity asset : assetListFull) {
                String assetName = asset.getAssetName() != null ? asset.getAssetName().toLowerCase() : "";
                String createdDate = asset.getCreatedDate() != null ? asset.getCreatedDate().toString().toLowerCase() : "";
                if (assetName.contains(lowerQuery) || createdDate.contains(lowerQuery)) {
                    filtered.add(asset);
                }
            }
            assetList = filtered;
        }
        notifyDataSetChanged();
    }


    public class AssetViewHolder extends RecyclerView.ViewHolder{

        private MaterialCardView cardAssetItem;
        private LinearLayout expandedRelLayoutAssetItem;
        private ImageView ivAssetImageItem;
        private TextView tvAssetNameItem;
        private ImageView btnDownArrowAssetItem, btnUpArrowAssetItem;
        private TextView tvDatetimeAssetItem;
        private ImageView ivIsAndroidBackedUp,ivIsServerBackedUp;
        private MaterialButton btnDeleteAssetItem, btnPushToServerAssetItem;
        public AssetViewHolder(@NonNull View itemView) {
            super(itemView);
            cardAssetItem = itemView.findViewById(R.id.cardAssetItem);
            expandedRelLayoutAssetItem = itemView.findViewById(R.id.expandedRelLayoutAssetItem);
            ivAssetImageItem = itemView.findViewById(R.id.ivAssetImageItem);
            tvAssetNameItem = itemView.findViewById(R.id.tvAssetNameItem);
            btnDownArrowAssetItem = itemView.findViewById(R.id.btnDownArrowAssetItem);
            btnUpArrowAssetItem = itemView.findViewById(R.id.btnUpArrowAssetItem);
            tvDatetimeAssetItem = itemView.findViewById(R.id.tvDatetimeAssetItem);
            ivIsAndroidBackedUp = itemView.findViewById(R.id.ivIsAndroidBackedUp);
            ivIsServerBackedUp = itemView.findViewById(R.id.ivIsServerBackedUp);
            btnDeleteAssetItem = itemView.findViewById(R.id.btnDeleteAssetItem);
            btnPushToServerAssetItem = itemView.findViewById(R.id.btnPushToServerAssetItem);

            //Expand
            btnDownArrowAssetItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    AssetEntity asset = assetList.get(getAbsoluteAdapterPosition());
                    asset.setExpanded(!asset.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

            //Collapse
            btnUpArrowAssetItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    AssetEntity asset = assetList.get(getAbsoluteAdapterPosition());
                    asset.setExpanded(!asset.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

        }
    }
}
