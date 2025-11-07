package com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard;

import android.app.Dialog;
import android.content.Context;
import android.content.Intent;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.view.Gravity;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.activity.OnBackPressedCallback;
import androidx.annotation.NonNull;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.core.content.ContextCompat;
import androidx.core.view.GravityCompat;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;

import com.blackdoorsdev.kovsieassetstracker.R;
import com.blackdoorsdev.kovsieassetstracker.remote.network.RetrofitClient;
import com.blackdoorsdev.kovsieassetstracker.remote.session.SessionManager;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications.LoginActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.authentications.ResetPasswordActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.AssetsFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.CreateAssetFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset.UpdateAssetFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category.CategoriesFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category.CreateCategoryFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.asset_category.UpdateCategoryFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.Home.HomeFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room.CreateRoomFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room.RoomsFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.dashboards.staffDashboard.fragments.room.UpdateRoomFragment;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.startups.AboutActivity;
import com.blackdoorsdev.kovsieassetstracker.ui.activities.startups.StartupActivity;
import com.bumptech.glide.Glide;
import com.google.android.material.appbar.CollapsingToolbarLayout;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.navigation.NavigationView;
import com.google.android.material.snackbar.Snackbar;

import de.hdodenhof.circleimageview.CircleImageView;

public class StaffDashboardActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener{

    /**
     * Declare views
     */
    private FloatingActionButton fab;
    private DrawerLayout drawerLayout;
    private BottomNavigationView bottomNavigationView;
    private NavigationView navigationView;
    private FrameLayout frameLayout;
    private Toolbar toolbar;
    private CollapsingToolbarLayout collapsingToolbarLayout;
    private CoordinatorLayout coordinatorLayout;
    private ImageView imgActiveStatus;
    private CircleImageView profileImage;
    private TextView tvName, tvRole,tvActiveStatus;
    private ActionBarDrawerToggle toggle;
    private Context context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Set fullscreen programmatically
        setFullscreen();

        setContentView(R.layout.activity_staff_dashboard);
        if (!com.blackdoorsdev.kovsieassetstracker.remote.network.NetworkUtils.isNetworkAvailable(this)) {
            // No network: clear stale IP cache to avoid Retrofit hangs
            new RetrofitClient()
                    .clearCache(this);

            // Inform user visually
            Snackbar.make(findViewById(android.R.id.content),
                    "You are offline. Viewing cached data only.",
                    Snackbar.LENGTH_LONG).show();
        }

        initAndSetup();
        displayUserInfo();

        if(savedInstanceState == null){
            getSupportFragmentManager().beginTransaction().replace(frameLayout.getId(), new HomeFragment()).commit();
        }

        replaceFragments(new HomeFragment());

