package com.blackdoorsdev.kovsieassetstracker.remote.api;

import com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.ForgotPasswordRequestDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.LoginRequestDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.auth.LoginResponseDTO;
import com.blackdoorsdev.kovsieassetstracker.remote.dto.shared.ApiResponseDTO;

import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.Multipart;
import retrofit2.http.POST;
import retrofit2.http.Part;

public interface ApiAuthService {

    @POST("Auth/login")
    Call<ApiResponseDTO<LoginResponseDTO>> login(@Body LoginRequestDTO request);

    @Multipart
    @POST("Auth/request-access")
    Call<ApiResponseDTO<String>> requestAccess(
            @Part("DisplayName") RequestBody displayName,
            @Part("Email") RequestBody email,
            @Part("RoleApplied") RequestBody role,
            @Part("Message") RequestBody message,
            @Part MultipartBody.Part PdfDocument
    );

    @POST("Auth/forgot-password")
    Call<ApiResponseDTO<String>> forgotPassword(@Body ForgotPasswordRequestDTO dto);
}
