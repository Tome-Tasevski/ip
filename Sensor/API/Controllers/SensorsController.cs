using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : Controller
    {
        //[Authorize(Roles ="User")]
        [HttpGet("User")]
        public IEnumerable<SensorData> Get()
        {
            return new List<SensorData>
            {
                new SensorData {SensorId = "1", Name = "L1", Type = "temperature", Value = "23.1"},
                new SensorData {SensorId = "2", Name = "L2", Type = "temperature", Value = "23.2"},
                new SensorData {SensorId = "3", Name = "L3", Type = "temperature", Value = "23.5"},
                new SensorData {SensorId = "4", Name = "H", Type = "humidity", Value = "40"},
                new SensorData {SensorId = "5", Name = "Out", Type = "temperature", Value = "12"},
                new SensorData {SensorId = "6", Name = "In", Type = "temperature", Value = "23.5"},
            };

        }

        //[Authorize(Roles = "Moderator")]
        [HttpGet("Moderator")]
        public IEnumerable<SensorData> GetAsModerator()
        {
            return new List<SensorData>
            {
                new SensorData {SensorId = "1", Name = "L1", Type = "temperature", Value = "23.1"},
                new SensorData {SensorId = "2", Name = "L2", Type = "temperature", Value = "23.2"},
                new SensorData {SensorId = "3", Name = "L3", Type = "temperature", Value = "23.5"},
                new SensorData {SensorId = "4", Name = "H", Type = "humidity", Value = "40"},
                new SensorData {SensorId = "5", Name = "Out", Type = "temperature", Value = "12"},
                new SensorData {SensorId = "6", Name = "In", Type = "temperature", Value = "23.5"},
            };
        }

        //[Authorize(Roles ="Admin")]
        [HttpGet("Admin")]
        public IEnumerable<SensorData> GetAsAdmin()
        {
            return new List<SensorData>
            {
                new SensorData {SensorId = "1", Name = "L1", Type = "temperature", Value = "23.1"},
                new SensorData {SensorId = "2", Name = "L2", Type = "temperature", Value = "23.2"},
                new SensorData {SensorId = "3", Name = "L3", Type = "temperature", Value = "23.5"},
                new SensorData {SensorId = "4", Name = "H", Type = "humidity", Value = "40"},
                new SensorData {SensorId = "5", Name = "Out", Type = "temperature", Value = "12"},
                new SensorData {SensorId = "6", Name = "In", Type = "temperature", Value = "23.5"},
            };
        }
    }
}
