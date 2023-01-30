using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using health_path.Model;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IDbConnection _connection;

    public ScheduleController(ILogger<ScheduleController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpGet]
     public ActionResult<IEnumerable<ScheduleEvent>> Fetch()
    {
        var dbResults = ReadData();
        //first we store a grouping of an event based on the name
        var combinedResults = dbResults
            .GroupBy(x => x.Item1.Name)
            .Select(g => 
            {
                //we select the first instance of a scheduleevent object  
                var firstEvent = g.First().Item1;
                 // Set the recurrences for the first event to the list of recurrences from the grouped results
                firstEvent.Recurrences = g.Select(x => x.Item2).ToList();
                return firstEvent;
            });

        return Ok(combinedResults);
    }

    private IEnumerable<(ScheduleEvent, ScheduleEventRecurrence)> ReadData() {
        var sql = @"
            SELECT e.*, r.*
            FROM Event e
            JOIN EventRecurrence r ON e.Id = r.EventId
            ORDER BY e.Id, r.DayOfWeek, r.StartTime, r.EndTime
        ";
        return _connection.Query<ScheduleEvent, ScheduleEventRecurrence, (ScheduleEvent, ScheduleEventRecurrence)>(sql, (e, r) => (e, r));
    }
}
