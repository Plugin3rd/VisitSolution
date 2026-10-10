using System.Globalization;

namespace VisitRegistry.Services;

/// <summary>
/// Localized helpers for rendering and parsing Jalali (Solar Hijri) dates,
/// times and numbers in the Persian UI.
/// </summary>
public class PersianDateService
{
    private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";
    private const string ArabicDigits = "٠١٢٣٤٥٦٧٨٩";
    private const string LatinDigits = "0123456789";

    private static readonly string[] MonthNames =
    [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    // Indexed by (int) PersianCalendar.GetDayOfWeek: Sunday=0 … Saturday=6
    private static readonly string[] WeekDayNames =
    [
        "یک‌شنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه"
    ];

    private readonly PersianCalendar _calendar = new();

    /// <summary>The current Jalali date, used as the default for pickers.</summary>
    public DateTime Today => _calendar.ToDateTime(
        _calendar.GetYear(DateTime.Now),
        _calendar.GetMonth(DateTime.Now),
        _calendar.GetDayOfMonth(DateTime.Now),
        0, 0, 0, 0);

    /// <summary>Converts ASCII digits in <paramref name="value"/> to Persian digits.</summary>
    public string ToPersianDigits(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var index = LatinDigits.IndexOf(chars[i]);
            if (index >= 0) chars[i] = PersianDigits[index];
        }

        return new string(chars);
    }

    /// <summary>Formats a number with Persian digits and thousands separators.</summary>
    public string ToPersianNumber(long value) => ToPersianDigits(value.ToString("N0", CultureInfo.InvariantCulture));

    /// <summary>Formats a number with Persian digits and no separators.</summary>
    public string ToPersianNumber(int value) => ToPersianDigits(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Replaces Latin digits with the matching Latin equivalents for parsing.</summary>
    public string NormalizeDigits(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var p = PersianDigits.IndexOf(chars[i]);
            if (p >= 0)
            {
                chars[i] = LatinDigits[p];
                continue;
            }

            var a = ArabicDigits.IndexOf(chars[i]);
            if (a >= 0) chars[i] = LatinDigits[a];
        }

        return new string(chars);
    }

    /// <summary>Numeric Jalali date, e.g. <c>1404/07/06</c>.</summary>
    public string ToPersian(DateTime date)
    {
        return $"{_calendar.GetYear(date):0000}/{_calendar.GetMonth(date):00}/{_calendar.GetDayOfMonth(date):00}";
    }

    public string ToPersian(DateTime? date) => date.HasValue ? ToPersian(date.Value) : string.Empty;

    /// <summary>Jalali date with Persian digits, e.g. <c>۱۴۰۴/۰۷/۰۶</c>.</summary>
    public string ToPersianDigitsDate(DateTime date) => ToPersianDigits(ToPersian(date));

    public string ToPersianDigitsDate(DateTime? date) => date.HasValue ? ToPersianDigitsDate(date.Value) : string.Empty;

    /// <summary>Long Jalali date, e.g. <c>شنبه ۶ مهر ۱۴۰۴</c>.</summary>
    public string ToPersianLong(DateTime date)
    {
        var month = MonthNames[_calendar.GetMonth(date) - 1];
        var day = _calendar.GetDayOfMonth(date);
        var year = _calendar.GetYear(date);
        return $"{WeekDayName(date)} {ToPersianDigits(day.ToString(CultureInfo.InvariantCulture))} {month} {ToPersianDigits(year.ToString(CultureInfo.InvariantCulture))}";
    }

    public string ToPersianLong(DateTime? date) => date.HasValue ? ToPersianLong(date.Value) : string.Empty;

    /// <summary>Full Jalali timestamp, e.g. <c>شنبه ۶ مهر ۱۴۰۴ - ۱۴:۳۰</c>.</summary>
    public string ToPersianDateTime(DateTime date) => $"{ToPersianLong(date)} - {ToPersianTime(date.TimeOfDay)}";

    public string ToPersianDateTime(DateTime? date) => date.HasValue ? ToPersianDateTime(date.Value) : string.Empty;

    /// <summary>Persian name of the Jalali week day, e.g. <c>یک‌شنبه</c>.</summary>
    public string WeekDayName(DateTime date) => WeekDayNames[(int)_calendar.GetDayOfWeek(date)];

    public string WeekDayName(DateTime? date) => date.HasValue ? WeekDayName(date.Value) : string.Empty;

    /// <summary>Jalali month name for the given month number (1-12).</summary>
    public string MonthName(int month) => month is >= 1 and <= 12 ? MonthNames[month - 1] : string.Empty;

    /// <summary>24-hour time with Persian digits, e.g. <c>۱۴:۳۰</c>.</summary>
    public string ToPersianTime(TimeSpan time) => ToPersianDigits(FormatTime(time));

    public string ToPersianTime(TimeSpan? time) => time.HasValue ? ToPersianTime(time.Value) : string.Empty;

    /// <summary>Time in Latin digits, suitable for an HTML value attribute.</summary>
    public static string FormatTime(TimeSpan time) => $"{(int)time.TotalHours:00}:{time.Minutes:00}";

    public static string FormatTime(TimeSpan? time) => time.HasValue ? FormatTime(time.Value) : string.Empty;

    /// <summary>Parses a Jalali date, accepting Latin, Persian and Arabic digits.</summary>
    public bool TryParse(string? value, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = NormalizeDigits(value.Trim()).Replace('-', '/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var d))
            return false;

        if (m is < 1 or > 12 || d < 1) return false;
        if (d > _calendar.GetDaysInMonth(y, m)) return false;

        try
        {
            date = _calendar.ToDateTime(y, m, d, 0, 0, 0, 0);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>Parses a 24-hour <c>HH:mm</c> time, accepting Persian digits.</summary>
    public static bool TryParseTime(string? value, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = NormalizeTimeDigits(value.Trim());
        var parts = normalized.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return false;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var h)) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m)) return false;

        var seconds = 0;
        if (parts.Length > 3 && !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds)) return false;

        if (h is < 0 or > 23 || m is < 0 or > 59 || seconds is < 0 or > 59) return false;

        time = new TimeSpan(h, m, seconds);
        return true;
    }

    private static string NormalizeTimeDigits(string value)
    {
        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var p = PersianDigits.IndexOf(chars[i]);
            if (p >= 0)
            {
                chars[i] = LatinDigits[p];
                continue;
            }

            var a = ArabicDigits.IndexOf(chars[i]);
            if (a >= 0) chars[i] = LatinDigits[a];
        }

        return new string(chars);
    }
}
