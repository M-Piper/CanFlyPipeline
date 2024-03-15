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
                dualInstructionGivenNotes, floatTimeNotes, VFRSimulatorNotes, 
                planeTypeID, pilotID, CAF, 
                takeOffs, landings, circuits, omitFromReports, 
                untetheredBalloon, altitudeBalloon, outsideCanada, instrumentGroundOptional, 
                launchLocationGlider, distanceGlider, launchTypeGlider, 
                aircraftCategory, aircraftTypeID) 
            VALUES (
                @Date, @Registration, @PilotInCommand, @StudentOrCoPilot, @ActivityExercises, 
                @SingleEngineDayDualTime, @SingleEngineDayPICTime, @SingleEngineNightDualTime, @SingleEngineNightPICTime, 
                @MultiEngineDayDualTime, @MultiEngineDayPICTime, @MultiEngineDaySICTime, @MultiEngineNightDualTime, @MultiEngineNightPICTime, @MultiEngineNightSICTime, 
                @InstrumentIMC, @InstrumentHood, @InstrumentFTD, @InstrumentApproachesCount, 
                @CrossCountryDayDualTime, @CrossCountryDayPICTime, @CrossCountryNightDualTime, @CrossCountryNightPICTime, 
                @RouteFrom, @RouteVia, @RouteTo, 
                @DualInstructionGivenNotes, @FloatTimeNotes, @VFRSimulatorNotes, 
                @PlaneTypeID, @PilotID, @CAF, 
                @TakeOffs, @Landings, @Circuits, @OmitFromReports, 
                @UntetheredBalloon, @AltitudeBalloon, @OutsideCanada, @InstrumentGroundOptional, 
                @LaunchLocationGlider, @DistanceGlider, @LaunchTypeGlider, 
                @AircraftCategory, @AircraftTypeID)";


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


                    myReader = myCommand.ExecuteReader();

                    table.Load(myReader);

                    myReader.Close();

                    myCon.Close();

                }

            }



            return new JsonResult("Added Successfully");

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

