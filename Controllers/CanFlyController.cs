using java.sql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.AccessControl;


namespace CanFlyPipeline.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class CanFlyController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<CanFlyController> _logger;

        public CanFlyController(IConfiguration configuration, ILogger<CanFlyController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        //GET PILOT REPORT 
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetPilotReport")]
        public async Task<IActionResult> GetPilotReport()

        {
            string query = @"
            -- DROP PRIOR INSTANCE OF TEMPTABLE
                DROP TEMPORARY TABLE IF EXISTS TempReport;

            -- CREATE TEMPORARY TABLE
                CREATE TEMPORARY TABLE TempReport (
                    displayName VARCHAR(255),
                    ratingName VARCHAR(255),
                    ratingStatus VARCHAR(50),
                    ratingDate DATE,
                    medicalName VARCHAR(255),
                    medicalDate DATE,
                    totalTime DECIMAL(10, 2),
                    totalPIC DECIMAL(10, 2),
                    totalDual DECIMAL(10, 2),
                    timeOnType DECIMAL(10, 2),
                    typeName VARCHAR(255),
                    totalNight DECIMAL(10, 2),
                    nightNoInstrument DECIMAL(10, 2),
                    totalInstrument DECIMAL(10, 2),
                    totalCrossCountry DECIMAL(10, 2),
                    totalSim DECIMAL(10, 2),
                    totalInstrumentSim DECIMAL(10, 2),
                    totalVFRSim DECIMAL(10, 2),
                    totalLast30Days DECIMAL(10, 2),
                    totalLast90Days DECIMAL(10, 2),
                    totalLast6Months DECIMAL(10, 2),
                    totalLast12Months DECIMAL(10, 2),
                    totalLast24Months DECIMAL(10, 2),
                    totalLast60Months DECIMAL(10, 2),
                    approachesLast6Months DECIMAL(10, 2),
                    daysSincePIC DECIMAL(10, 2),
                    daysSinceIPC DECIMAL(10, 2),
                    daysSinceCurrencyUpgrade DECIMAL(10, 2)
                );

                -- IN-PROGRESS RATINGS FOR PILOT 2
                INSERT INTO TempReport (displayName, ratingName, ratingStatus)
                SELECT 'Rating' AS displayName,
                    rt.longName AS ratingName,
                    'InProgress' AS ratingStatus
                FROM pilot p
                LEFT JOIN rating r ON p.pilotID = r.pilotID 
                LEFT JOIN ratingType rt ON r.ratingTypeID = rt.ratingTypeID
                WHERE p.pilotID = 2 
                AND r.isWorkingTowards IS NOT NULL 
                AND r.dateAwarded IS NULL;

                -- COMPLETED RATINGS FOR PILOT 2
                INSERT INTO TempReport (displayName, ratingName, ratingStatus, ratingDate)
                SELECT 'Rating' AS displayName,
                    rt.longName AS ratingName, 
                    'Completed' AS ratingStatus, 
                    r.dateAwarded AS ratingDate 
                FROM pilot p 
                LEFT JOIN rating r ON p.pilotID = r.pilotID 
                LEFT JOIN ratingType rt ON r.ratingTypeID = rt.ratingTypeID
                WHERE p.pilotID = 2 
                AND r.isWorkingTowards IS NULL 
                AND r.dateAwarded IS NOT NULL;

                -- MEDICAL FOR PILOT 2
                INSERT INTO TempReport (displayName, medicalName, medicalDate)
                SELECT 'Medical' AS displayName,
                    mt.letterOrCertificate AS medicalName, 
                    m.expiry AS medicalDate 
                FROM medical m 
                LEFT JOIN medicalType mt ON mt.medicalTypeID = m.medicalTypeID
                WHERE m.pilotID = 2;

                -- TOTAL TIME FOR PILOT 2
                INSERT INTO TempReport (displayName, totalTime) 
                SELECT 'Total Hours' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalTime
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- PILOT IN COMMAND HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalPIC) 
                SELECT 'Total Pilot-in-Command Hours' AS displayName,
                    SUM(COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0)) AS totalPIC
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- DUAL HOURS TOTAL FOR PILOT 2
                INSERT INTO TempReport (displayName, totalDual) 
                SELECT 'Total Dual Hours' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalDual
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL TIME ON TYPE FOR PILOT 2 FOR CESSNA 172 (aircraftTypeID = 149)
                INSERT INTO TempReport (displayName, timeOnType, typeName) 
                SELECT 'Total Time on Type' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS timeOnType, 
                    'Cessna 172' AS typeName
                FROM logEntry
                WHERE pilotID = 2 AND aircraftTypeID = 149
                GROUP BY pilotID;

                -- TOTAL NIGHT HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalNight)
                SELECT 'Total Night Hours' as displayName,
                    SUM(COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0)) AS totalNight
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL NIGHT HOURS WITHOUT COUNTING INSTRUMENT HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, nightNoInstrument)
                SELECT 'Total Night Hours (excluding Instrument Hours)' AS displayName,
                    SUM(COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0)) AS nightNoInstrument
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL INSTRUMENT HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalInstrument)
                SELECT 'Total Instrument Hours' AS displayName,
                    SUM(COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalInstrument
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL CROSS COUNTRY HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalCrossCountry)
                SELECT 'Total Cross-Country Hours' AS displayName,
                    SUM(COALESCE(crossCountryDayDualTime, 0) +
                        COALESCE(crossCountryDayPICTime, 0) +
                        COALESCE(crossCountryNightDualTime, 0) +
                        COALESCE(crossCountryNightPICTime, 0)) AS totalCrossCountry
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL SIMULATOR HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalSim)
                SELECT 'Total Simulator Hours' AS displayName,
                    SUM(COALESCE(instrumentSimulatorDualTime, 0) +
                        COALESCE(VFRSimulatorDualTime, 0)) AS totalSim
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL INSTRUMENT SIMULATOR HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalInstrumentSim)
                SELECT 'Total Instrument Simulator Hours' AS displayName,
                    SUM(COALESCE(instrumentSimulatorDualTime, 0)) AS totalInstrumentSim
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL VFR SIMULATOR HOURS FOR PILOT 2
                INSERT INTO TempReport (displayName, totalVFRSim)
                SELECT 'Total VFR Simulator Hours' AS displayName,
                    SUM(COALESCE(VFRSimulatorDualTime, 0)) AS totalVFRSim
                FROM logEntry
                WHERE pilotID = 2
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 30 DAYS
                INSERT INTO TempReport (displayName, totalLast30Days)
                SELECT 'Total Hours (Last 30 Days)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast30Days
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 30 DAY AND NOW()
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 90 DAYS
                INSERT INTO TempReport (displayName, totalLast90Days)
                SELECT 'Total Hours (Last 90 Days)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast90Days
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 90 DAY AND NOW()
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 6 MONTHS
                INSERT INTO TempReport (displayName, totalLast6Months)
                SELECT 'Total Hours (Last 6 Months)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast6Months
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 6 MONTH AND NOW()
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 12 MONTHS
                INSERT INTO TempReport (displayName, totalLast12Months)
                SELECT 'Total Hours (Last 12 Months)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast12Months
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 12 MONTH AND NOW()
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 24 MONTHS
                INSERT INTO TempReport (displayName, totalLast24Months)
                SELECT 'Total Hours (Last 24 Months)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast24Months
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 24 MONTH AND NOW()
                GROUP BY pilotID;

                -- TOTAL HOURS FOR PILOT 2 LAST 60 MONTHS
                INSERT INTO TempReport (displayName, totalLast60Months)
                SELECT 'Total Hours (Last 60 Months)' AS displayName,
                    SUM(COALESCE(singleEngineDayDualTime, 0) +
                        COALESCE(singleEngineDayPICTime, 0) +
                        COALESCE(singleEngineNightDualTime, 0) +
                        COALESCE(singleEngineNightPICTime, 0) +
                        COALESCE(multiEngineDayDualTime, 0) +
                        COALESCE(multiEngineDayPICTime, 0) +
                        COALESCE(multiEngineDaySICTime, 0) +
                        COALESCE(multiEngineNightDualTime, 0) +
                        COALESCE(multiEngineNightPICTime, 0) +
                        COALESCE(multiEngineNightSICTime, 0) +
                        COALESCE(instrumentActualTime, 0) +
                        COALESCE(instrumentHoodTime, 0)) AS totalLast60Months
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 60 MONTH AND NOW()
                GROUP BY pilotID;

                -- TOTAL INSTRUMENT APPROACHES IN LAST 6 MONTHS FOR PILOT 2
                INSERT INTO TempReport (displayName, approachesLast6Months)
                SELECT 'Total Instrument Approaches (Last 6 Months)' AS displayName,
                    SUM(COALESCE(instrumentApproachesCount, 0)) AS approachesLast6Months
                FROM logEntry
                WHERE pilotID = 2 AND entryDate BETWEEN NOW() - INTERVAL 6 MONTH AND NOW()
                GROUP BY pilotID;

                -- DAYS SINCE LAST PILOT-IN-COMMAND TIME FOR PILOT 2
                INSERT INTO TempReport (displayName, daysSincePIC)
                SELECT 'Days Since Last Pilot-in-Command Time' AS displayName,
                    DATEDIFF(NOW(), MAX(entryDate)) AS daysSincePIC
                FROM logEntry
                WHERE pilotID = 2 AND (singleEngineDayPICTime > 0 OR singleEngineNightPICTime > 0)
                GROUP BY pilotID;

                -- DAYS SINCE LAST INSTRUMENT PROFICIENCY CHECK FOR PILOT 2
                INSERT INTO TempReport (displayName, daysSinceIPC)
                SELECT 'Days Since Last Instrument Proficiency Check' AS displayName,
                    DATEDIFF(NOW(), MAX(entryDate)) AS daysSinceIPC
                FROM logEntry
                WHERE pilotID = 2 AND instrumentApproachesCount > 0
                GROUP BY pilotID;

                -- DAYS SINCE LAST CURRENCY UPGRADE FOR PILOT 2
                INSERT INTO TempReport (displayName, daysSinceCurrencyUpgrade)
                SELECT 'Days Since Last Currency Upgrade' AS displayName,
                    DATEDIFF(NOW(), MAX(entryDate)) AS daysSinceCurrencyUpgrade
                FROM logEntry
                WHERE pilotID = 2 AND (singleEngineDayPICTime > 0 OR singleEngineNightPICTime > 0)
                GROUP BY pilotID;

                -- OUTPUT FINAL REPORT
                SELECT * FROM TempReport;";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        //command.CommandTimeout = 300; 
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var result = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    displayName = reader["displayName"] as string,
                                    ratingName = reader["ratingName"] as string,
                                    ratingStatus = reader["ratingStatus"] as string,
                                    ratingDate = reader["ratingDate"] as DateTime?,
                                    medicalName = reader["medicalName"] as string,
                                    medicalDate = reader["medicalDate"] as DateTime?,
                                    totalTime = reader["totalTime"] as decimal?,
                                    totalPIC = reader["totalPIC"] as decimal?,
                                    totalDual = reader["totalDual"] as decimal?,
                                    timeOnType = reader["timeOnType"] as decimal?,
                                    typeName = reader["typeName"] as string,
                                    totalNight = reader["totalNight"] as decimal?,
                                    nightNoInstrument = reader["nightNoInstrument"] as decimal?,
                                    totalInstrument = reader["totalInstrument"] as decimal?,
                                    totalCrossCountry = reader["totalCrossCountry"] as decimal?,
                                    totalSim = reader["totalSim"] as decimal?,
                                    totalInstrumentSim = reader["totalInstrumentSim"] as decimal?,
                                    totalVFRSim = reader["totalVFRSim"] as decimal?,
                                    totalLast30Days = reader["totalLast30Days"] as decimal?,
                                    totalLast90Days = reader["totalLast90Days"] as decimal?,
                                    totalLast6Months = reader["totalLast6Months"] as decimal?,
                                    totalLast12Months = reader["totalLast12Months"] as decimal?,
                                    totalLast24Months = reader["totalLast24Months"] as decimal?,
                                    totalLast60Months = reader["totalLast60Months"] as decimal?,
                                    approachesLast6Months = reader["approachesLast6Months"] as decimal?,
                                    daysSincePIC = reader["daysSincePIC"] as decimal?,
                                    daysSinceIPC = reader["daysSinceIPC"] as decimal?,
                                    daysSinceCurrencyUpgrade = reader["daysSinceCurrencyUpgrade"] as decimal?
                                };

                                result.Add(record);
                            }
                            return Ok(result); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the pilot report");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //GET REQUIREMENT SUMMARY (HARD CODED FOR PPL (RatingTypeID = 2) FOR DEMO STUDENT (PilotID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetRequirementSummary")]
        public async Task<IActionResult> GetRequirementSummary()

        {
            string query = @"
                -- Drop temporary tables
                
                DROP TEMPORARY TABLE IF EXISTS TempRequirements;
                DROP TEMPORARY TABLE IF EXISTS TempPilotLogAggregated;

                CREATE TEMPORARY TABLE TempRequirements (
                  requirementsID INT,
                  ratingTypeID INT,
                  dualSoloTotalCredit VARCHAR(50),
                  thisEntryHoursRequired DECIMAL(10, 2),
                  instrumentFlightRequired DECIMAL(10, 2),
                  crossCountryRequired DECIMAL(10, 2),
                  crossCountryStopsRequired INT,
                  crossCountryDistanceRequired DECIMAL(10, 2),
                  simulatorOptional DECIMAL(10, 2),
                  documentTypeID INT,
                  instrumentGroundOptional DECIMAL(10, 2), -- Refers to instrument dual simulator
                  parentRequirementsID INT,
                  displayName VARCHAR(100),
                  hierarchy VARCHAR(255)
                );

                -- Insert the requirements for ratingTypeID = 1 (Private Pilot's License)
                INSERT INTO TempRequirements
                SELECT requirementsID, ratingTypeID, dualSoloTotalCredit, thisEntryHoursRequired, instrumentFlightRequired, crossCountryRequired, crossCountryStopsRequired, crossCountryDistanceRequired, simulatorOptional, documentTypeID, instrumentGroundOptional, parentRequirementsID, displayName, hierarchy
                FROM requirements
                WHERE ratingTypeID = 1;

                -- Temporary table to store the pilot's aggregated log entries
                DROP TEMPORARY TABLE IF EXISTS TempPilotLogAggregated;

                CREATE TEMPORARY TABLE TempPilotLogAggregated (
                  totalHours DECIMAL(10, 2),
                  totalDual DECIMAL(10, 2),
                  totalInstrument DECIMAL(10, 2),
                  totalInstrumentSim DECIMAL(10, 2),
                  totalCrossCountry DECIMAL(10, 2),
                  totalDualCrossCountry DECIMAL(10, 2),
                  totalSoloCrossCountry DECIMAL(10, 2),
                  totalPIC DECIMAL(10, 2),
                  totalSimulator DECIMAL(10, 2), -- both instrument sim and VFR
                  soloCrossCountryTripStops INT,
                  soloCrossCountryDistance DECIMAL(10, 2),
                  crossCountryDate DATE
                );

                -- Aggregate the log entries for pilotID = 2
                INSERT INTO TempPilotLogAggregated
                SELECT
                  SUM(COALESCE(singleEngineDayDualTime, 0) +
                      COALESCE(singleEngineDayPICTime, 0) +
                      COALESCE(singleEngineNightDualTime, 0) +
                      COALESCE(singleEngineNightPICTime, 0) +
                      COALESCE(multiEngineDayDualTime, 0) +
                      COALESCE(multiEngineDayPICTime, 0) +
                      COALESCE(multiEngineDaySICTime, 0) +
                      COALESCE(multiEngineNightDualTime, 0) +
                      COALESCE(multiEngineNightPICTime, 0) +
                      COALESCE(multiEngineNightSICTime, 0) +
                      COALESCE(instrumentActualTime, 0) +
                      COALESCE(instrumentHoodTime, 0)) AS totalHours,
                  SUM(COALESCE(singleEngineDayDualTime, 0) +
                      COALESCE(singleEngineNightDualTime, 0) +
                      COALESCE(multiEngineDayDualTime, 0) +
                      COALESCE(multiEngineNightDualTime, 0)) AS totalDual,
                  SUM(COALESCE(instrumentActualTime, 0) +
                      COALESCE(instrumentHoodTime, 0)) AS totalInstrument,
                  SUM(COALESCE(instrumentSimulatorDualTime, 0)) AS totalInstrumentSim,
                  SUM(COALESCE(crossCountryDayDualTime, 0) +
                      COALESCE(crossCountryNightDualTime, 0) +
                      COALESCE(crossCountryDayPICTime, 0) +
                      COALESCE(crossCountryNightPICTime, 0)) AS totalCrossCountry,
                  SUM(COALESCE(crossCountryDayDualTime, 0) +
                      COALESCE(crossCountryNightDualTime, 0)) AS totalDualCrossCountry,
                  SUM(COALESCE(crossCountryDayPICTime, 0) +
                      COALESCE(crossCountryNightPICTime, 0)) AS totalSoloCrossCountry,
                  SUM(COALESCE(singleEngineDayPICTime, 0) +
                      COALESCE(singleEngineNightPICTime, 0) +
                      COALESCE(multiEngineDayPICTime, 0) +
                      COALESCE(multiEngineNightPICTime, 0)) AS totalPIC,
                  SUM(COALESCE(VFRsimulatorDualTime, 0) + 
                      COALESCE(instrumentSimulatorDualTime,0)) AS totalSimulator, -- both instrument sim and VFR
                  MAX(CASE WHEN routeTo IS NOT NULL AND routeVia IS NOT NULL AND routeFrom IS NOT NULL AND crossCountryDistance >= 150 AND crossCountryDayPICTime IS NOT NULL AND landings >= 3 THEN landings-1 ELSE 0 END) AS soloCrossCountryTripStops,
                  MAX(CASE WHEN routeTo IS NOT NULL AND routeVia IS NOT NULL AND routeFrom IS NOT NULL AND crossCountryDistance >= 150 AND crossCountryDayPICTime IS NOT NULL AND landings >= 3 THEN crossCountryDistance ELSE 0 END) AS soloCrossCountryDistance,
                  MAX(CASE WHEN routeTo IS NOT NULL AND routeVia IS NOT NULL AND routeFrom IS NOT NULL AND crossCountryDistance >= 150 AND crossCountryDayPICTime IS NOT NULL AND landings >= 3 THEN entryDate ELSE NULL END) AS crossCountryDate
                FROM logEntry
                WHERE pilotID = 2;

                -- Compare the requirements with the pilot's aggregated log entries and display the progress
                SELECT
                  r.requirementsID,
                  r.displayName,
                  CASE
                    WHEN r.displayName LIKE 'Total Hours%' THEN CONCAT(COALESCE(l.totalHours, 0), ' / ', r.thisEntryHoursRequired, ' hours completed')
                    WHEN r.displayName LIKE 'Total Dual%' THEN CONCAT(COALESCE(l.totalDual, 0), ' / ', r.thisEntryHoursRequired, ' hours completed')
                    WHEN r.displayName LIKE 'Dual Instrument%' THEN CONCAT(COALESCE(l.totalInstrument, 0), ' / ', r.instrumentFlightRequired, ' hours completed')
                    WHEN r.displayName LIKE 'Dual Cross%' THEN CONCAT(COALESCE(l.totalDualCrossCountry, 0), ' / ', r.crossCountryRequired, ' hours completed')
                    WHEN r.displayName LIKE 'Total Solo%' THEN CONCAT(COALESCE(l.totalPIC, 0), ' / ', r.thisEntryHoursRequired, ' hours completed')
                    WHEN r.displayName LIKE 'Solo Cross%' THEN CONCAT(COALESCE(l.totalSoloCrossCountry, 0), ' / ', r.crossCountryRequired, ' hours completed')
                    ELSE 'N/A'
                  END AS HoursStatus,
  
                  CASE
                    WHEN r.crossCountryStopsRequired IS NOT NULL AND r.crossCountryDistanceRequired IS NOT NULL THEN CONCAT('150NM with 2 stops completed on ', l.crossCountryDate)
                  END AS CrossCountryStatus,
  
                  CASE WHEN r.instrumentGroundOptional IS NOT NULL THEN CONCAT(COALESCE(l.totalInstrumentSim, 0), ' / ', r.instrumentGroundOptional)
                    ELSE NULL
                  END AS InstrumentSim,

                  CASE WHEN r.simulatorOptional IS NOT NULL THEN CONCAT(COALESCE(l.totalSimulator, 0), ' / ', r.simulatorOptional)
                    ELSE NULL
                  END AS TotalSim,

                  r.parentRequirementsID,
                  r.hierarchy
                FROM TempRequirements r
                LEFT JOIN TempPilotLogAggregated l ON 1 = 1; -- Cross join to compare each requirement with the aggregated log
                
                ";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var result = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    requirementsID = reader["requirementsID"] as decimal?,
                                    displayName = reader["displayName"] as string,
                                    HoursStatus = reader["HoursStatus"] as string,
                                    CrossCountryStatus = reader["CrossCountryStatus"] as string,
                                    InstrumentSim = reader["InstrumentSim"] as string,
                                    TotalSim = reader["TotalSim"] as string,
                                    parentRequirementsID = reader["parentRequirementsID"] as decimal?,
                                    hierarchy = reader["hierarchy"] as string
                                };

                                result.Add(record);
                            }
                            return Ok(result); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the requirements report");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        //RETRIEVING DATA FROM PILOT TABLE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetProfile")]
        public async Task<IActionResult> GetProfile()

        {
            string query = "select * from pilot WHERE pilotID=2";
            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var result = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    pilotID = reader["pilotID"] as decimal?,
                                    firstName = reader["firstName"] as string,
                                    lastName = reader["lastName"] as string,
                                    email = reader["email"] as string,
                                    streetAddress = reader["streetAddress"] as string,
                                    city = reader["city"] as string,
                                    province = reader["province"] as string,
                                    canadianForcesActive = reader["canadianForcesActive"] as decimal?,
                                    canadianForcesRetired = reader["canadianForcesRetired"] as decimal?,
                                    dob = reader["dob"] as DateTime?,
                                    phone = reader["phone"] as decimal?,
                                    primaryInstructor = reader["primaryInstructor"] as string
                                };

                                result.Add(record);
                            }
                            return Ok(result); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the pilot profile");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //RETRIEVING DATA FROM LOGENTRY TABLE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetLogs")]
        public async Task<IActionResult> GetLogs()

        {
            string query = "select * from logEntry";
            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var result = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    logEntryID = reader["logEntryID"] as decimal?,
                                    entryDate = reader["entryDate"] as DateTime?,
                                    registration = reader["registration"] as string,
                                    pilotInCommand = reader["pilotInCommand"] as string,
                                    studentOrCoPilot = reader["studentOrCoPilot"] as string,
                                    activityExercises = reader["activityExercises"] as string,
                                    singleEngineDayDualTime = reader["singleEngineDayDualTime"] as float?,
                                    singleEngineDayPICTime = reader["singleEngineDayPICTime"] as float?,
                                    singleEngineNightDualTime = reader["singleEngineNightDualTime"] as float?,
                                    singleEngineNightPICTime = reader["singleEngineNightPICTime"] as float?,
                                    multiEngineDayDualTime = reader["multiEngineDayDualTime"] as float?,
                                    multiEngineDayPICTime = reader["multiEngineDayPICTime"] as float?,
                                    multiEngineDaySICTime = reader["multiEngineDaySICTime"] as float?,
                                    multiEngineNightDualTime = reader["multiEngineNightDualTime"] as float?,
                                    multiEngineNightPICTime = reader["multiEngineNightPICTime"] as float?,
                                    multiEngineNightSICTime = reader["multiEngineNightSICTime"] as float?,
                                    instrumentActualTime = reader["instrumentActualTime"] as float?,
                                    instrumentHoodTime = reader["instrumentHoodTime"] as float?,
                                    instrumentSimulatorDualTime = reader["instrumentSimulatorDualTime"] as float?,
                                    instrumentApproachesCount = reader["instrumentApproachesCount"] as int?,
                                    crossCountryDayDualTime = reader["crossCountryDayDualTime"] as float?,
                                    crossCountryDayPICTime = reader["crossCountryDayPICTime"] as float?,
                                    crossCountryNightDualTime = reader["crossCountryNightDualTime"] as float?,
                                    crossCountryNightPICTime = reader["crossCountryNightPICTime"] as float?,
                                    crossCountryDistance = reader["crossCountryDistance"] as float?,
                                    routeFrom = reader["routeFrom"] as string,
                                    routeVia = reader["routeVia"] as string,
                                    routeTo = reader["routeTo"] as string,
                                    dualInstructionGivenTime = reader["dualInstructionGivenTime"] as string,
                                    floatTime = reader["floatTime"] as string,
                                    VFRSimulatorDualTime = reader["VFRSimulatorDualTime"] as string,
                                    pilotID = reader["pilotID"] as int?,
                                    CAF = reader["CAF"] as bool?,
                                    takeOffs = reader["takeOffs"] as int?,
                                    landings = reader["landings"] as int?,
                                    circuits = reader["circuits"] as int?,
                                    omitFromReports = reader["omitFromReports"] as bool?,
                                    untetheredBalloon = reader["untetheredBalloon"] as int?,
                                    altitudeBalloon = reader["altitudeBalloon"] as int?,
                                    outsideCanada = reader["outsideCanada"] as bool?,
                                    launchLocationGlider = reader["launchLocationGlider"] as string,
                                    distanceGlider = reader["distanceGlider"] as int?,
                                    launchTypeGlider = reader["launchTypeGlider"] as string,
                                    aircraftCategory = reader["aircraftCategory"] as string,
                                    aircraftTypeID = reader["aircraftTypeID"] as int?
                                };

                                result.Add(record);
                            }
                            return Ok(result); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the pilot logbook");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // DELETING ROWS FROM LOGENTRY TABLE
        [HttpDelete("{id}")]
        public JsonResult DeleteNotes(int id)
        {
            string query = "DELETE FROM logEntry WHERE logEntryID=@logEntryID";
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");

            using (MySqlConnection myCon = new MySqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (MySqlCommand myCommand = new MySqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@logEntryID", id);
                    int rowsAffected = myCommand.ExecuteNonQuery(); // Use ExecuteNonQuery for delete
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Deleted Successfully");
                    }
                    else
                    {
                        return new JsonResult("No record found");
                    }
                }
            }
        }


        // ADDING A NEW LOG ENTRY
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("AddNotes")]
        public async Task<IActionResult> AddNotes([FromBody] LogEntryModel logentrymodel)
        {
            string query = @"INSERT INTO logEntry (
                entryDate, registration, pilotInCommand, studentOrCoPilot, activityExercises, 
                singleEngineDayDualTime, singleEngineDayPICTime, singleEngineNightDualTime, singleEngineNightPICTime, 
                multiEngineDayDualTime, multiEngineDayPICTime, multiEngineDaySICTime, multiEngineNightDualTime, multiEngineNightPICTime, multiEngineNightSICTime, 
                instrumentActualTime, instrumentHoodTime, instrumentSimulatorDualTime, instrumentApproachesCount, 
                crossCountryDayDualTime, crossCountryDayPICTime, crossCountryNightDualTime, crossCountryNightPICTime, crossCountryDistance,
                routeFrom, routeVia, routeTo, 
                dualInstructionGivenTime, floatTime, VFRSimulatorDualTime, CAF, 
                takeOffs, landings, circuits, omitFromReports, 
                untetheredBalloon, altitudeBalloon, outsideCanada,
                launchLocationGlider, distanceGlider, launchTypeGlider, 
                aircraftCategory, aircraftTypeID, pilotID) 
            VALUES (
                @entryDate, @Registration, @PilotInCommand, @StudentOrCoPilot, @ActivityExercises, 
                @SingleEngineDayDualTime, @SingleEngineDayPICTime, @SingleEngineNightDualTime, @SingleEngineNightPICTime, 
                @MultiEngineDayDualTime, @MultiEngineDayPICTime, @MultiEngineDaySICTime, @MultiEngineNightDualTime, @MultiEngineNightPICTime, @MultiEngineNightSICTime, 
                @InstrumentActualTime, @InstrumentHoodTime, @InstrumentSimulatorDualTime, @InstrumentApproachesCount, 
                @CrossCountryDayDualTime, @CrossCountryDayPICTime, @CrossCountryNightDualTime, @CrossCountryNightPICTime, @CrossCountryDistance, 
                @RouteFrom, @RouteVia, @RouteTo, 
                @DualInstructionGivenTime, @FloatTime, @VFRSimulatorDualTime, 
                @CAF, 
                @TakeOffs, @Landings, @Circuits, @OmitFromReports, 
                @UntetheredBalloon, @AltitudeBalloon, @OutsideCanada,
                @LaunchLocationGlider, @DistanceGlider, @LaunchTypeGlider, 
                @AircraftCategory, @AircraftTypeID, 2)";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@entryDate", logentrymodel.entryDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Registration", logentrymodel.registration ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PilotInCommand", logentrymodel.pilotInCommand ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StudentOrCoPilot", logentrymodel.studentOrCoPilot ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ActivityExercises", logentrymodel.activityExercises ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SingleEngineDayDualTime", logentrymodel.singleEngineDayDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SingleEngineDayPICTime", logentrymodel.singleEngineDayPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SingleEngineNightDualTime", logentrymodel.singleEngineNightDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SingleEngineNightPICTime", logentrymodel.singleEngineNightPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineDayDualTime", logentrymodel.multiEngineDayDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineDayPICTime", logentrymodel.multiEngineDayPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineDaySICTime", logentrymodel.multiEngineDaySICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineNightDualTime", logentrymodel.multiEngineNightDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineNightPICTime", logentrymodel.multiEngineNightPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MultiEngineNightSICTime", logentrymodel.multiEngineNightSICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@InstrumentActualTime", logentrymodel.instrumentActualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@InstrumentHoodTime", logentrymodel.instrumentHoodTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@InstrumentSimulatorDualTime", logentrymodel.instrumentSimulatorDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@InstrumentApproachesCount", logentrymodel.instrumentApproachesCount ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossCountryDayDualTime", logentrymodel.crossCountryDayDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossCountryDayPICTime", logentrymodel.crossCountryDayPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossCountryNightDualTime", logentrymodel.crossCountryNightDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossCountryNightPICTime", logentrymodel.crossCountryNightPICTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossCountryDistance", logentrymodel.crossCountryDistance ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@RouteFrom", logentrymodel.routeFrom ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@RouteVia", logentrymodel.routeVia ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@RouteTo", logentrymodel.routeTo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DualInstructionGivenTime", logentrymodel.dualInstructionGivenTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@FloatTime", logentrymodel.floatTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@VFRSimulatorDualTime", logentrymodel.VFRSimulatorDualTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CAF", logentrymodel.CAF ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TakeOffs", logentrymodel.takeOffs ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Landings", logentrymodel.landings ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Circuits", logentrymodel.circuits ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@OmitFromReports", logentrymodel.omitFromReports ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UntetheredBalloon", logentrymodel.untetheredBalloon ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AltitudeBalloon", logentrymodel.altitudeBalloon ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@OutsideCanada", logentrymodel.outsideCanada ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@LaunchLocationGlider", logentrymodel.launchLocationGlider ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DistanceGlider", logentrymodel.distanceGlider ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@LaunchTypeGlider", logentrymodel.launchTypeGlider ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AircraftCategory", logentrymodel.aircraftCategory ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AircraftTypeID", logentrymodel.aircraftTypeID ?? (object)DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                return Ok(new { message = "Added Successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the log entry");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET RATINGS THAT ARE IN PROGRESS FOR DEMO PILOT (PILOTID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetInProgressRatings")]
        public async Task<IActionResult> GetInProgressRatings(int pilotId)
        {
            string query = @"
    SELECT r.longName, r.shortName
    FROM rating AS ra 
    JOIN ratingType AS r ON ra.ratingTypeID = r.ratingTypeID 
    WHERE ra.pilotID = @PilotId AND ra.isWorkingTowards > 0;";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PilotId", pilotId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var ratings = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    LongName = reader["longName"] as string,
                                    ShortName = reader["shortName"] as string
                                };

                                ratings.Add(record);
                            }
                            return Ok(ratings); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the in-progress ratings");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // RETRIEVE COMPLETED RATINGS FOR DEMO PILOT (PILOTID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetCompletedRatings")]
        public async Task<IActionResult> GetCompletedRatings()
        {
            string query = @"
    SELECT r.longName, r.shortName, ra.dateAwarded 
    FROM rating AS ra 
    JOIN ratingType AS r ON ra.ratingTypeID = r.ratingTypeID 
    WHERE ra.pilotID = 2 AND ra.isWorkingTowards IS NULL;";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var ratings = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    LongName = reader["longName"] as string,
                                    ShortName = reader["shortName"] as string,
                                    DateAwarded = reader["dateAwarded"] as DateTime?
                                };

                                ratings.Add(record);
                            }
                            return Ok(ratings); // This returns JSON
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the completed ratings");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // RETRIEVING TOTAL SUM OF SINGLE ENGINE HOURS FOR LANDING PAGE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetTotalHours")]
        public async Task<IActionResult> GetTotalHours()
        {
            string query = @"
       SELECT 
        ROUND(
            SUM(
                COALESCE(singleEngineDayDualTime, 0) +
                COALESCE(singleEngineDayPICTime, 0) +
                COALESCE(singleEngineNightDualTime, 0) +
                COALESCE(singleEngineNightPICTime, 0) +
                COALESCE(multiEngineDayDualTime, 0) +
                COALESCE(multiEngineDayPICTime, 0) +
                COALESCE(multiEngineDaySICTime, 0) +
                COALESCE(multiEngineNightDualTime, 0) +
                COALESCE(multiEngineNightPICTime, 0) +
                COALESCE(multiEngineNightSICTime, 0) +
                COALESCE(instrumentActualTime, 0) +
                COALESCE(instrumentHoodTime, 0)
            ), 
            1
            ) AS TotalHours
        FROM logEntry
        WHERE pilotID = 2
     GROUP BY pilotID;";

            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Correctly read the TotalHours from the reader
                                var totalHours = reader["TotalHours"] as float? ?? 0f;
                                return Ok(new { TotalHours = totalHours }); // This returns JSON
                            }
                            return NotFound(); // No data found for the given pilotID
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the total hours");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //RETRIEVING PILOT NAME FOR LANDING PAGE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetName")]
        //public JsonResult GetName()
        public async Task<IActionResult> GetName()
        {
            string query = @"SELECT CONCAT(firstName, ' ', lastName) AS FullName FROM pilot WHERE pilotID = 2;";
            try
            {
                using (var connection = new MySqlConnection(_configuration.GetConnectionString("CanFlyDBConn")))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var fullName = new
                                {
                                    FullName = reader["fullName"] as string
                                };
                                return Ok(fullName); // This returns JSON
                            }
                            return NotFound(); // No data found
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the pilot name");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

