package com.blackdoorsdev.kovsieassetstracker.helpers;

import androidx.room.TypeConverter;

import java.util.Date;

public class DateConverter {
    @TypeConverter
    public static Long fromDate(Date date) {
        return date != null ? date.getTime() : null;
    }
    @TypeConverter
    public static Date toDate(Long timestamp) {
        return timestamp != null ? new Date(timestamp) : null;
    }
}
