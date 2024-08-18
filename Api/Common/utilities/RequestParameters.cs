namespace Api.Common.utilities
{
    public static class RequestParameters
    {
        public static (int year, int month, string healthUnit) GetYearMonthHealthUnit(HttpContext context)
        {
            var yearMonthPeriod = context.Request.Query["period"].FirstOrDefault();
            var healthUnit = context.Request.Query["healthUnit"].FirstOrDefault();

            if (string.IsNullOrEmpty(yearMonthPeriod) || yearMonthPeriod.Length < 6)
            {
                throw new ArgumentException("Invalid period format. Expected format: YYYYMM.");

            }

            if (string.IsNullOrEmpty(healthUnit))
            {
                throw new ArgumentException("Health unit parameter is missing.");
            }

            var year = int.Parse(yearMonthPeriod.Substring(0, 4));
            var month = int.Parse(yearMonthPeriod.Substring(4, 2));

            return (year, month, healthUnit);
        }
    }
}
