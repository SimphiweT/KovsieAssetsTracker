package com.blackdoorsdev.kovsieassetstracker.remote.network;

import java.io.File;

import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;

public class MultipartUtils {

    public static RequestBody toRequestBody(String value) {
        return RequestBody.create(okhttp3.MediaType.parse("text/plain"), value == null ? "" : value);
    }

    public static RequestBody toRequestBody(int value) {
        return RequestBody.create(okhttp3.MediaType.parse("text/plain"), String.valueOf(value));
    }

    public static RequestBody toRequestBody(boolean value) {
        return RequestBody.create(okhttp3.MediaType.parse("text/plain"), String.valueOf(value));
    }
    public static RequestBody toRequestBody(Object value) {
        return RequestBody.create(
                MediaType.parse("text/plain"),
                value == null ? "" : String.valueOf(value)
        );
    }

    public static MultipartBody.Part prepareFilePart(String partName, String filePath) {
        if (filePath == null || filePath.isEmpty()) return null;
        File file = new File(filePath);
        RequestBody requestFile = RequestBody.create(MediaType.parse("image/*"), file);
        return MultipartBody.Part.createFormData(partName, file.getName(), requestFile);
    }

}
