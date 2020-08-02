using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Miracle.FileZilla.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using webapi.Models;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : Controller
    {
        private PlayersDBContext db;
        public UserController(PlayersDBContext context)
        {
            db = context;
        }
        [Authorize]
        [Route("getuser")]
        public async Task<IActionResult> GetUser(int? id)
        {
            if (id != null)
            {
                Models.User person = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                return Json(person);
            }
            return BadRequest("Пользователь не найден");
        }
        // список пользователей
        [Authorize(Roles = "1")]
        [Route("GetUserList")]
        public async Task<IActionResult> GetUserList()
        {
            List<Models.User> list = await db.Users.ToListAsync();
            return Json(list);
        }

        [Authorize]
        [Route("getlogin")]
        public IActionResult GetLogin()
        {
            return Ok($"Ваш логин: {User.Identity.Name}");
        }

        [Authorize]
        [HttpGet]
        [Route("getrole")]
        public async Task<IActionResult> GetRole(string username, string password)
        {
            Models.User user =  await db.Users.FirstOrDefaultAsync(x => x.user_name == username && x.password == password);
            // пишем дату авторизации
            if (user != null)
            {
                user.AuthTime = DateTime.Now.ToString("g");
                db.Update(user);
                await db.SaveChangesAsync();
            }
            
            return Json(user);
        }
        // смена пароля
        [Authorize]
        [HttpGet]
        [Route("newpass")]
        public async Task<IActionResult> NewPassword(int? id, string newpassword)
        {
            if (id != null) 
            {
                Models.User user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
                user.password = newpassword;
                db.Update(user);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(id);
        }

        // создать пользователя на ftp и бд
        [Authorize]
        [HttpPost]
        [Route("create_user")]
        public async Task<IActionResult> CreateUser([FromForm] string userName, [FromForm] string password, 
                [FromForm] string surname, [FromForm] string name, [FromForm] string desctiption, [FromForm] int adminId)
        {
            Models.User newUser = await db.Users.FirstOrDefaultAsync(p => p.user_name == userName);

            if (newUser != null) return BadRequest("this username exist");

            DirectoryInfo dir = new DirectoryInfo(@"C:\clients\" + userName);
            dir.Create();
            
            using (IFileZillaApi fileZillaApi = new FileZillaApi(IPAddress.Parse("127.0.0.1"), 14147))
            {
                fileZillaApi.Connect("kiselev1987tarja");
                var accountSettings = fileZillaApi.GetAccountSettings();
                var user = new Miracle.FileZilla.Api.User
                {
                    UserName = userName,
                    SharedFolders = new List<SharedFolder>()
                    {
                    new SharedFolder()
                        {
                            Directory = @"C:\clients\" + userName,
                            AccessRights = AccessRights.DirList | AccessRights.DirSubdirs | AccessRights.FileRead |
                                AccessRights.FileWrite | AccessRights.IsHome | AccessRights.DirDelete | AccessRights.FileDelete |
                                AccessRights.FileAppend | AccessRights.DirCreate
                        }
                    }
                };
                user.AssignPassword(password, fileZillaApi.ProtocolVersion);
                accountSettings.Users.Add(user); 
                fileZillaApi.SetAccountSettings(accountSettings);
            }

            Models.User addUser = new Models.User();
            addUser.user_name = userName;
            addUser.password = password;
            addUser.Role = "2";
            addUser.working_folder = @"C:\clients\" + userName;
            addUser.name = name;
            addUser.surname = surname;
            addUser.desctiption = desctiption;
            addUser.deleted = false;
            addUser.locked = false;
            addUser.adminId = adminId;

            db.Add(addUser);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}
