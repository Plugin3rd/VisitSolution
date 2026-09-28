using System.Globalization;

namespace VisitRegistry.Services;

public class PersianDateService
{
    private readonly PersianCalendar _calendar = new();

    public string ToPersian(DateTime date)
    {
        return $"{_calendar.GetYear(date):0000}/{_calendar.GetMonth(date):00}/{_calendar.GetDayOfMonth(date):00}";
    }

    public string ToPersian(DateTime? date) => date.HasValue ? ToPersian(date.Value) : "";

    public bool TryParse(string? value, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = value.Trim()
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
            .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
            .Replace('-', '/');

        var parts = normalized.Split('/');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var y) || !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var d))
            return false;

        try
        {
            date = _calendar.ToDateTime(y, m, d, 0, 0, 0, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
