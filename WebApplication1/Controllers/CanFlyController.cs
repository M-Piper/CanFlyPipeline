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
            string query = @"
            --DROP PRIOR INSTANCE OF TEMPTABLE
                IF OBJECT_ID('tempdb..#TempReport') IS NOT NULL
                   BEGIN
                   DROP TABLE #TempReport;
                END


            --CREATE TEMPORARY TABLE
            CREATE TABLE #TempReport (
                pilotID INT,
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
            INSERT INTO #TempReport (pilotID, ratingName, ratingStatus)
            SELECT 
                p.pilotID,
                rt.longName AS ratingName,
                'InProgress' AS ratingStatus
            FROM pilot p
            LEFT JOIN rating r 
                ON p.pilotID = r.pilotID 
            LEFT JOIN ratingType rt
                ON r.ratingTypeID = rt.ratingTypeID
            WHERE p.pilotID = 2 
            AND r.isWorkingTowards IS NOT NULL 
            AND r.dateAwarded IS NULL;

            -- COMPLETED RATINGS FOR PILOT 2
            INSERT INTO #TempReport (pilotID, ratingName, ratingStatus, ratingDate)
            SELECT 
                p.pilotID,
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
            INSERT INTO #TempReport (pilotID, medicalName, medicalDate)
            SELECT 
                m.pilotID,
                mt.letterOrCertificate AS medicalName, 
                m.expiry AS medicalDate 
            FROM medical m 
            LEFT JOIN medicalType mt ON mt.medicalTypeID = m.medicalTypeID
            WHERE m.pilotID = 2;

            -- TOTAL TIME FOR PILOT 2
            INSERT INTO #TempReport (pilotID, totalTime) 
            SELECT 
                pilotID,
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
            INSERT INTO #TempReport (pilotID, totalPIC) 
            SELECT 
                pilotID,
                SUM(COALESCE(singleEngineDayPICTime, 0) +
                    COALESCE(singleEngineNightPICTime, 0) +
                    COALESCE(multiEngineDayPICTime, 0) +
                    COALESCE(multiEngineNightPICTime, 0)) AS totalPIC
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;

            -- DUAL HOURS TOTAL FOR PILOT 2
            INSERT INTO #TempReport (pilotID, totalDual) 
            SELECT 
                pilotID,
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
            INSERT INTO #TempReport (pilotID, timeOnType, typeName) 
            SELECT 
                pilotID,
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
            INSERT INTO #TempReport (pilotID, totalNight)
            SELECT 
                pilotID,
                SUM(COALESCE(singleEngineNightDualTime, 0) +
                    COALESCE(singleEngineNightPICTime, 0) +
                    COALESCE(multiEngineNightDualTime, 0) +
                    COALESCE(multiEngineNightPICTime, 0) +
                    COALESCE(multiEngineNightSICTime, 0)) AS totalNight
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;

            -- TOTAL NIGHT HOURS WITHOUT COUNTING INSTRUMENT HOURS FOR PILOT 2
            INSERT INTO #TempReport (pilotID, nightNoInstrument)
            SELECT 
                pilotID,
                SUM(COALESCE(singleEngineNightDualTime, 0) +
                    COALESCE(singleEngineNightPICTime, 0) +
                    COALESCE(multiEngineNightDualTime, 0) +
                    COALESCE(multiEngineNightPICTime, 0) +
                    COALESCE(multiEngineNightSICTime, 0)) AS nightNoInstrument
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;

            -- TOTAL INSTRUMENT HOURS FOR PILOT 2
            INSERT INTO #TempReport (pilotID, totalInstrument)
            SELECT 
                pilotID,
                SUM(COALESCE(instrumentActualTime, 0) +
                    COALESCE(instrumentHoodTime, 0)) AS totalInstrument
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;

            -- TOTAL CROSS COUNTRY HOURS FOR PILOT 2
            INSERT INTO #TempReport (pilotID, totalCrossCountry)
            SELECT 
                pilotID,
                SUM(COALESCE(crossCountryDayDualTime, 0) +
                    COALESCE(crossCountryDayPICTime, 0) +
                    COALESCE(crossCountryNightDualTime, 0) +
                    COALESCE(crossCountryNightPICTime, 0)) AS totalCrossCountry
            FROM logEntry
            WHERE pilotID = 2
            GROUP BY pilotID;


            --TOTAL FOR LAST 30 DAYS
            INSERT INTO #TempReport (pilotID, totalLast30Days) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(DAY, -30, GETDATE())
            GROUP BY pilotID;


            --TOTAL LAST 90 DAYS
            INSERT INTO #TempReport (pilotID, totalLast90Days) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(DAY, -90, GETDATE())
            GROUP BY pilotID;


            --TOTAL LAST 6 MONTHS
            INSERT INTO #TempReport (pilotID, totalLast6Months) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(MONTH, -6, GETDATE())
            GROUP BY pilotID;

            --TOTAL LAST 12 MONTHS
            INSERT INTO #TempReport (pilotID, totalLast12Months) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(MONTH, -12, GETDATE())
            GROUP BY pilotID;


            --TOTAL LAST 24 MONTHS
            INSERT INTO #TempReport (pilotID, totalLast24Months) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(MONTH, -24, GETDATE())
            GROUP BY pilotID;

            --TOTAL LAST 6 MONTHS
            INSERT INTO #TempReport (pilotID, totalLast60Months) 
            SELECT 
                pilotID,
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
            WHERE pilotID = 2
            AND date >= DATEADD(MONTH, -60, GETDATE())
            GROUP BY pilotID;

            --DAYS SINCE PILOT IN COMMAND

            --DAYS SINCE IPC

            --DAYS SINCE CURRENCY UPGRADE

            -- Select from the temporary table
            SELECT * FROM #TempReport;";

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