        bottomNavigationView.setBackground(null);
        bottomNavigationView.setOnItemSelectedListener( menuItem -> {
            int id = menuItem.getItemId();
            if(id == R.id.bottomNavHome_dashboard){
                replaceFragments(new HomeFragment());
            }else if(id == R.id.bottomNavDevices_dashboard){
                replaceFragments(new AssetsFragment());
            } else if(id == R.id.bottomNavCategories_dashboard) {
                replaceFragments(new CategoriesFragment());
            } else if (id == R.id.bottomNavRoom_dashboard) {
                replaceFragments(new RoomsFragment());
            }

            return true;
        });

        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showBottomDialog();
            }
        });

        getOnBackPressedDispatcher().addCallback(this, new OnBackPressedCallback(true) {
            @Override
            public void handleOnBackPressed() {
                if (drawerLayout.isDrawerOpen(GravityCompat.START)) {
                    drawerLayout.closeDrawer(GravityCompat.START);
                } else {
                    // If you still want default system back behavior
                    setEnabled(false);
                    getOnBackPressedDispatcher().onBackPressed();
                }
            }
        });
    }
    private void setFullscreen() {

        getWindow().getDecorView().setSystemUiVisibility(
                View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                        | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                        | View.SYSTEM_UI_FLAG_FULLSCREEN
        );

        if (getSupportActionBar() != null) {
            getSupportActionBar().show();
        }
    }


    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        if (hasFocus) {
            setFullscreen();
        }
    }
    private void initAndSetup(){
        /**
         * Initialize Fields and properties
         */
        context = StaffDashboardActivity.this;

        /**
         * Initialize views
         */
        fab = findViewById(R.id.fab_dashboard);
        drawerLayout= findViewById(R.id.drawerLayout_dashboard);
        bottomNavigationView = findViewById(R.id.bottomNavigationView_dashboard);
        navigationView = findViewById(R.id.navView_dashboard);
        frameLayout = findViewById(R.id.frameLayout_dashboard);
        toolbar = findViewById(R.id.toolbar_dashboard);
        collapsingToolbarLayout = findViewById(R.id.collapsingToolbar_dashboard);

        profileImage = findViewById(R.id.profile_image);
        tvName = findViewById(R.id.usernameTV);
        tvRole = findViewById(R.id.roleNameTV);
        tvActiveStatus = findViewById(R.id.statusDescriptionTV);
        imgActiveStatus = findViewById(R.id.imgUserActiveStatus);

        coordinatorLayout = findViewById(R.id.coordinator_dashboard);

        /**
         * Configure Fields and Properties
         */
        setSupportActionBar(toolbar);

        collapsingToolbarLayout.setTitle(""); //Hide title in expanded state
        collapsingToolbarLayout.setExpandedTitleColor(ContextCompat.getColor(context, android.R.color.transparent));
        collapsingToolbarLayout.setCollapsedTitleTextColor(ContextCompat.getColor(context, android.R.color.white));



        Glide.with(context)
                .load(R.drawable.ic_profile)
                .placeholder(R.drawable.ic_profile)
                .circleCrop()
                .into(profileImage);

        toggle = new ActionBarDrawerToggle(StaffDashboardActivity.this,drawerLayout,toolbar,R.string.open_nav,R.string.close_nav);
        drawerLayout.addDrawerListener(toggle);
        toggle.syncState();
        toggle.setDrawerIndicatorEnabled(true);
        navigationView.setNavigationItemSelectedListener(this);


    }
    private void displayUserInfo() {
        SessionManager session = new SessionManager(context);
        String token = session.getToken();
        String name = session.getDisplayName();
        String role = session.getRole();
        String profileUrl = session.getProfileUrl();

        // Default offline state (safe fallback)
        tvName.setText(name != null && !name.isEmpty() ? name : "Unknown User");
        tvRole.setText(role != null && !role.isEmpty() ? role : "No role");
        imgActiveStatus.setImageResource(R.drawable.ic_offline);
        tvActiveStatus.setText("Offline");
        tvActiveStatus.setTextColor(ContextCompat.getColor(context, R.color.red));

        // Load default / cached profile image
        Glide.with(context)
                .load(profileUrl != null && !profileUrl.isEmpty() ? profileUrl : R.drawable.ic_profile)
                .placeholder(R.drawable.ic_profile)
                .error(R.drawable.ic_profile)
                .circleCrop()
                .into(profileImage);

        // --- Check network and token status ---
        boolean hasNetwork = com.blackdoorsdev.kovsieassetstracker.remote.network.NetworkUtils.isNetworkAvailable(context);
        boolean hasValidToken = token != null && !token.isEmpty();

        if (hasNetwork && hasValidToken) {
            // Online mode
            tvActiveStatus.setText("Online");
            tvActiveStatus.setTextColor(ContextCompat.getColor(context, R.color.green));
            imgActiveStatus.setImageResource(R.drawable.ic_online);
        } else {
            // Offline mode or expired token → reset profile to offline
            session.clearToken(); // optional: clear invalid token
            Glide.with(context)
                    .load(R.drawable.ic_profile)
                    .placeholder(R.drawable.ic_profile)
                    .circleCrop()
                    .into(profileImage);
            tvActiveStatus.setText("Offline");
            tvActiveStatus.setTextColor(ContextCompat.getColor(context, R.color.red));
            imgActiveStatus.setImageResource(R.drawable.ic_offline);
        }
    }


    private void replaceFragments(Fragment fragment){
        FragmentManager fragmentManager = getSupportFragmentManager();
        FragmentTransaction fragmentTransaction = fragmentManager.beginTransaction();
        fragmentTransaction.replace(R.id.frameLayout_dashboard,fragment);
        fragmentTransaction.commit();
    }

    private void showBottomDialog(){

        final Dialog dialog = new Dialog(context);
        dialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
        dialog.setContentView(R.layout.bottomsheet_layout);

        LinearLayout createDeviceLayout = dialog.findViewById(R.id.layoutCreateAsset_dashboard);
        LinearLayout createRoomLayout = dialog.findViewById(R.id.layoutCreateRoom_dashboard);
        LinearLayout updateDeviceLayout = dialog.findViewById(R.id.layoutUpdateAsset_dashboard);
        LinearLayout updateRoomLayout = dialog.findViewById(R.id.layoutUpdateRoom_dashboard);

        createDeviceLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                replaceFragments(new CreateAssetFragment());
            }
        });

        createRoomLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                replaceFragments(new CreateRoomFragment());
            }
        });

        updateDeviceLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                replaceFragments(new UpdateAssetFragment());
            }
        });

        updateRoomLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                replaceFragments(new UpdateRoomFragment());
            }
        });
        dialog.show();;
        dialog.getWindow().setLayout(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        dialog.getWindow().getAttributes().windowAnimations = R.style.DialogAnimation;
        dialog.getWindow().setGravity(Gravity.BOTTOM);
    }

    @Override
    public boolean onNavigationItemSelected(@NonNull MenuItem item) {

        int id = item.getItemId();

        if (id == R.id.navProfile_dashboard) {
            Snackbar.make(coordinatorLayout, "To User Profile Activity", Snackbar.LENGTH_SHORT).show();

        } else if (id == R.id.navChangePassword_dashboard) {
            Intent intent = new Intent(StaffDashboardActivity.this, ResetPasswordActivity.class);
            Snackbar.make(coordinatorLayout, "To Change Password Activity", Snackbar.LENGTH_SHORT).show();
            intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_NEW_TASK);
            startActivity(intent);
            finish();

        } else if (id == R.id.navCreateCategory_dashboard) {
            replaceFragments(new CreateCategoryFragment());
        } else if (id == R.id.navUpdateCategory_dashboard) {
            replaceFragments(new UpdateCategoryFragment());
        } else if (id == R.id.navLogout_dashboard) {
            new androidx.appcompat.app.AlertDialog.Builder(context)
                    .setTitle("Logout")
                    .setMessage("Are you sure you want to log out?")
                    .setPositiveButton("Yes", (dialog, which) -> {
                        Snackbar.make(coordinatorLayout, "Logging out...", Snackbar.LENGTH_SHORT).show();

                        drawerLayout.postDelayed(() -> {
                            SessionManager session = new SessionManager(context);
                            session.clearAllSessionsAndCache(context);

                            Intent intent = new Intent(StaffDashboardActivity.this, StartupActivity.class);
                            intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_NEW_TASK);
                            startActivity(intent);
                            finish();
                        }, 500);
                    })
                    .setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss())
                    .show();
        }


        //drawerLayout.postDelayed(() -> drawerLayout.closeDrawer(GravityCompat.START), 150);
        drawerLayout.closeDrawer(GravityCompat.START);
        return true;
    }
    @Override
    protected void onResume() {
        super.onResume();
        displayUserInfo();
    }
}
