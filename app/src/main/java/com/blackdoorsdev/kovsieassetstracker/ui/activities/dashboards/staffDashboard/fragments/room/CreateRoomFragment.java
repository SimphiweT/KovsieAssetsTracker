package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room;

import android.Manifest;
import android.content.ContentResolver;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.core.content.FileProvider;
import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageView;

import com.blackdoorsdev.kovsieassetstracker.MyApp;
import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.database.entities.AssetCategoryEntity;
import com.blackdoorsdev.kovsieassetstracker.database.entities.RoomEntity;
import com.blackdoorsdev.kovsieassetstracker.database.repositories.RoomRepository;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.Date;


public class CreateRoomFragment extends Fragment {
    private EditText txtRoomServerId, txtRoomName, txtRoomDescription;
    private ImageView roomImageView, roomCaptureImageView;
    private CheckBox isAndroidBackedUp, isServerBackedUp;
    private MaterialButton btnCreateRoomLocally, btnCreateRoomServer;
    private CoordinatorLayout coordinatorLayout;
    private String savedRoomImagePath = null;
    private Uri cameraPhotoUri;
    private RoomRepository roomRepository;

    // Permissions
    private final ActivityResultLauncher<String> cameraPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) captureAssetPhotoInternal();
                else Snackbar.make(coordinatorLayout, "Camera permission required", Snackbar.LENGTH_SHORT).show();
            });

    // Pick room image from gallery
    private final ActivityResultLauncher<String> pickRoomImageLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    savedRoomImagePath = saveImageFromUri(uri, "room_");
                    if (savedRoomImagePath != null) {
                        Bitmap bitmap = BitmapFactory.decodeFile(savedRoomImagePath);
                        roomImageView.setImageBitmap(bitmap);
                    } else {
                        Snackbar.make(coordinatorLayout, "Failed to load image", Snackbar.LENGTH_SHORT).show();
                    }
                }
            });
    // Take photo with camera
    private File currentPhotoFile; // store real file
    private final ActivityResultLauncher<Uri> takePhotoLauncher =
            registerForActivityResult(new ActivityResultContracts.TakePicture(), result -> {
                if (result && currentPhotoFile != null && currentPhotoFile.exists()) {
                    savedRoomImagePath = currentPhotoFile.getAbsolutePath();
                    Bitmap bitmap = BitmapFactory.decodeFile(savedRoomImagePath);
                    roomImageView.setImageBitmap(bitmap);
                } else {
                    Snackbar.make(coordinatorLayout, "Failed to save photo", Snackbar.LENGTH_SHORT).show();
                }
            });

    public CreateRoomFragment() {

    }


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_create_room, container, false);
        initViews(view);

        roomRepository = RoomRepository.getInstance(requireContext());

        roomImageView.setOnClickListener(v -> pickRoomImageLauncher.launch("image/*"));
        roomCaptureImageView.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));
        btnCreateRoomLocally.setOnClickListener(v -> saveRoom(true));
        btnCreateRoomServer.setOnClickListener(v -> saveRoom(false));

        return view;
    }
    private void initViews(View view) {
        txtRoomServerId = view.findViewById(R.id.txtRoomServerId);
        txtRoomName = view.findViewById(R.id.txtRoomName);
        txtRoomDescription = view.findViewById(R.id.txtRoomDescription);
        roomImageView = view.findViewById(R.id.roomImageView);
        roomCaptureImageView = view.findViewById(R.id.roomCaptureImageView);
        isAndroidBackedUp = view.findViewById(R.id.isAndroidBackedUp);
        isServerBackedUp = view.findViewById(R.id.isServerBackedUp);
        btnCreateRoomLocally = view.findViewById(R.id.btnCreateRoomLocally);
        btnCreateRoomServer = view.findViewById(R.id.btnCreateRoomServer);
        coordinatorLayout = requireActivity().findViewById(R.id.coordinator_dashboard);
    }
    private void captureAssetPhotoInternal() {
        try {
            File photoDir = new File(requireContext().getFilesDir(), "rooms");
            if (!photoDir.exists()) photoDir.mkdirs();

            currentPhotoFile = new File(photoDir, "room_" + System.currentTimeMillis() + ".jpg");
            cameraPhotoUri = FileProvider.getUriForFile(
                    requireContext(),
                    requireContext().getPackageName() + ".provider",
                    currentPhotoFile
            );

            takePhotoLauncher.launch(cameraPhotoUri);
        } catch (Exception e) {
            Snackbar.make(coordinatorLayout, "Error capturing photo: " + e.getMessage(), Snackbar.LENGTH_LONG).show();
        }
    }
    private String saveBitmap(Bitmap bitmap, String prefix) {
        try {
            File dir = new File(requireContext().getFilesDir(), "rooms");
            if (!dir.exists()) dir.mkdirs();

            File file = new File(dir, prefix + System.currentTimeMillis() + ".jpg");
            FileOutputStream fos = new FileOutputStream(file);
            bitmap.compress(Bitmap.CompressFormat.JPEG, 90, fos);
            fos.flush();
            fos.close();

            Log.d("CreateRoomFragment", "Saved image: " + file.getAbsolutePath());
            return file.getAbsolutePath();
        } catch (Exception e) {
            Log.e("CreateRoomFragment", "Error saving bitmap", e);
            Snackbar.make(requireView(), "Failed to save image", Snackbar.LENGTH_LONG).show();
            return null;
        }
    }
    private String saveImageFromUri(Uri uri, String prefix) {
        try {
            ContentResolver resolver = requireActivity().getContentResolver();
            InputStream inputStream = resolver.openInputStream(uri);
            if (inputStream == null) return null;

            Bitmap bitmap = BitmapFactory.decodeStream(inputStream);
            inputStream.close();
            if (bitmap == null) return null;

            return saveBitmap(bitmap, prefix);
        } catch (Exception e) {
            Log.e("CreateRoomFragment", "Error saving image from URI", e);
            return null;
        }
    }
    private void saveRoom(boolean local) {
        String serverId = txtRoomServerId.getText().toString().trim();
        Integer iServerId = null;
        if(!serverId.isEmpty())
        {
            try {
                iServerId = Integer.parseInt(serverId);
            } catch (NumberFormatException e) {
                txtRoomServerId.setError("Provided value is not an Integer. Please provide valid Id");
                return;
            }
        }

        String name = txtRoomName.getText().toString().trim();
        if(name.isEmpty()){
            txtRoomName.setError("Enter category name");
            return;
        }

        String description = txtRoomDescription.getText().toString().trim();
        if(description.isEmpty()){
            txtRoomDescription.setError("Enter category description");
            return;
        }

        RoomEntity room = new RoomEntity(
                iServerId,
                name,
                description,
                savedRoomImagePath != null? savedRoomImagePath : "",
                isAndroidBackedUp.isChecked(),
                isServerBackedUp.isChecked(),
                new Date()
        );

        if(local){
            MyApp.getDatabase().roomDao().create(room);
            Snackbar.make(coordinatorLayout, "Room saved locally", Snackbar.LENGTH_SHORT).show();
        }else {
            roomRepository.uploadRoomToServer(room)
                    .observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());

        }

        //Reset form
        txtRoomServerId.setText("");
        txtRoomName.setText("");
        txtRoomDescription.setText("");
        roomImageView.setImageResource(R.drawable.outline_broken_image_24);
        savedRoomImagePath =null;
        isAndroidBackedUp.setChecked(false);
        isServerBackedUp.setChecked(false);
    }
}