package com.blackdoorsdev.kovsieassetstracker.helpers;

import android.app.Activity;
import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.graphics.Color;
import android.os.Handler;
import android.os.Looper;
import android.view.Gravity;
import android.view.ViewGroup;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.widget.FrameLayout;
import android.widget.TextView;

/**
 * Developer-only overlay to display environment info (e.g., API URL).
 * Automatically fades out after 5s and is disabled in release builds.
 */
public class DebugOverlay {

    private static TextView overlayView;
    private static final Handler handler = new Handler(Looper.getMainLooper());

    /** Checks if this is a debug build */
    private static boolean isDebug(Context context) {
        return (context.getApplicationInfo().flags & ApplicationInfo.FLAG_DEBUGGABLE) != 0;
    }

    /** Displays overlay text for developers */
    public static void show(Activity activity, String text) {
        if (!isDebug(activity)) return; // Skip in release builds

        handler.post(() -> {
            if (overlayView == null) {
                overlayView = new TextView(activity);
                overlayView.setTextColor(Color.WHITE);
                overlayView.setBackgroundColor(Color.parseColor("#66000000"));
                overlayView.setTextSize(12);
                overlayView.setPadding(20, 10, 20, 10);
                overlayView.setGravity(Gravity.CENTER);
                overlayView.setElevation(20f);
            }

            overlayView.setText("Connected → " + text);

            FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
                    ViewGroup.LayoutParams.WRAP_CONTENT,
                    ViewGroup.LayoutParams.WRAP_CONTENT
            );
            params.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
            params.bottomMargin = 40;

            if (overlayView.getParent() == null) {
                activity.addContentView(overlayView, params);
            }

            // Cancel any previous fade timers
            handler.removeCallbacksAndMessages(null);

            // Auto-fade out after 5 seconds
            handler.postDelayed(() -> fadeOut(), 5000);
        });
    }

    /** Fades out and removes overlay */
    private static void fadeOut() {
        if (overlayView == null) return;

        AlphaAnimation fade = new AlphaAnimation(1.0f, 0.0f);
        fade.setDuration(600);
        fade.setFillAfter(true);
        fade.setAnimationListener(new Animation.AnimationListener() {
            @Override public void onAnimationStart(Animation animation) {}
            @Override public void onAnimationRepeat(Animation animation) {}
            @Override
            public void onAnimationEnd(Animation animation) {
                hide(); // Remove view after fade
            }
        });
        overlayView.startAnimation(fade);
    }

    /** Hides the overlay manually */
    public static void hide() {
        if (overlayView != null) {
            ViewGroup parent = (ViewGroup) overlayView.getParent();
            if (parent != null) parent.removeView(overlayView);
            overlayView = null;
        }
    }
}
