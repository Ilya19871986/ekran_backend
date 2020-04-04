using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.Models;

namespace webapi.Controllers
{
    [ApiController]
    [Route("bus")]
    public class BusController : Controller
    {
        private BusDBContext db;
        public BusController(BusDBContext context)
        {
            db = context;
        }
        // список автобусов отсортированных по времени отправления
        [Authorize]
        [HttpGet]
        [Route("GetScheduleBus")]
        public async Task<IActionResult> GetListBuses()
        {
            List<Bus> scheduleBuses = await db.Buses.OrderBy(t => t.departure_time).ToListAsync();
            return Json(scheduleBuses);
        }
        // удалить автобус по id
        [Authorize]
        [Route("Delete")]
        [HttpGet]
        public async Task<IActionResult> DeleteBus(int? id)
        {
            Bus bus = await db.Buses.FirstOrDefaultAsync(x => x.Id == id);
            db.Buses.Remove(bus);
            await db.SaveChangesAsync();
            return Json(bus);
        }
        // добавить автобус
        [Authorize]
        [HttpPost]
        [Route("AddBus")]
        public async Task<IActionResult> AddBus(BusTemp bus2)
        {
            if (bus2 == null)
            {
                return BadRequest();
            }

            Bus bus = new Bus();

            bus.from_ = bus2.from_;
            bus.to_ = bus2.to_;
            bus.departure_time = TimeSpan.Parse(bus2.departure_time);
            bus.platform = Int32.Parse(bus2.platform);
            bus.Monday = bus2.Monday ? 1 : 0;
            bus.Tuesday = bus2.Tuesday ? 1 : 0;
            bus.Wednesday = bus2.Wednesday ? 1 : 0;
            bus.Thursday = bus2.Thursday ? 1 : 0;
            bus.Friday = bus2.Friday ? 1 : 0;
            bus.Saturday = bus2.Saturday ? 1 : 0;
            bus.Sunday = bus2.Sunday ? 1 : 0;
            bus.SeeTime = Int32.Parse(bus2.SeeTime);

            db.Buses.Add(bus);
            await db.SaveChangesAsync();
            return Json(bus);
        }
        // изменяем автобус
        [Route("UpdateBus")]
        [HttpPost]
        public async Task<IActionResult> Edit(BusTemp bus2)
        {
            if (bus2 == null)
            {
                return BadRequest();
            }
            if (!db.Buses.Any(x => x.Id == bus2.Id))
            {
                return NotFound();
            }

            Bus bus = new Bus();

            bus.Id = bus2.Id;
            bus.from_ = bus2.from_;
            bus.to_ = bus2.to_;
            bus.departure_time = TimeSpan.Parse(bus2.departure_time);
            bus.platform = Int32.Parse(bus2.platform);
            bus.canceled = bus2.canceled;
            bus.Monday = bus2.Monday ? 1 : 0;
            bus.Tuesday = bus2.Tuesday ? 1 : 0;
            bus.Wednesday = bus2.Wednesday ? 1 : 0;
            bus.Thursday = bus2.Thursday ? 1 : 0;
            bus.Friday = bus2.Friday ? 1 : 0;
            bus.Saturday = bus2.Saturday ? 1 : 0;
            bus.Sunday = bus2.Sunday ? 1 : 0;
            bus.SeeTime = Int32.Parse(bus2.SeeTime);
            
            db.Update(bus);
            await db.SaveChangesAsync();
            
            return Json(bus);
        }
    }
}
