using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    }
}
