using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DateTimeExtensions
{
    public static string ToTimeAgoFormat(this DateTime targetDate, DateTime compareDate)
    {
        if (targetDate > compareDate)
        {
            throw new ArgumentOutOfRangeException(nameof(targetDate), "Thời gian truyền vào không được nằm ở tương lai.");
        }
        TimeSpan difference = compareDate - targetDate;

        double seconds = Math.Max(0, difference.TotalSeconds);

        if (seconds < 60)
        {
            return $"{(int)seconds} giây trước";
        }

        if (difference.TotalMinutes < 60)
        {
            return $"{(int)difference.TotalMinutes} phút trước";
        }

        if (difference.TotalHours < 24)
        {
            return $"{(int)difference.TotalHours} giờ trước";
        }

        return targetDate.ToString("dd/MM/yyyy HH:mm");
    }

}

