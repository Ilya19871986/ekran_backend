﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
        private PlayersDBContext db2;
        public PanelController(PanelsDBContext context, PlayersDBContext context2)
        {
            db = context;
            db2 = context2;
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
        public async Task<IActionResult> ChangePanel([FromForm] int? id, [FromForm]int run_text, [FromForm]int time_vip, [FromForm]string address, [FromForm]string newName, [FromForm] int OnlyVip)
        {
            if (id != null)
            {
                var panel = await db.Panels.FirstOrDefaultAsync(p => p.id == id);
                panel.panel_name = newName;
                panel.run_text = run_text;
                panel.time_vip = time_vip;
                panel.address = address;
                panel.only_vip = OnlyVip;
                db.Update(panel);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        [Authorize]
        [Route("createPanel")]
        [HttpGet]
        public async Task<IActionResult> CreatePanel(string panelName, string username)
        {
            User user = await db2.Users.FirstOrDefaultAsync(p => p.user_name == username);
            if (user != null)
            {
                Panel panel = new Panel();
                panel.panel_name = panelName;
                panel.user_id = user.Id;
                panel.player_version = "3.0.0";
                db.Add(panel);
                
                await db.SaveChangesAsync();

                panel = await  db.Panels.FirstOrDefaultAsync(p => p.panel_name == panelName);

                DirectoryInfo dirInfo = new DirectoryInfo(user.working_folder + @"\" + panelName);

                dirInfo.Create();

                dirInfo.CreateSubdirectory("Видео");

                return Json(panel);
            }
            else
            {
                return BadRequest("User not found");
            }
        }
    }
}
