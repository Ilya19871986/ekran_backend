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
    [Route("panel")]
    public class PanelController : Controller
    {
        private PanelsDBContext db;
        public PanelController(PanelsDBContext context)
        {
            db = context;
        }
        // получаем список панелей пользователя
        [Authorize]
        [Route("GetMyPanels")]
        public IActionResult GetMyPanels(int? UserId)
        {
            if (UserId != null)
            {
                var panels =  db.Panels.Where(p => p.user_id == UserId);
                return Json(panels);
            }
            return BadRequest("UserId is null");
        }
        // изменить настройки панели
        [Authorize]
        [Route("ChangePanel")]
        [HttpPost]
        public async Task<IActionResult> ChangePanel([FromForm] int? id, [FromForm]int run_text, [FromForm]int time_vip, [FromForm]string address, [FromForm]string newName)
        {
            if (id != null)
            {
                var panel = await db.Panels.FirstOrDefaultAsync(p => p.id == id);
                panel.panel_name = newName;
                panel.run_text = run_text;
                panel.time_vip = time_vip;
                panel.address = address;
                db.Update(panel);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
    }
}
