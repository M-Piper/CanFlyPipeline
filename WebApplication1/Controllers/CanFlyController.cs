using java.sql;
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
            string query = "--DROP PRIOR INSTANCE OF TEMPTABLE\r\nDROP TABLE #TempReport;\r\n\r\n--CREATE TEMPORARY TABLE\r\nCREATE TABLE #TempReport (\r\n    pilotID INT,\r\n    ratingName VARCHAR(255),\r\n    ratingStatus VARCHAR(50),\r\n    ratingDate DATE,\r\n    medicalName VARCHAR(255),\r\n    medicalDate DATE,\r\n    totalTime DECIMAL(10, 2),\r\n    totalPIC DECIMAL(10, 2),\r\n    totalDual DECIMAL(10, 2),\r\n    timeOnType DECIMAL(10, 2),\r\n    typeName VARCHAR(255),\r\n    totalNight DECIMAL(10, 2),\r\n    nightNoInstrument DECIMAL(10, 2),\r\n    totalInstrument DECIMAL(10, 2),\r\n    totalCrossCountry DECIMAL(10, 2),\r\n    totalLast30Days DECIMAL(10, 2),\r\n    totalLast90Days DECIMAL(10, 2),\r\n    totalLast6Months DECIMAL(10, 2),\r\n    totalLast12Months DECIMAL(10, 2),\r\n    totalLast24Months DECIMAL(10, 2),\r\n    totalLast60Months DECIMAL(10, 2),\r\n    approachesLast6Months DECIMAL(10, 2),\r\n    daysSincePIC DECIMAL(10, 2),\r\n    daysSinceIPC DECIMAL(10, 2),\r\n    daysSinceCurrencyUpgrade DECIMAL(10, 2)\r\n);\r\n\r\n-- IN-PROGRESS RATINGS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, ratingName, ratingStatus)\r\nSELECT \r\n    p.pilotID,\r\n    rt.longName AS ratingName,\r\n    'InProgress' AS ratingStatus\r\nFROM pilot p\r\nLEFT JOIN rating r \r\n    ON p.pilotID = r.pilotID \r\nLEFT JOIN ratingType rt\r\n    ON r.ratingTypeID = rt.ratingTypeID\r\nWHERE p.pilotID = 2 \r\nAND r.isWorkingTowards IS NOT NULL \r\nAND r.dateAwarded IS NULL;\r\n\r\n-- COMPLETED RATINGS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, ratingName, ratingStatus, ratingDate)\r\nSELECT \r\n    p.pilotID,\r\n    rt.longName AS ratingName, \r\n    'Completed' AS ratingStatus, \r\n    r.dateAwarded AS ratingDate \r\nFROM pilot p \r\nLEFT JOIN rating r ON p.pilotID = r.pilotID \r\nLEFT JOIN ratingType rt ON r.ratingTypeID = rt.ratingTypeID\r\nWHERE p.pilotID = 2 \r\nAND r.isWorkingTowards IS NULL \r\nAND r.dateAwarded IS NOT NULL;\r\n\r\n-- MEDICAL FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, medicalName, medicalDate)\r\nSELECT \r\n    m.pilotID,\r\n    mt.letterOrCertificate AS medicalName, \r\n    m.expiry AS medicalDate \r\nFROM medical m \r\nLEFT JOIN medicalType mt ON mt.medicalTypeID = m.medicalTypeID\r\nWHERE m.pilotID = 2;\r\n\r\n-- TOTAL TIME FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalTime) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalTime\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- PILOT IN COMMAND HOURS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalPIC) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0)) AS totalPIC\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- DUAL HOURS TOTAL FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalDual) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalDual\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- TOTAL TIME ON TYPE FOR PILOT 2 FOR CESSNA 172 (aircraftTypeID = 149)\r\nINSERT INTO #TempReport (pilotID, timeOnType, typeName) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS timeOnType, \r\n    'Cessna 172' AS typeName\r\nFROM logEntry\r\nWHERE pilotID = 2 AND aircraftTypeID = 149\r\nGROUP BY pilotID;\r\n\r\n-- TOTAL NIGHT HOURS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalNight)\r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0)) AS totalNight\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- TOTAL NIGHT HOURS WITHOUT COUNTING INSTRUMENT HOURS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, nightNoInstrument)\r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0)) AS nightNoInstrument\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- TOTAL INSTRUMENT HOURS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalInstrument)\r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalInstrument\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n-- TOTAL CROSS COUNTRY HOURS FOR PILOT 2\r\nINSERT INTO #TempReport (pilotID, totalCrossCountry)\r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(crossCountryDayDualTime, 0) +\r\n        COALESCE(crossCountryDayPICTime, 0) +\r\n        COALESCE(crossCountryNightDualTime, 0) +\r\n        COALESCE(crossCountryNightPICTime, 0)) AS totalCrossCountry\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nGROUP BY pilotID;\r\n\r\n\r\n--TOTAL FOR LAST 30 DAYS\r\nINSERT INTO #TempReport (pilotID, totalLast30Days) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast30Days\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(DAY, -30, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n\r\n--TOTAL LAST 90 DAYS\r\nINSERT INTO #TempReport (pilotID, totalLast90Days) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast90Days\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(DAY, -90, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n\r\n--TOTAL LAST 6 MONTHS\r\nINSERT INTO #TempReport (pilotID, totalLast6Months) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast6Months\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(MONTH, -6, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n--TOTAL LAST 12 MONTHS\r\nINSERT INTO #TempReport (pilotID, totalLast12Months) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast12Months\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(MONTH, -12, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n\r\n--TOTAL LAST 24 MONTHS\r\nINSERT INTO #TempReport (pilotID, totalLast24Months) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast24Months\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(MONTH, -24, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n--TOTAL LAST 6 MONTHS\r\nINSERT INTO #TempReport (pilotID, totalLast60Months) \r\nSELECT \r\n    pilotID,\r\n    SUM(COALESCE(singleEngineDayDualTime, 0) +\r\n        COALESCE(singleEngineDayPICTime, 0) +\r\n        COALESCE(singleEngineNightDualTime, 0) +\r\n        COALESCE(singleEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineDayDualTime, 0) +\r\n        COALESCE(multiEngineDayPICTime, 0) +\r\n        COALESCE(multiEngineDaySICTime, 0) +\r\n        COALESCE(multiEngineNightDualTime, 0) +\r\n        COALESCE(multiEngineNightPICTime, 0) +\r\n        COALESCE(multiEngineNightSICTime, 0) +\r\n        COALESCE(instrumentActualTime, 0) +\r\n        COALESCE(instrumentHoodTime, 0)) AS totalLast60Months\r\nFROM logEntry\r\nWHERE pilotID = 2\r\nAND date >= DATEADD(MONTH, -60, GETDATE())\r\nGROUP BY pilotID;\r\n\r\n--DAYS SINCE PILOT IN COMMAND\r\n\r\n--DAYS SINCE IPC\r\n\r\n--DAYS SINCE CURRENCY UPGRADE\r\n\r\n-- Select from the temporary table\r\nSELECT * FROM #TempReport;\r\n";
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
                instrumentIMC, instrumentHood, instrumentFTD, instrumentApproachesCount, 
                crossCountryDayDualTime, crossCountryDayPICTime, crossCountryNightDualTime, crossCountryNightPICTime, 
                routeFrom, routeVia, routeTo, 
                dualInstructionGivenNotes, floatTimeNotes, VFRSimulatorNotes, CAF, 
                takeOffs, landings, circuits, omitFromReports, 
                untetheredBalloon, altitudeBalloon, outsideCanada, instrumentGroundOptional, 
                launchLocationGlider, distanceGlider, launchTypeGlider, 
                aircraftCategory, aircraftTypeID, pilotID) 
            VALUES (
                @Date, @Registration, @PilotInCommand, @StudentOrCoPilot, @ActivityExercises, 
                @SingleEngineDayDualTime, @SingleEngineDayPICTime, @SingleEngineNightDualTime, @SingleEngineNightPICTime, 
                @MultiEngineDayDualTime, @MultiEngineDayPICTime, @MultiEngineDaySICTime, @MultiEngineNightDualTime, @MultiEngineNightPICTime, @MultiEngineNightSICTime, 
                @InstrumentIMC, @InstrumentHood, @InstrumentFTD, @InstrumentApproachesCount, 
                @CrossCountryDayDualTime, @CrossCountryDayPICTime, @CrossCountryNightDualTime, @CrossCountryNightPICTime, 
                @RouteFrom, @RouteVia, @RouteTo, 
                @DualInstructionGivenNotes, @FloatTimeNotes, @VFRSimulatorNotes, 
                @CAF, 
                @TakeOffs, @Landings, @Circuits, @OmitFromReports, 
                @UntetheredBalloon, @AltitudeBalloon, @OutsideCanada, @InstrumentGroundOptional, 
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
                    myCommand.Parameters.AddWithValue("@instrumentIMC", logentrymodel.instrumentIMC).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentHood", logentrymodel.instrumentHood).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentFTD", logentrymodel.instrumentFTD).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentApproachesCount", logentrymodel.instrumentApproachesCount).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryDayDualTime", logentrymodel.crossCountryDayDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryDayPICTime", logentrymodel.crossCountryDayPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryNightDualTime", logentrymodel.crossCountryNightDualTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@crossCountryNightPICTime", logentrymodel.crossCountryNightPICTime).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeFrom", logentrymodel.routeFrom).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeVia", logentrymodel.routeVia).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@routeTo", logentrymodel.routeTo).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@dualInstructionGivenNotes", logentrymodel.dualInstructionGivenNotes).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@floatTimeNotes", logentrymodel.floatTimeNotes).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@VFRSimulatorNotes", logentrymodel.VFRSimulatorNotes).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@CAF", logentrymodel.CAF).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@takeOffs", logentrymodel.takeOffs).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@landings", logentrymodel.landings).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@circuits", logentrymodel.circuits).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@omitFromReports", logentrymodel.omitFromReports).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@untetheredBalloon", logentrymodel.untetheredBalloon).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@altitudeBalloon", logentrymodel.altitudeBalloon).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@outsideCanada", logentrymodel.outsideCanada).Value ??= DBNull.Value;
                    myCommand.Parameters.AddWithValue("@instrumentGroundOptional", logentrymodel.instrumentGroundOptional).Value ??= DBNull.Value;
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
            WHERE ra.pilotID = @PilotId AND ra.isWorkingTowards>0;";

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
        public JsonResult GetCompletedRatings(int pilotId)
        {
            string query = @"
            SELECT r.longName, r.shortName, ra.dateAwarded 
            FROM rating AS ra 
            JOIN ratingType AS r ON ra.ratingTypeID = r.ratingTypeID 
            WHERE ra.pilotID = @PilotId AND ra.isWorkingTowards IS NULL;";

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
                SUM(
                    COALESCE(singleEngineDayDualTime, 0) + 
                    COALESCE(singleEngineDayPICTime, 0) + 
                    COALESCE(singleEngineNightDualTime, 0) +
                    COALESCE(singleEngineNightPICTime, 0)
                ),
                2) as TotalHours
            FROM logEntry;";

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

