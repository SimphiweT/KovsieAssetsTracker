package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.user;

import android.content.Intent;
import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications.ResetPasswordActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.startups.AboutActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.startups.StartupActivity;
import com.google.android.material.button.MaterialButton;

/**
 * A simple {@link Fragment} subclass.
 * Use the  factory method to
 * create an instance of this fragment.
 */
public class UserProfileFragment extends Fragment {

    private MaterialButton btnAbout, btnResetPassword, btnLogout;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_user_profile, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        btnAbout = view.findViewById(R.id.btnAbout);
        btnResetPassword = view.findViewById(R.id.btnResetPassword);
        btnLogout = view.findViewById(R.id.btnLogout);

        btnAbout.setOnClickListener(v ->
                startActivity(new Intent(requireContext(), AboutActivity.class)));

        btnResetPassword.setOnClickListener(v ->
                startActivity(new Intent(requireContext(), ResetPasswordActivity.class)));

        btnLogout.setOnClickListener(v -> {
            // Clear session if needed
            Intent intent = new Intent(requireContext(), StartupActivity.class);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(intent);
        });
    }
}