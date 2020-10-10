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
                var panels = db.Panels.Where(p => p.user_id == UserId);
                return Json(panels);
            }
            return BadRequest("UserId is null");
        }
        // изменить настройки панели
        [Authorize]
        [Route("ChangePanel")]
        [HttpPost]
        public async Task<IActionResult> ChangePanel([FromForm] int? id, [FromForm] int run_text, [FromForm] int time_vip, [FromForm] string address, [FromForm] string newName, [FromForm] int OnlyVip)
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

                panel = await db.Panels.FirstOrDefaultAsync(p => p.panel_name == panelName);

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
                panel.last_connect = DateTime.Now.ToString("s");
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

        [Authorize]
        [HttpGet]
        [Route("createGroup")]
        public async Task<IActionResult> insertGroupPanel(string GroupName, string comment, int user_id)
        {
            if (db.GroupPanels.Count(p => p.group_name == GroupName) != 0)
            {
                return BadRequest("Группа с таким названием уже существует");
            }

            GroupPanel groupPanel = new GroupPanel();

            groupPanel.user_id = user_id;
            groupPanel.group_name = GroupName;
            groupPanel.comment = comment;

            await db.AddAsync(groupPanel);
            await db.SaveChangesAsync();

            return Ok("Группа успешно добавлена");
        }

        [Authorize]
        [HttpGet]
        [Route("getGroupPanels")]
        public IActionResult getGroupPanels(int user_id)
        {
            var groups = db.GroupPanels.Where(p => p.user_id == user_id);

            return Json(groups);
        }

        [Authorize]
        [HttpGet]
        [Route("deleteGroup")]
        public async Task<IActionResult> deleteGroup(int id)
        {
            var group = await db.GroupPanels.FirstAsync(x => x.Id == id);

            if (group != null)
            {
                db.GroupPanels.Remove(group);
                await db.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Группа не найдена");
            }
            return Ok("Группа успешно удалена");
        }

        [Authorize]
        [HttpGet]
        [Route("change")]
        public async Task<IActionResult> changeNameCommentGroup(int id, string name, string comment)
        {
            var group = await db.GroupPanels.FirstAsync(x => x.Id == id);

            if (group == null)
            {
                return BadRequest("Группа не найдена");
            }
            else
            {
                group.group_name = name;
                group.comment = comment;
                db.Update(group);
                await db.SaveChangesAsync();
            }
            return Ok("Группа успешно изменена");
        }

        [Authorize]
        [HttpGet]
        [Route("getPanelsInGroup")]
        public IActionResult getPanelsInGroup(int user_id, int group_id)
        {
            var panels = db.Panels.Where(p => p.user_id == user_id && p.group_id == group_id);

            return Json(panels);
        }

        [Authorize]
        [HttpGet]
        [Route("getPanelsNoGroup")]
        public IActionResult getPanelsNoGroup(int user_id)
        {
            var panels = db.Panels.Where(p => p.user_id == user_id && p.group_id == 0);

            return Json(panels);
        }

        [Authorize]
        [HttpGet]
        [Route("setGroup")]
        public async Task<IActionResult> setGroup(int panel_id, int group_id)
        {
            Panel panel = await db.Panels.FirstAsync(p => p.id == panel_id);

            if (panel == null)
            {
                return BadRequest("Панель не найдена");
            }
            else
            {
                panel.group_id = group_id;
                db.Panels.Update(panel);
                await db.SaveChangesAsync();
                return Ok("Группа установлена");
            }
        }
    }
}
