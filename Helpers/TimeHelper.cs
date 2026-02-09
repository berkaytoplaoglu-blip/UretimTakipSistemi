using System;

namespace UretimTakipSistemi.Helpers
{
    public static class TimeHelper
    {
        /// <summary>
        /// Saniyeyi "X saat Y dakika" formatına çevirir
        /// </summary>
        public static string FormatSeconds(int seconds)
        {
            if (seconds == 0) return "0 dakika";

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            string result = "";

            if (hours > 0)
                result += hours + " saat ";

            if (minutes > 0)
                result += minutes + " dakika";

            if (hours == 0 && minutes == 0 && secs > 0)
                result += secs + " saniye";

            return result.Trim();
        }

        /// <summary>
        /// Saniyeyi "HH:MM:SS" formatına çevirir
        /// </summary>
        public static string FormatSecondsToTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, secs);
        }

        /// <summary>
        /// Saniyeyi ondalık saate çevirir (örn: 3661 saniye = 1.02 saat)
        /// </summary>
        public static decimal SecondsToHours(int seconds)
        {
            return Math.Round((decimal)seconds / 3600, 2);
        }

        /// <summary>
        /// İki tarih arasındaki farkı saat olarak döner
        /// </summary>
        public static decimal GetHoursDifference(DateTime start, DateTime end)
        {
            TimeSpan diff = end - start;
            return Math.Round((decimal)diff.TotalHours, 2);
        }

        /// <summary>
        /// Süre metni oluşturur (Raporlar için)
        /// Örnek: 7200 saniye = "2.00 saat (2 saat 0 dakika)"
        /// </summary>
        public static string FormatDuration(int seconds)
        {
            decimal hours = SecondsToHours(seconds);
            string readable = FormatSeconds(seconds);
            return $"{hours:N2} saat ({readable})";
        }

        /// <summary>
        /// Verimlilik hesaplar (adet/saat)
        /// </summary>
        public static decimal CalculateEfficiency(int totalProduction, int totalSeconds)
        {
            if (totalSeconds == 0) return 0;
            decimal hours = (decimal)totalSeconds / 3600;
            return Math.Round(totalProduction / hours, 2);
        }

        /// <summary>
        /// Fire oranı hesaplar
        /// </summary>
        public static decimal CalculateScrapRate(int scrapAmount, int totalProduction)
        {
            int total = scrapAmount + totalProduction;
            if (total == 0) return 0;
            return Math.Round((decimal)scrapAmount / total * 100, 2);
        }

        /// <summary>
        /// Net verimlilik hesaplar (net süre bazlı)
        /// </summary>
        public static decimal CalculateNetEfficiency(int production, int netSeconds, int totalSeconds)
        {
            if (netSeconds == 0 || totalSeconds == 0) return 0;

            decimal efficiency = CalculateEfficiency(production, netSeconds);
            decimal utilizationRate = (decimal)netSeconds / totalSeconds * 100;

            return Math.Round(efficiency, 2);
        }
    }
}