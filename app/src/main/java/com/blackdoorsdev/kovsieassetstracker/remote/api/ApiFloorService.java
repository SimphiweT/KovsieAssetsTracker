package com.blackdoorsdev.kovsieassetstracker.remote.api;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.floor.FloorResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.Path;

public interface ApiFloorService {

    // ============================================================
    // GET: All Floors
    // ============================================================
    @GET("floors")
    Call<ApiResponseDTO<List<FloorResponseDTO>>> getAllFloors(
            @Header("Authorization") String token
    );

    // ============================================================
    // GET: All Floors with Rooms
    // ============================================================
    @GET("floors/get_all_floors_with_rooms")
    Call<ApiResponseDTO<List<FloorResponseDTO>>> getAllFloorsWithRooms(
            @Header("Authorization") String token
    );

    // ============================================================
    // GET: Single Floor
    // ============================================================
    @GET("floors/{id}")
    Call<ApiResponseDTO<FloorResponseDTO>> getFloorById(
            @Header("Authorization") String token,
            @Path("id") int id
    );

    // ============================================================
    // GET: Single Floor with Rooms
    // ============================================================
    @GET("floors/get_floor_with_rooms/{id}")
    Call<ApiResponseDTO<FloorResponseDTO>> getFloorByIdWithRooms(
            @Header("Authorization") String token,
            @Path("id") int id
    );
}
