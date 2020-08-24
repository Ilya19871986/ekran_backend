using Microsoft.AspNetCore.Authorization;
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
            Panel checkPanelName = await db.Panels.FirstOrDefaultAsync(p => p.panel_name == panelName);
            if (checkPanelName != null) return BadRequest();

            User user = await db2.Users.FirstOrDefaultAsync(p => p.user_name == username);
            if (user != null)
            {
                Panel panel = new Panel();
                panel.panel_name = panelName.Replace(" ", "_");
                panel.user_id = user.Id;
                panel.player_version = "3.0.0";
                panel.time_vip = 5;
                db.Add(panel);
                
                await db.SaveChangesAsync();

                panel = await  db.Panels.FirstOrDefaultAsync(p => p.panel_name == panelName);

                DirectoryInfo dirInfo = new DirectoryInfo(user.working_folder + @"\" + panelName);

                dirInfo.Create();

                dirInfo.CreateSubdirectory("Видео");
                dirInfo.CreateSubdirectory("update");

                return Json(panel);
            }
            else
            {
                return BadRequest("User not found");
            }
        }

        private string ContentName(int type)
        {
            var typeName = "";
            switch (type)
            {
                case 1: typeName = "Акции"; break;
                case 2: typeName = "Афиша"; break;
                case 3: typeName = "Объявления"; break;
                case 4: typeName = "Видео"; break;
                case 5: typeName = "Строка"; break;
                case 6: typeName = "Vip"; break;
                default:
                    typeName = ""; break;
            }
            return typeName;
        }

        // удалить файл из БД
        [Authorize]
        [HttpGet]
        [Route("deleteFile")]
        public async Task<IActionResult> DeleteFileDb(int? id)
        {
            Content content = await db.Content.FirstOrDefaultAsync(p => p.Id == id);
            if (content != null)
            {
                db.Remove(content);
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest("File not found");
            }
        }
        [Authorize]
        [HttpGet]
        [Route("endUploadingFile")]
        public async Task<IActionResult> FileUploadEnd(int? id)
        {
            Content content = await db.Content.FirstOrDefaultAsync(p => p.Id == id);
            if (content != null)
            {
                content.sync = 1;
                db.Update(content);
                await db.SaveChangesAsync();

                Panel panel = await db.Panels.FirstOrDefaultAsync(p => p.id == content.panel_id);
                User user = await db2.Users.FirstOrDefaultAsync(p => p.Id == panel.user_id);
                var path = user.working_folder + @"\" + panel.panel_name + @"\" + ContentName(content.type_content) + @"\" + content.file_name;

                System.IO.File.Delete(path);

                return Ok();
            }
            return BadRequest();
        }
        [Authorize]
        [HttpGet]
        [Route("connect")]
        public async Task<IActionResult> updateConnectTime(int? id)
        {
            Panel panel = await db.Panels.FirstOrDefaultAsync(p => p.id == id);
            if (panel != null)
            {
                panel.last_connect =  DateTime.Now.ToString("s");
                db.Update(panel);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        [Authorize]
        [HttpGet]
        [Route("checkNewVersion")]
        public async Task<IActionResult> checkNewVersion(int? PanelId)
        {
            Panel panel = await db.Panels.FirstOrDefaultAsync(p => p.id == PanelId);
            if (panel != null)
            {
                return Json(panel);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("deletePanel")]
        // удалить панель
        public async Task<IActionResult> deletePanel(int? PanelId)
        {
            Panel panel = await db.Panels.FirstOrDefaultAsync(p => p.id == PanelId);
            
            User user = await db2.Users.FirstOrDefaultAsync(p => p.Id == panel.user_id);

            DirectoryInfo dirInfo = new DirectoryInfo(user.working_folder + @"\" + panel.panel_name);

           
            dirInfo.Delete(true);

            if (panel != null)
            {
                db.Remove(panel);
                await db.SaveChangesAsync();
                return Ok("Панель удалена");
            }
            return BadRequest("Панель не найдена");
        }
    }
}
