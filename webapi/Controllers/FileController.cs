using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Controllers
{
    [ApiController]
    [Route("file")]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _appEnvironment;
        public FileController(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        [Authorize]
        [Route("GetPlayer")]
        public IActionResult Download(int id)
        {
            string file_path;
            string file_name;

            if (id == 1)
            {
                file_path = Path.Combine(_appEnvironment.ContentRootPath, @"C:\api\Downloads\standart.7z");
                file_name = "standart.7z";
            }
            else
            if (id == 2)
            {
                file_path = Path.Combine(_appEnvironment.ContentRootPath, @"C:\api\Downloads\player_bus.7z");
                file_name = "player_bus.7z";
            }
            else 
            if (id == 3)
            {
                file_path = Path.Combine(_appEnvironment.ContentRootPath, @"C:\api\Downloads\Настройка Windows.7z");
                file_name = "Manual.7z";
            }
            else  return null;

            string file_type = "application/octet-stream";
            
            return PhysicalFile(file_path, file_type, file_name);
        }
    }
}
