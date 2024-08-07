﻿using java.sql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.AccessControl;


namespace CanFlyPipeline.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class CanFlyController : ControllerBase
    {

        private IConfiguration _configuration;

        public CanFlyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        //GET PILOT REPORT 
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetPilotReport")]
        public JsonResult GetPilotreport()

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
                    SUM(COALESCE(instrumentApproachTime, 0)) AS approachesLast6Months
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
                WHERE pilotID = 2 AND instrumentApproachTime > 0
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

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }


        //GET REQUIREMENT SUMMARY (HARD CODED FOR PPL (RatingTypeID = 2) FOR DEMO STUDENT (PilotID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetRequirementSummary")]
        public JsonResult GetRequirementSummary()

        {
            string query = @"
                -- Temporary table to store the requirements
                DROP TEMPORARY TABLE IF EXISTS TempRequirements;

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
                  MAX(CASE WHEN routeTo IS NOT NULL AND routeVia IS NOT NULL AND routeFrom IS NOT NULL AND crossCountryDistance >= 150 AND crossCountryDayPICTime IS NOT NULL AND landings >= 3 THEN date ELSE NULL END) AS crossCountryDate
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

                -- Drop temporary tables
                DROP TEMPORARY TABLE IF EXISTS TempRequirements;
                DROP TEMPORARY TABLE IF EXISTS TempPilotLogAggregated;";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }



        //RETRIEVING DATA FROM PILOT TABLE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetProfile")]
        public JsonResult GetProfile()

        {
            string query = "select * from pilot WHERE pilotID=2";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }

        //RETRIEVING DATA FROM LOGENTRY TABLE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetLogs")]
        public JsonResult GetLogs()

        {
            string query = "select * from logEntry";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }


        //DELETING ROWS FROM LOGENTRY TABLE
        [HttpDelete]
        [Microsoft.AspNetCore.Mvc.Route("DeleteLogs")]
        public JsonResult DeleteNotes(string id)
        {
            string query = "delete from logEntry where logEntryID=@logEntryID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {

                    myCommand.Parameters.AddWithValue("@logEntryID", id);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();

                }
            }
            return new JsonResult("Deleted Successfully");
        }



        [HttpPost]

        [Microsoft.AspNetCore.Mvc.Route("AddNotes")]

        public JsonResult AddNotes([FromBody] LogEntryModel logentrymodel)

        {

            string query = @"INSERT INTO logEntry (
                date, registration, pilotInCommand, studentOrCoPilot, activityExercises, 
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
                @Date, @Registration, @PilotInCommand, @StudentOrCoPilot, @ActivityExercises, 
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


            DataTable table = new DataTable();

            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");

            SqlDataReader myReader;

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))

            {

                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand(query, myCon))

                {
                    myCommand.Parameters.AddWithValue("@date", logentrymodel.date).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@registration", logentrymodel.registration).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@pilotInCommand", logentrymodel.pilotInCommand).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@studentOrCoPilot", logentrymodel.studentOrCoPilot).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@activityExercises", logentrymodel.activityExercises).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@singleEngineDayDualTime", logentrymodel.singleEngineDayDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@singleEngineDayPICTime", logentrymodel.singleEngineDayPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@singleEngineNightDualTime", logentrymodel.singleEngineNightDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@singleEngineNightPICTime", logentrymodel.singleEngineNightPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineDayDualTime", logentrymodel.multiEngineDayDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineDayPICTime", logentrymodel.multiEngineDayPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineDaySICTime", logentrymodel.multiEngineDaySICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineNightDualTime", logentrymodel.multiEngineNightDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineNightPICTime", logentrymodel.multiEngineNightPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@multiEngineNightSICTime", logentrymodel.multiEngineNightSICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentActualTime", logentrymodel.instrumentActualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentHoodTime", logentrymodel.instrumentHoodTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentSimulatorDualTime", logentrymodel.instrumentSimulatorDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentApproachesCount", logentrymodel.instrumentApproachesCount).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryDayDualTime", logentrymodel.crossCountryDayDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryDayPICTime", logentrymodel.crossCountryDayPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryNightDualTime", logentrymodel.crossCountryNightDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryNightPICTime", logentrymodel.crossCountryNightPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryDistance", logentrymodel.crossCountryDistance).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeFrom", logentrymodel.routeFrom).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeVia", logentrymodel.routeVia).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeTo", logentrymodel.routeTo).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@dualInstructionGivenTime", logentrymodel.dualInstructionGivenTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@floatTime", logentrymodel.floatTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@VFRSimulatorDualTime", logentrymodel.VFRSimulatorDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@CAF", logentrymodel.CAF).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@takeOffs", logentrymodel.takeOffs).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@landings", logentrymodel.landings).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@circuits", logentrymodel.circuits).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@omitFromReports", logentrymodel.omitFromReports).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@untetheredBalloon", logentrymodel.untetheredBalloon).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@altitudeBalloon", logentrymodel.altitudeBalloon).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@outsideCanada", logentrymodel.outsideCanada).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@launchLocationGlider", logentrymodel.launchLocationGlider).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@distanceGlider", logentrymodel.distanceGlider).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@launchTypeGlider", logentrymodel.launchTypeGlider).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@aircraftTypeID", logentrymodel.aircraftTypeID).Value = 149;
                    myCommand.Parameters.AddWithValue("@aircraftCategory", logentrymodel.aircraftCategory).Value ??= DBNull.Value;


                    myReader = myCommand.ExecuteReader();

                    table.Load(myReader);

                    myReader.Close();

                    myCon.Close();

                }
            }
            return new JsonResult("Added Successfully");
        }


        //GET RATINGS THAT ARE IN PROGRESS FOR DEMO PILOT (PILOTID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetInProgressRatings")]
        public JsonResult GetInProgressRatings(int pilotId)
        {
            string query = @"
            SELECT r.longName, r.shortName
            FROM rating AS ra 
            JOIN ratingType AS r ON ra.ratingTypeID = r.ratingTypeID 
            WHERE ra.pilotID = ? AND ra.isWorkingTowards>0;";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@PilotId", pilotId);
                    myCon.Open();
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();
                    }
                }
            }

            var ratings = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                ratings.Add(new
                {
                    LongName = row["longName"],
                    ShortName = row["shortName"]
                });
            }

            return new JsonResult(ratings);
        }




        //RETRIEVE COMPLETED RATINGS FOR DEMO PILOT (PILOTID = 2)
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetCompletedRatings")]
        public JsonResult GetCompletedRatings()
        {
            string query = @"
            SELECT r.longName, r.shortName, ra.dateAwarded 
            FROM rating AS ra 
            JOIN ratingType AS r ON ra.ratingTypeID = r.ratingTypeID 
            WHERE ra.pilotID = 2 AND ra.isWorkingTowards IS NULL;";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            var ratings = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                ratings.Add(new
                {
                    LongName = row["longName"],
                    ShortName = row["shortName"],
                    DateAwarded = row["dateAwarded"]
                });
            }

            return new JsonResult(ratings);
        }



        //RETRIEVING TOTAL SUM OF SINGLE ENGINE HOURS FOR LANDING PAGE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetTotalHours")]
        public JsonResult GetTotalHours()
        {
            string query = @"
            SELECT 
                ROUND(
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
                    COALESCE(instrumentHoodTime, 0)) AS TotalHours
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(new { TotalHours = table.Rows[0]["TotalHours"] });
        }

        //RETRIEVING PILOT NAME FOR LANDING PAGE
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetName")]
        public JsonResult GetName()
        {
            string query = @"SELECT firstName + ' ' + lastName AS FullName FROM pilot WHERE pilotID = 2;";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("CanFlyDBConn");
            SqlDataReader myReader;

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(new { FullName = table.Rows[0]["FullName"] });
        }
    }
}

