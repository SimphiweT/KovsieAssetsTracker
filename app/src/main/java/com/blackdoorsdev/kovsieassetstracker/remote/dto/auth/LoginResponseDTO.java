package com.blackdoorsdev.kovsieassetstracker.remote.dto.auth;

import java.util.Date;

public class LoginResponseDTO {
    private String userId;
    private String userDisplayName;
    private String userEmail;
    private String userRole;
    private String profilePictureUrl;
    private String token;
    private Date tokenExpirationTime;

    // Getters
    public String getUserId() { return userId; }
    public String getUserDisplayName() { return userDisplayName; }
    public String getUserEmail() { return userEmail; }
    public String getUserRole() { return userRole; }
    public String getProfilePictureUrl() { return profilePictureUrl; }
    public String getToken() { return token; }
    public Date getTokenExpirationTime() { return tokenExpirationTime; }
}
