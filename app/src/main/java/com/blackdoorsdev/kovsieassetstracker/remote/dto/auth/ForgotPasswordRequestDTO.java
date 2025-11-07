package com.blackdoorsdev.kovsieassetstracker.remote.dto.auth;

public class ForgotPasswordRequestDTO {
    private String email;

    public ForgotPasswordRequestDTO(String email) {
        this.email = email;
    }

    public String getEmail() {
        return email;
    }
}
