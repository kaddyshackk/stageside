namespace ComedyPull.Application.Utils
{
    public static class ParseUtils
    {
        /// <summary>
        /// Attempts to parse separate date, time, and year information into a single <see cref="DateTimeOffset"/>
        /// object. Uses the local timezone unless specified elsewhere before conversion to UTC.
        /// </summary>
        /// <param name="dateText">String containing date information.</param>
        /// <param name="timeText">String containing time information.</param>
        /// <param name="year">Year.</param>
        /// <param name="timezone">Timezone override.</param>
        /// <returns>Parsed <see cref="DateTimeOffset"/> object converted to UTC.</returns>
        public static DateTimeOffset? ParseDateTime(string dateText, string timeText, int year, TimeZoneInfo? timezone = null)
        {
            var cleanDate = dateText.Trim();
            var cleanTime = timeText.Trim();
            var combinedString = $"{cleanDate} {year} {cleanTime}";
            if (!DateTime.TryParse(combinedString, out var parsedDateTime)) return null;
            var tz = timezone ?? TimeZoneInfo.Local;
            var localDateTimeOffset = new DateTimeOffset(parsedDateTime, tz.GetUtcOffset(parsedDateTime));
            return localDateTimeOffset.ToUniversalTime();
        }

        /// <summary>
        /// Attempts to parse separate date, time, and year information into a single <see cref="DateTimeOffset"/>
        /// object. Uses the local timezone unless specified elsewhere before conversion to UTC. Automatically converts
        /// the date to a future time (Assumes no provided dates will ever be in the past).
        /// </summary>
        /// <param name="dateText">String containing date information.</param>
        /// <param name="timeText">String containing time information.</param>
        /// <param name="timezone">Timezone override.</param>
        /// <returns>Parsed <see cref="DateTimeOffset"/> object converted to UTC.</returns>
        public static DateTimeOffset? ParseFutureEventDateTime(string dateText, string timeText, TimeZoneInfo? timezone = null)
        {
            var currentDate = DateTime.Now;
            var currentYear = currentDate.Year;
            var result = ParseDateTime(dateText, timeText, currentYear, timezone);
            if (!result.HasValue || result.Value.Date >= currentDate.Date) return result;
            result = ParseDateTime(dateText, timeText, currentYear + 1);
            return result;
        }
    }
}