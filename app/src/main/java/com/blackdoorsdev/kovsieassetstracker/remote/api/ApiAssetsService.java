package com.blackdoorsdev.kovsieassetstracker.remote.api;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.asset.AssetResponseDTO;
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

/**
 * Retrofit service for interacting with /api/assets endpoints.
 */
public interface ApiAssetsService {

    // ============================================================
    // GET: All assets (with room and category info)
    // ============================================================
    @GET("assets")
    Call<ApiResponseDTO<List<AssetResponseDTO>>> getAllAssets(
            @Header("Authorization") String bearerToken
    );

    // ============================================================
    // GET: Single asset by ID
    // ============================================================
    @GET("assets/{id}")
    Call<ApiResponseDTO<AssetResponseDTO>> getAssetById(
            @Header("Authorization") String bearerToken,
            @Path("id") int id
    );

    // ============================================================
    // POST: Create asset
    // ============================================================
    @Multipart
    @POST("assets")
    Call<ApiResponseDTO<AssetResponseDTO>> createAsset(
            @Header("Authorization") String bearerToken,
            @Part("AssetLocalId") RequestBody assetLocalId,
            @Part("AssetName") RequestBody assetName,
            @Part("AssetDescription") RequestBody assetDescription,
            @Part("QRCodeSecretText") RequestBody qRCodeSecretText,
            @Part("Status") RequestBody status,
            @Part("IsAndroidBackedUp") RequestBody isAndroidBackedUp,
            @Part("IsServerBackedUp") RequestBody isServerBackedUp,
            @Part("RoomServerId") RequestBody roomServerId,
            @Part("AssetCategoryServerId") RequestBody assetCategoryServerId,
            @Part MultipartBody.Part ImageFileAsset,
            @Part MultipartBody.Part ImageFileQrCode
    );

    // ============================================================
    // PUT: Update existing asset
    // ============================================================
    @Multipart
    @PUT("assets/{id}")
    Call<ApiResponseDTO<AssetResponseDTO>> updateAsset(
            @Header("Authorization") String bearerToken,
            @Path("id") int assetServerId,
            @Part("AssetLocalId") RequestBody assetLocalId,
            @Part("AssetName") RequestBody assetName,
            @Part("AssetDescription") RequestBody assetDescription,
            @Part("QRCodeSecretText") RequestBody qRCodeSecretText,
            @Part("Status") RequestBody status,
            @Part("IsAndroidBackedUp") RequestBody isAndroidBackedUp,
            @Part("IsServerBackedUp") RequestBody isServerBackedUp,
            @Part("RoomServerId") RequestBody roomServerId,
            @Part("AssetCategoryServerId") RequestBody assetCategoryServerId,
            @Part MultipartBody.Part ImageFileAsset,
            @Part MultipartBody.Part ImageFileQrCode
    );

    // ============================================================
    // DELETE: Asset
    // ============================================================
    @DELETE("assets/{id}")
    Call<ApiResponseDTO<String>> deleteAsset(
            @Header("Authorization") String bearerToken,
            @Path("id") int id
    );
}
