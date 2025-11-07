package com.blackdoorsdev.kovsieassetstracker.remote.api;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.room.RoomResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;

import java.util.List;

import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.Multipart;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Part;
import retrofit2.http.Path;

public interface ApiRoomService {

    // ============================================================
    // GET: All Rooms
    // ============================================================
    @GET("rooms")
    Call<ApiResponseDTO<List<RoomResponseDTO>>> getAllRooms(
            @Header("Authorization") String token
    );

    // ============================================================
    // GET: Single Room by ID
    // ============================================================
    @GET("rooms/{id}")
    Call<ApiResponseDTO<RoomResponseDTO>> getRoomById(
            @Header("Authorization") String token,
            @Path("id") int id
    );

    // ============================================================
    // POST: Create Room
    // ============================================================
    @Multipart
    @POST("rooms")
    Call<ApiResponseDTO<RoomResponseDTO>> createRoom(
            @Header("Authorization") String token,
            @Part("RoomLocalId") RequestBody roomLocalId,
            @Part("RoomName") RequestBody roomName,
            @Part("RoomDescription") RequestBody roomDescription,
            @Part MultipartBody.Part ImageFile,
            @Part("IsAndroidBackedUp") RequestBody isAndroidBackedUp,
            @Part("IsServerBackedUp") RequestBody isServerBackedUp,
            @Part("FloorServerId") RequestBody floorServerId
    );

    // ============================================================
    // PUT: Update Room
    // ============================================================
    @Multipart
    @PUT("rooms")
    Call<ApiResponseDTO<RoomResponseDTO>> updateRoom(
            @Header("Authorization") String token,
            @Part("RoomServerId") RequestBody roomServerId,
            @Part("RoomLocalId") RequestBody roomLocalId,
            @Part("RoomName") RequestBody roomName,
            @Part("RoomDescription") RequestBody roomDescription,
            @Part MultipartBody.Part ImageFile,
            @Part("IsAndroidBackedUp") RequestBody isAndroidBackedUp,
            @Part("IsServerBackedUp") RequestBody isServerBackedUp,
            @Part("FloorServerId") RequestBody floorServerId
    );

    // ============================================================
    // DELETE: Room
    // ============================================================
    @DELETE("rooms/{id}")
    Call<ApiResponseDTO<String>> deleteRoom(
            @Header("Authorization") String token,
            @Path("id") int id
    );
}
