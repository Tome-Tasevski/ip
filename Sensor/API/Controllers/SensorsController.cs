using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class SensorsController : Controller
    {
        [HttpGet("user")]
        [Authorize(Roles ="User")]
        public IEnumerable<SensorData> GetUser()
        {
            return new List<SensorData>
            {
                new SensorData {SensorId = "1", Name = "L1-User", Type = "temperature", Value = "23.1"},
                new SensorData {SensorId = "2", Name = "L2-User", Type = "temperature", Value = "23.2"},
                new SensorData {SensorId = "3", Name = "L3-User", Type = "temperature", Value = "23.5"},
                new SensorData {SensorId = "4", Name = "H-User", Type = "humidity", Value = "40"},
                new SensorData {SensorId = "5", Name = "Out-User", Type = "temperature", Value = "12"},
                new SensorData {SensorId = "6", Name = "In-User", Type = "temperature", Value = "23.5"},
            };

        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IEnumerable<SensorData> GetAdmin()
        {
            return new List<SensorData>
            {
                new SensorData {SensorId = "1", Name = "L1-Admin", Type = "temperature", Value = "23.1"},
                new SensorData {SensorId = "2", Name = "L2-Admin", Type = "temperature", Value = "23.2"},
                new SensorData {SensorId = "3", Name = "L3-Admin", Type = "temperature", Value = "23.5"},
                new SensorData {SensorId = "4", Name = "H-Admin", Type = "humidity", Value = "40"},
                new SensorData {SensorId = "5", Name = "Out-Admin", Type = "temperature", Value = "12"},
                new SensorData {SensorId = "6", Name = "In-Admin", Type = "temperature", Value = "23.5"},
            };

        }

        [HttpGet("test")]
        public IActionResult GetTest()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}
