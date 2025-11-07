package com.blackdoorsdev.kovsieassetstracker.remote.dto.asset;

import okhttp3.MultipartBody;
import okhttp3.RequestBody;

/**
 * DTO used for updating an existing Asset on the server via Retrofit.
 * Matches the expected fields of your ASP.NET API's PUT /api/assets/{id} endpoint.
 */
public class UpdateAssetRequestDTO {

    private RequestBody assetServerId;
    private RequestBody assetLocalId;
    private RequestBody assetName;
    private RequestBody assetDescription;
    private RequestBody qRCodeSecretText;
    private RequestBody status;
    private RequestBody isAndroidBackedUp;
    private RequestBody isServerBackedUp;
    private RequestBody roomServerId;
    private RequestBody assetCategoryServerId;
    private MultipartBody.Part imageFileAsset;
    private MultipartBody.Part imageFileQrCode;

    // --- Constructor ---
    public UpdateAssetRequestDTO(RequestBody assetServerId,
                                 RequestBody assetLocalId,
                                 RequestBody assetName,
                                 RequestBody assetDescription,
                                 RequestBody qRCodeSecretText,
                                 RequestBody status,
                                 RequestBody isAndroidBackedUp,
                                 RequestBody isServerBackedUp,
                                 RequestBody roomServerId,
                                 RequestBody assetCategoryServerId,
                                 MultipartBody.Part imageFileAsset,
                                 MultipartBody.Part imageFileQrCode) {
        this.assetServerId = assetServerId;
        this.assetLocalId = assetLocalId;
        this.assetName = assetName;
        this.assetDescription = assetDescription;
        this.qRCodeSecretText = qRCodeSecretText;
        this.status = status;
        this.isAndroidBackedUp = isAndroidBackedUp;
        this.isServerBackedUp = isServerBackedUp;
        this.roomServerId = roomServerId;
        this.assetCategoryServerId = assetCategoryServerId;
        this.imageFileAsset = imageFileAsset;
        this.imageFileQrCode = imageFileQrCode;
    }

    // --- Getters & Setters ---
    public RequestBody getAssetServerId() { return assetServerId; }
    public void setAssetServerId(RequestBody assetServerId) { this.assetServerId = assetServerId; }

    public RequestBody getAssetLocalId() { return assetLocalId; }
    public void setAssetLocalId(RequestBody assetLocalId) { this.assetLocalId = assetLocalId; }

    public RequestBody getAssetName() { return assetName; }
    public void setAssetName(RequestBody assetName) { this.assetName = assetName; }

    public RequestBody getAssetDescription() { return assetDescription; }
    public void setAssetDescription(RequestBody assetDescription) { this.assetDescription = assetDescription; }

    public RequestBody getQRCodeSecretText() { return qRCodeSecretText; }
    public void setQRCodeSecretText(RequestBody qRCodeSecretText) { this.qRCodeSecretText = qRCodeSecretText; }

    public RequestBody getStatus() { return status; }
    public void setStatus(RequestBody status) { this.status = status; }

    public RequestBody getIsAndroidBackedUp() { return isAndroidBackedUp; }
    public void setIsAndroidBackedUp(RequestBody isAndroidBackedUp) { this.isAndroidBackedUp = isAndroidBackedUp; }

    public RequestBody getIsServerBackedUp() { return isServerBackedUp; }
    public void setIsServerBackedUp(RequestBody isServerBackedUp) { this.isServerBackedUp = isServerBackedUp; }

    public RequestBody getRoomServerId() { return roomServerId; }
    public void setRoomServerId(RequestBody roomServerId) { this.roomServerId = roomServerId; }

    public RequestBody getAssetCategoryServerId() { return assetCategoryServerId; }
    public void setAssetCategoryServerId(RequestBody assetCategoryServerId) { this.assetCategoryServerId = assetCategoryServerId; }

    public MultipartBody.Part getImageFileAsset() { return imageFileAsset; }
    public void setImageFileAsset(MultipartBody.Part imageFileAsset) { this.imageFileAsset = imageFileAsset; }

    public MultipartBody.Part getImageFileQrCode() { return imageFileQrCode; }
    public void setImageFileQrCode(MultipartBody.Part imageFileQrCode) { this.imageFileQrCode = imageFileQrCode; }
}
