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
                    db.SaveChanges();

                    return Ok();
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
    }
}
