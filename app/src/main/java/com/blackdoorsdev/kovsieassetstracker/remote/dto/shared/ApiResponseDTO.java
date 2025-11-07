package com.blackdoorsdev.kovsieassetstracker.remote.dto.shared;

public class ApiResponseDTO<T> {
    private boolean success;
    private String message;
    private String error;
    private T data;

    public boolean isSuccess() { return success; }
    public String getMessage() { return message; }
    public String getError() { return error; }
    public T getData() { return data; }
}
