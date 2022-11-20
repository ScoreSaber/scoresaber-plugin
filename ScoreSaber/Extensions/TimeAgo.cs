#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace ScoreSaber.Extensions {
    internal static class TimeAgo {
        public static TimeSpan Days(this int number) => TimeSpan.FromDays(number);
        public static TimeSpan Hours(this int number) => TimeSpan.FromHours(number);
        public static TimeSpan Minutes(this int number) => TimeSpan.FromMinutes(number);
        public static TimeSpan Seconds(this int number) => TimeSpan.FromSeconds(number);
        public static TimeSpan Milliseconds(this int number) => TimeSpan.FromMilliseconds(number);
        public static decimal Round(this decimal @this, int digits) =>
            Math.Round(@this, digits, MidpointRounding.AwayFromZero);

        public static double Round(this double @this, int digits) {

            if (double.IsNaN(@this)) return double.NaN;
            return (double)((decimal)@this).Round(digits);
        }

        static bool TryReduceDays(ref TimeSpan period, int len, out double result) {

            if (period.TotalDays >= len) {
                result = (int)Math.Floor(period.TotalDays / len);
                period -= TimeSpan.FromDays(len * result);

                return true;
            }

            result = 0;
            return false;
        }

        public static string ToNaturalTime(this TimeSpan @this, int precisionParts, bool longForm) {

            var names = new Dictionary<string, string> { { "year", "y" }, { "month", "M" }, { "week", "w" }, { "day", "d" }, { "hour", "h" }, { "minute", "m" }, { "second", "s" }, { " and ", " " }, { ", ", " " } };

            string name(string k) => longForm ? k : names[k];

            var parts = new Dictionary<string, double>();

            const int YEAR = 365, MONTH = 30, WEEK = 7;

            if (TryReduceDays(ref @this, YEAR, out var years))
                parts.Add(name("year"), years);

            if (TryReduceDays(ref @this, MONTH, out var months))
                parts.Add(name("month"), months);

            if (TryReduceDays(ref @this, WEEK, out var weeks))
                parts.Add(name("week"), weeks);

            if (@this.TotalDays >= 1) {
                parts.Add(name("day"), @this.Days);
                @this -= @this.Days.Days();
            }
            if (@this.TotalHours >= 1 && @this.Hours > 0) {
                parts.Add(name("hour"), @this.Hours);
                @this = @this.Subtract(@this.Hours.Hours());
            }

            if (@this.TotalMinutes >= 1 && @this.Minutes > 0) {
                parts.Add(name("minute"), @this.Minutes);
                @this = @this.Subtract(@this.Minutes.Minutes());
            }

            if (@this.TotalSeconds >= 1 && @this.Seconds > 0) {
                parts.Add(name("second"), @this.Seconds);
                @this = @this.Subtract(@this.Seconds.Seconds());
            } else if (@this.TotalSeconds > 0) {
                parts.Add(name("second"), @this.TotalSeconds.Round(3));
                @this = TimeSpan.Zero;
            }

            var outputParts = parts.Take(precisionParts).ToList();
            var r = new StringBuilder();

            foreach (var part in outputParts) {
                r.Append(part.Value);

                if (longForm) r.Append(" ");

                r.Append(part.Key);

                if (part.Value > 1 && longForm) r.Append("s");

                if (outputParts.IndexOf(part) == outputParts.Count - 2)
                    r.Append(name(" and "));
                else if (outputParts.IndexOf(part) < outputParts.Count - 2)
                    r.Append(name(", "));
            }

            return r.ToString();
        }
    }
}