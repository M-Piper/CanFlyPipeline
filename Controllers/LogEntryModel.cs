namespace CanFlyPipeline.Controllers
{
    public class LogEntryModel
    {
        public int logEntryID { get; set; }
        public DateTime? entryDate { get; set; }
        public string? registration { get; set; }
        public string? pilotInCommand { get; set; }
        public string? studentOrCoPilot { get; set; }
        public string? activityExercises { get; set; }
        public float? singleEngineDayDualTime { get; set; }
        public float? singleEngineDayPICTime { get; set; }
        public float? singleEngineNightDualTime { get; set; }
        public float? singleEngineNightPICTime { get; set; }
        public float? multiEngineDayDualTime { get; set; }
        public float? multiEngineDayPICTime { get; set; }
        public float? multiEngineDaySICTime { get; set; }
        public float? multiEngineNightDualTime { get; set; }
        public float? multiEngineNightPICTime { get; set; }
        public float? multiEngineNightSICTime { get; set; }
        public float? instrumentActualTime { get; set; }
        public float? instrumentHoodTime { get; set; }
        public float? instrumentSimulatorDualTime { get; set; }
        public int? instrumentApproachesCount { get; set; }
        public float? crossCountryDayDualTime { get; set; }
        public float? crossCountryDayPICTime { get; set; }
        public float? crossCountryNightDualTime { get; set; }
        public float? crossCountryNightPICTime { get; set; }
        public float? crossCountryDistance { get; set; }
        public string? routeFrom { get; set; }
        public string? routeVia { get; set; }
        public string? routeTo { get; set; }
        public string? dualInstructionGivenTime { get; set; }
        public string? floatTime { get; set; }
        public string? VFRSimulatorDualTime { get; set; }
        public int? pilotID { get; set; }
        public bool? CAF { get; set; }
        public int? takeOffs { get; set; }
        public int? landings { get; set; }
        public int? circuits { get; set; }
        public bool? omitFromReports { get; set; }
        public int? untetheredBalloon { get; set; }
        public int? altitudeBalloon { get; set; }
        public bool? outsideCanada { get; set; }
        public string? launchLocationGlider { get; set; }
        public int? distanceGlider { get; set; }
        public string? launchTypeGlider { get; set; }
        public string? aircraftCategory { get; set; }
        public int? aircraftTypeID { get; set; }

    }
}
