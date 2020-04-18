using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.Models;

namespace webapi.Controllers
{
    [ApiController]
    [Route("content")]
    public class ContentController : Controller
    {
        private PanelsDBContext db;
        public ContentController(PanelsDBContext context)
        {
            db = context;
        }

        [Authorize]
        [HttpGet]
        [Route("GetContent")]
        public IActionResult GetContent(int? PanelId)
        {
            if (PanelId != null)
            {
                var content = db.Content.Where(p => p.panel_id == PanelId);
                return Json(content);
            }
            return BadRequest("not found");
        }
    }
}
