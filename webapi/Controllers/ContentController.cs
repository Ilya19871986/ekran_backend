﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using webapi.Models;

namespace webapi.Controllers
{
    [ApiController]
    [Route("content")]
    public class ContentController : Controller
    {
        private PanelsDBContext db;
        private PlayersDBContext dbUser;

        public ContentController(PanelsDBContext context, PlayersDBContext contextUser)
        {
            db = context;
            dbUser = contextUser;
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

        [Authorize]
        [HttpGet]
        [Route("GetContentType")]
        public IActionResult GetContentType(int? PanelId, int? type)
        {
            if (PanelId != null)
            {
                var content = db.Content.Where(p => p.panel_id == PanelId && p.type_content == type);
                return Json(content);
            }
            return BadRequest("not found");
        }

        // загрузка файла
        [Authorize]
        [HttpPost]
        [Route("AddFile")]
        public async Task<IActionResult> AddFile(
            [FromForm]IFormFile uploadedFile, [FromForm]string path, [FromForm]int panel_id, [FromForm]int user_id, [FromForm]int type_content
        )
        {
            try
            {
                if (uploadedFile != null)
                {
                    string FileName = uploadedFile.FileName.Trim().Replace(' ', '_'); 

                    if (FileName.Contains("mp4") || (FileName.Contains("jpeg")) || (FileName.Contains("jpg")) || (FileName.Contains("png")))
                    {
                            using (var fileStream = new FileStream(path + FileName, FileMode.Create))
                            {
                                await uploadedFile.CopyToAsync(fileStream);
                            }

                            Content content = new Content();
                            content.file_name = FileName;
                            content.file_size = (int)uploadedFile.Length;
                            content.sync = 0;
                            content.deleted = 0;
                            content.end_date = DateTime.Parse("2999-01-01");
                            content.user_id = user_id;
                            content.panel_id = panel_id;
                            content.type_content = type_content;

                            db.Content.Add(content);
                            await db.SaveChangesAsync();

                            return Ok();
                    }
                    else
                    {
                        return BadRequest("null");
                    }
                }
                else
                    return BadRequest("null");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        // удалить файл
        [Authorize]
        [HttpPost]
        [Route("DeleteFile")]
        public async Task<IActionResult> DeleteFile([FromForm]int? id)
        {
            if (id != null)
            {
                var content =  await db.Content.FirstOrDefaultAsync(p => p.Id == id);

                content.deleted = 1;
                content.sync = 2;
                db.Update(content);
                await db.SaveChangesAsync();
                return Ok();
            };
            return BadRequest();
        }
        // обновить время удаления файла
        [Authorize]
        [HttpPost]
        [Route("UpdateFile")]
        public async Task<IActionResult> UpdateFile([FromForm] int? id, [FromForm] DateTime newDate)
        {
            if ((id != null) && (newDate != null))
            {
                var content = await db.Content.FirstOrDefaultAsync(p => p.Id == id);
                content.end_date = newDate;
                db.Update(content);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        // список файлов для загрузки
        [Authorize]
        [HttpGet]
        [Route("toUpload")]
        public IActionResult ToUpload(int? PanelId)
        {
            var content = db.Content.Where(p => (p.panel_id == PanelId) && (p.sync == 0));
            if (content != null)
            {
                return Json(content);
            };
            return Json("result: not_found");
        }
        [Authorize]
        [HttpGet]
        [Route("GetDeletedFile")]
        public IActionResult GetDeletedFile(int? id)
        {
            var content = db.Content.Where(p => (p.panel_id == id) && (p.deleted == 1));
            if (content != null)
            {
                return Json(content);
            }
            return BadRequest();
        }
        // загрузка файла в группу
        [Authorize]
        [HttpPost]
        [Route("AddFileGroup")]
        public async Task<IActionResult> AddFileGroup(
            [FromForm] IFormFile uploadedFile, [FromForm] string path, [FromForm] int group_id, [FromForm] int user_id, 
            [FromForm] int type_content)
        {
            try
            {
                if (uploadedFile != null)
                {
                    string FileName = uploadedFile.FileName.Trim().Replace(' ', '_');

                    GroupPanel group = await db.GroupPanels.FirstOrDefaultAsync(p => p.Id == group_id);
                    var panels = db.Panels.Where(p => p.group_id == group_id);

                    int checkUnique = await db.Content.CountAsync(p => p.group_id == group_id && p.file_name == FileName);

                    if (checkUnique > 0) return BadRequest("Файл с таким именем уже загружен в эту группу");

                    if (FileName.Contains("mp4") || (FileName.Contains("jpeg")) || (FileName.Contains("jpg")) || (FileName.Contains("png")))
                    {
                        using (var fileStream = new FileStream(path + @"\Group_" + group.Id + @"\" + FileName, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }

                        foreach (Panel panel in panels)
                        {
                            Content content = new Content();
                            content.file_name = FileName;
                            content.file_size = (int)uploadedFile.Length;
                            content.sync = 0;
                            content.deleted = 0;
                            content.end_date = DateTime.Parse("2999-01-01");
                            content.user_id = user_id;
                            content.panel_id = panel.id;
                            content.type_content = type_content;
                            content.group_id = group_id;

                            db.Content.Add(content);
                        }
                        
                        await db.SaveChangesAsync();

                        return Ok("Файл успешно загружен");
                    }
                    else
                    {
                        return BadRequest("Недопустимый формат");
                    }
                }
                else
                    return BadRequest("Ошибка прикрепления файла");
            }
            catch (Exception e)
            {
                return BadRequest("Было вызвано исключение: " + e.Message);
            }
        }
        // получить контент загруженный в группу
        [Authorize]
        [HttpGet]
        [Route("GetContentInGroup")]
        public IActionResult GetContentInGroup(int? GroupId)
        {
            if (GroupId != null)
            {
                var result = db.Content
                    .Where(p => p.group_id == GroupId)
                    .Select(m => 
                        new { m.file_name, m.file_size, m.deleted, m.end_date, m.user_id, m.type_content })
                    .Distinct()
                    .ToList();

                return Json(result);
            }
            return BadRequest("GroupId is null");
        }
        // удалить файл из группы
        [Authorize]
        [HttpGet]
        [Route("DeleteFileGroup")]
        public async Task<IActionResult> DeleteFileGroup(int GroupId, string FileName)
        {
            var content = db.Content.Where(p => p.group_id == GroupId && p.file_name == FileName);

            foreach (Content content1 in content)
            {
                if (content1.sync == 1)
                {
                    content1.deleted = 1;
                    content1.sync = 2;
                    db.Content.Update(content1);
                }
                else
                {
                    db.Remove(content1);
                }
                
            }

            await db.SaveChangesAsync();

            return Json(content);
        }

        // удалить файл из группы
        [Authorize]
        [HttpGet]
        [Route("ChangeTimeDelete")]
        public async Task<IActionResult> ChangeTimeDelete(int GroupId, string FileName, DateTime newDate)
        {
            var content = db.Content.Where(p => p.group_id == GroupId && p.file_name == FileName);

            foreach (Content content1 in content)
            {
                content1.end_date = newDate;
                db.Content.Update(content1);
            }

            await db.SaveChangesAsync();

            return Json(content);
        }

        [Authorize]
        [HttpGet]
        [Route("GetStatusFileInGroup")]
        public JsonResult GetStatusFileInGroup(int GroupId, string FileName)
        {
            List<Content> content = db.Content.Where(p => p.group_id == GroupId && p.file_name == FileName).ToList();

            List<StatusFileInPanel> statusFileInPanel = new List<StatusFileInPanel>();

            foreach (Content content1 in content)
            {
                statusFileInPanel.Add(new StatusFileInPanel(db.Panels.FirstOrDefault(p => p.id == content1.panel_id).panel_name , content1.sync));
            }
            
            return Json(statusFileInPanel);
        }
    }
}
