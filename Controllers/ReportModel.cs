using System;

namespace WebApplication1.Controllers
{
    public class ReportModel
    {
        public string? displayName { get; set; }
        public string? ratingName { get; set; }
        public DateTime? ratingDate { get; set; }
        public string? medicalName { get; set; }
        public string? medicalDate { get; set; }
        public float? totalTime { get; set; }
        public float? totalPIC { get; set; }
        public float? totalDual { get; set; }
        public float? timeOnType { get; set; }
        public string? typeName { get; set; }
        public float? totalNight { get; set; }
        public float? nightNoInstrument { get; set; }
        public float? totalCrossCountry { get; set; }
        public float? totalSim { get; set; }
        public float? totalInstrumentSim { get; set; }
        public float? totalVFRSim { get; set; }
        public float? totalLast30Days { get; set; }
        public float? totalLast90Days { get; set; }
        public float? totalLast6Months { get; set; }
        public float? totalLast12Months { get; set; }
        public float? totalLast24Months { get; set; }
        public float? totalLast60Months { get; set; }
        public float? approachesLast6Months { get; set; }
        public float? daysSincePIC { get; set; }
        public float? daysSinceIPC { get; set; }
        public float? daysSinceCurrencyUpgrade { get; set; }

    }

}