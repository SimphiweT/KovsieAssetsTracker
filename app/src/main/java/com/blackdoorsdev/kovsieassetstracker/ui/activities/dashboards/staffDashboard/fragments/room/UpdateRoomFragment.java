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
import androidx.lifecycle.ViewModelProvider;

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
import com.blackdoorsdev.kovsieassetstracker.ui.viewmodels.RoomViewModel;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.Date;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link UpdateRoomFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class UpdateRoomFragment extends Fragment {

    private EditText txtRoomLocalId, txtRoomServerId, txtRoomName, txtRoomDescription;
    private ImageView roomImageView, roomCaptureImageView;
    private CheckBox isAndroidBackedUp, isServerBackedUp;
    private MaterialButton btnUpdateRoomLocally, btnUpdateRoomServer;
    private CoordinatorLayout coordinatorLayout;
    private String savedRoomImagePath = null;
    private Uri cameraPhotoUri;
    private RoomRepository roomRepository;
    private Date createdDate;
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

    private static final String ARG_ROOM_ID = "room_id";
    private int roomId = -1;

    public UpdateRoomFragment() {
        // Required empty public constructor
    }

    public static UpdateRoomFragment newInstance(int roomId) {
        UpdateRoomFragment fragment = new UpdateRoomFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_ROOM_ID, roomId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            roomId = getArguments().getInt(ARG_ROOM_ID, -1);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_update_room, container, false);
        initViews(view);
        roomRepository = RoomRepository.getInstance(requireContext());
        RoomViewModel roomViewModel = new ViewModelProvider(requireActivity()).get(RoomViewModel.class);

        if(roomId != -1){
            roomViewModel.getRoomById(roomId).observe(getViewLifecycleOwner(), room -> {
                if(room != null){
                    preloadRoomData(room);
                }
            });
        }

        roomImageView.setOnClickListener(v -> pickRoomImageLauncher.launch("image/*"));
        roomCaptureImageView.setOnClickListener(v -> cameraPermissionLauncher.launch(Manifest.permission.CAMERA));

        btnUpdateRoomLocally.setOnClickListener(v -> UpdateRoom(true));
        btnUpdateRoomServer.setOnClickListener(v -> UpdateRoom(false));

        return view;
    }
    private void initViews(View view) {
        txtRoomLocalId = view.findViewById(R.id.txtRoomLocalId);
        txtRoomServerId = view.findViewById(R.id.txtRoomServerId);
        txtRoomName = view.findViewById(R.id.txtRoomName);
        txtRoomDescription = view.findViewById(R.id.txtRoomDescription);
        roomImageView = view.findViewById(R.id.roomImageView);
        roomCaptureImageView = view.findViewById(R.id.roomCaptureImageView);
        isAndroidBackedUp = view.findViewById(R.id.isAndroidBackedUp);
        isServerBackedUp = view.findViewById(R.id.isServerBackedUp);
        btnUpdateRoomLocally = view.findViewById(R.id.btnUpdateRoomLocally);
        btnUpdateRoomServer = view.findViewById(R.id.btnUpdateRoomServer);
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

            Log.d("UpdateRoomFragment", "Saved image: " + file.getAbsolutePath());
            return file.getAbsolutePath();
        } catch (Exception e) {
            Log.e("UpdateRoomFragment", "Error saving bitmap", e);
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
            Log.e("UpdateRoomFragment", "Error saving image from URI", e);
            return null;
        }
    }
    private void preloadRoomData(RoomEntity room) {
        txtRoomLocalId.setText(String.valueOf(room.getRoomLocalId()));
        txtRoomServerId.setText(room.getRoomServerId() !=null ? String.valueOf(room.getRoomServerId()) : "");
        txtRoomName.setText(room.getRoomName());
        txtRoomDescription.setText(room.getRoomDescription());

        //Load Image
        if(room.getRoomImage() != null && !room.getRoomImage().isEmpty()){
            savedRoomImagePath = room.getRoomImage();
            roomImageView.setImageURI(Uri.fromFile(new File(savedRoomImagePath)));
        }

        isAndroidBackedUp.setChecked(room.isAndroidBackedUp());
        isServerBackedUp.setChecked(room.isServerBackedUp());

        createdDate = room.getCreatedDate();
    }
    private void UpdateRoom(boolean local) {

        String localId = txtRoomLocalId.getText().toString().trim();
        Integer iLocalId = null;
        if(!localId.isEmpty())
        {
            try {
                iLocalId = Integer.parseInt(localId);
            } catch (NumberFormatException e) {
                txtRoomLocalId.setError("Provided value is not an Integer. Please provide valid Id");
                return;
            }
        }

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
                iLocalId,
                iServerId,
                name,
                description,
                savedRoomImagePath != null? savedRoomImagePath : "",
                isAndroidBackedUp.isChecked(),
                isServerBackedUp.isChecked(),
                createdDate,
                new Date()
        );

        if(local){
            MyApp.getDatabase().roomDao().update(room);
            Snackbar.make(coordinatorLayout, "Room updated locally", Snackbar.LENGTH_SHORT).show();
        }else {
            roomRepository.updateRoomOnServer(room)
                    .observe(getViewLifecycleOwner(), msg ->
                            Snackbar.make(coordinatorLayout, msg, Snackbar.LENGTH_LONG).show());
        }

        if(roomId == -1) {
            txtRoomLocalId.setText("");
            txtRoomServerId.setText("");
            txtRoomName.setText("");
            txtRoomDescription.setText("");
            roomImageView.setImageResource(R.drawable.outline_broken_image_24);
            savedRoomImagePath = null;
            isAndroidBackedUp.setChecked(false);
            isServerBackedUp.setChecked(false);
        }
    }
}