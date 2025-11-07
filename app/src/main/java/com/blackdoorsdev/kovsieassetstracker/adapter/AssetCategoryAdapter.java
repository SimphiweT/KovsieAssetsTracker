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
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class AssetCategoryAdapter extends RecyclerView.Adapter<AssetCategoryAdapter.CategoryViewHolder> {
    private static final String TAG ="AssetCategoryAdapter";
    public interface OnCategoryActionListener {
        void onDeleteCategory(AssetCategoryEntity category);
        void onSaveToServer(AssetCategoryEntity category);
        void onCategoryClicked(AssetCategoryEntity category);
    }

    private List<AssetCategoryEntity> categoryList = new ArrayList<>();
    private List<AssetCategoryEntity> categoryListFull = new ArrayList<>();
    private OnCategoryActionListener listener;
    private String currentQuery = "";
    private Context context;
    private CoordinatorLayout coordinatorLayout;

    public AssetCategoryAdapter(Context context, CoordinatorLayout coordinatorLayout, OnCategoryActionListener listener) {
        this.context = context;
        this.coordinatorLayout = coordinatorLayout;
        this.listener = listener;
    }

    @NonNull
    @Override
    public CategoryViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_category,parent,false);
        return new CategoryViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull CategoryViewHolder holder, @SuppressLint("RecyclerView") final int position) {
        AssetCategoryEntity currentCategory = categoryList.get(position);

        // Debug log each bound item
        Log.d(TAG, "onBindViewHolder: position=" + position
                + " | id=" + currentCategory.getAssetCategoryLocalId()
                + " | name=" + currentCategory.getAssetCategoryName());

        // Click Item
        holder.cardCategoryItem.setOnClickListener(v -> {
            listener.onCategoryClicked(currentCategory);
            Snackbar.make(coordinatorLayout, "Getting Details...", Snackbar.LENGTH_SHORT).show();
        });

        // Expand / Collapse
        if (currentCategory.isExpanded()) {
            TransitionManager.beginDelayedTransition(holder.cardCategoryItem);
            holder.expandedRelLayoutCategoryItem.setVisibility(View.VISIBLE);
            holder.btnDownArrowCategoryItem.setVisibility(View.GONE);

            // Delete Category
            holder.btnDeleteCategoryItem.setOnClickListener(v -> {
                new AlertDialog.Builder(context)
                        .setMessage("Are you sure you want to delete " + currentCategory.getAssetCategoryName() + "?")
                        .setPositiveButton("YES", (dialog, which) -> listener.onDeleteCategory(currentCategory))
                        .setNegativeButton("NO", (dialog, which) -> dialog.dismiss())
                        .create()
                        .show();
            });

            // Push to server later
            holder.btnPushToServerCategoryItem.setOnClickListener(v -> listener.onSaveToServer(currentCategory));

        } else {
            TransitionManager.beginDelayedTransition(holder.cardCategoryItem);
            holder.expandedRelLayoutCategoryItem.setVisibility(View.GONE);
            holder.btnDownArrowCategoryItem.setVisibility(View.VISIBLE);
        }

        // Category Image
        String assetImage = currentCategory.getAssetCategoryImage();
        if (assetImage != null && !assetImage.trim().isEmpty()) {
            File imageFile = new File(assetImage);
            Glide.with(context)
                    .load(imageFile.exists() ? imageFile : Uri.parse(assetImage))
                    .error(R.drawable.outline_broken_image_24)
                    .placeholder(R.drawable.outline_broken_image_24)
                    .into(holder.ivCategoryImageItem);
        } else {
            holder.ivCategoryImageItem.setImageResource(R.drawable.outline_broken_image_24);
        }


        // Category Name
        String categoryName = currentCategory.getAssetCategoryName();
        if (categoryName == null || categoryName.isEmpty()) {
            categoryName = "No Name";
        }
        holder.tvCategoryNameItem.setText(getHighlightedText(categoryName, currentQuery, R.color.red));

        // Created Date
        Date createdDate = currentCategory.getCreatedDate();

        holder.tvDatetimeCategoryItem.setText(getHighlightedText(String.valueOf(createdDate), currentQuery, R.color.red));

        // Android Backup
        holder.ivIsAndroidBackedUp.setImageResource(
                currentCategory.isAndroidBackedUp()
                        ? R.drawable.android_backup_24px
                        : R.drawable.android_no_backup_24px
        );

        // Server Backup
        holder.ivIsServerBackedUp.setImageResource(
                currentCategory.isServerBackedUp()
                        ? R.drawable.server_backup_24px
                        : R.drawable.server_no_backup_24px
        );

    }

    @Override
    public int getItemCount() {
        return categoryList.size();
    }

    @SuppressLint("NotifyDataSetChanged")
    public void setCategoryList(List<AssetCategoryEntity> categoryList) {
        this.categoryList = new ArrayList<>(categoryList);
        this.categoryListFull = new  ArrayList<>(categoryList);
        notifyDataSetChanged();

        Log.d(TAG, "setCategoryList: total=" + categoryList.size());
        for (int i = 0; i < categoryList.size(); i++) {
            Log.d(TAG, "Index " + i + " -> " + categoryList.get(i));
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
            categoryList = new ArrayList<>(categoryListFull);
        } else {
            String lowerQuery = query.toLowerCase();
            List<AssetCategoryEntity> filtered = new ArrayList<>();
            for (AssetCategoryEntity category : categoryListFull) {
                String assetName = category.getAssetCategoryName() != null ? category.getAssetCategoryName().toLowerCase() : "";
                String createdDate = category.getCreatedDate() != null ? category.getCreatedDate().toString().toLowerCase() : "";
                if (assetName.contains(lowerQuery) || createdDate.contains(lowerQuery)) {
                    filtered.add(category);
                }
            }
            categoryList = filtered;
        }
        notifyDataSetChanged();
    }


    public class CategoryViewHolder extends RecyclerView.ViewHolder{

        private MaterialCardView cardCategoryItem;
        private LinearLayout expandedRelLayoutCategoryItem;
        private ImageView ivCategoryImageItem;
        private TextView tvCategoryNameItem;
        private ImageView btnDownArrowCategoryItem, btnUpArrowCategoryItem;
        private TextView tvDatetimeCategoryItem;
        private ImageView ivIsAndroidBackedUp,ivIsServerBackedUp;
        private MaterialButton btnDeleteCategoryItem, btnPushToServerCategoryItem;
        public CategoryViewHolder(@NonNull View itemView) {
            super(itemView);
            cardCategoryItem = itemView.findViewById(R.id.cardCategoryItem);
            expandedRelLayoutCategoryItem = itemView.findViewById(R.id.expandedRelLayoutCategoryItem);
            ivCategoryImageItem = itemView.findViewById(R.id.ivCategoryImageItem);
            tvCategoryNameItem = itemView.findViewById(R.id.tvCategoryNameItem);
            btnDownArrowCategoryItem = itemView.findViewById(R.id.btnDownArrowCategoryItem);
            btnUpArrowCategoryItem = itemView.findViewById(R.id.btnUpArrowCategoryItem);
            tvDatetimeCategoryItem = itemView.findViewById(R.id.tvDatetimeCategoryItem);
            ivIsAndroidBackedUp = itemView.findViewById(R.id.ivIsAndroidBackedUp);
            ivIsServerBackedUp = itemView.findViewById(R.id.ivIsServerBackedUp);
            btnDeleteCategoryItem = itemView.findViewById(R.id.btnDeleteCategoryItem);
            btnPushToServerCategoryItem = itemView.findViewById(R.id.btnPushToServerCategoryItem);

            //Expand
            btnDownArrowCategoryItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    AssetCategoryEntity category = categoryList.get(getAbsoluteAdapterPosition());
                    category.setExpanded(!category.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

            //Collapse
            btnUpArrowCategoryItem.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    AssetCategoryEntity category = categoryList.get(getAbsoluteAdapterPosition());
                    category.setExpanded(!category.isExpanded());
                    notifyItemChanged(getAbsoluteAdapterPosition());
                }
            });

        }
    }
}
