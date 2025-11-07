package com.blackdoorsdev.kovsieassetstracker.remote.api;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.category.AssetCategoryResponseDTO;
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

public interface ApiCategoryService {

    // ============================================================
    // GET: All Categories
    // ============================================================
    @GET("assetcategories")
    Call<ApiResponseDTO<List<AssetCategoryResponseDTO>>> getAllCategories(
            @Header("Authorization") String token
    );

    // ============================================================
    // GET: All Categories with Assets
    // ============================================================
    @GET("assetcategories/get_all_categories_with_assets")
    Call<ApiResponseDTO<List<AssetCategoryResponseDTO>>> getAllCategoriesWithAssets(
            @Header("Authorization") String token
    );

    // ============================================================
    // GET: Single Category
    // ============================================================
    @GET("assetcategories/{id}")
    Call<ApiResponseDTO<AssetCategoryResponseDTO>> getCategoryById(
            @Header("Authorization") String token,
            @Path("id") int id
    );

    // ============================================================
    // POST: Create Category
    // ============================================================
    @Multipart
    @POST("assetcategories")
    Call<ApiResponseDTO<AssetCategoryResponseDTO>> createCategory(
            @Header("Authorization") String token,
            @Part("AssetCategoryLocalId") RequestBody localId,
            @Part("AssetCategoryName") RequestBody name,
            @Part("AssetCategoryDescription") RequestBody description,
            @Part("IsAndroidBackedUp") RequestBody isAndroid,
            @Part("IsServerBackedUp") RequestBody isServer,
            @Part MultipartBody.Part ImageFile
    );

    // ============================================================
    // PUT: Update Category
    // ============================================================
    @Multipart
    @PUT("assetcategories")
    Call<ApiResponseDTO<AssetCategoryResponseDTO>> updateCategory(
            @Header("Authorization") String token,
            @Part("AssetCategoryServerId") RequestBody serverId,
            @Part("AssetCategoryLocalId") RequestBody localId,
            @Part("AssetCategoryName") RequestBody name,
            @Part("AssetCategoryDescription") RequestBody description,
            @Part("IsAndroidBackedUp") RequestBody isAndroid,
            @Part("IsServerBackedUp") RequestBody isServer,
            @Part MultipartBody.Part ImageFile
    );

    // ============================================================
    // DELETE: Category
    // ============================================================
    @DELETE("assetcategories/{id}")
    Call<ApiResponseDTO<String>> deleteCategory(
            @Header("Authorization") String token,
            @Path("id") int id
    );
}
