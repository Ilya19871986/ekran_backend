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
    }
}
