using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Miracle.FileZilla.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using webapi.Models;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : Controller
    {
        private PlayersDBContext db;
        private PanelsDBContext db2;
        public UserController(PlayersDBContext context, PanelsDBContext context2)
        {
            db = context;
            db2 = context2;
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
        //[Authorize(Roles = "1")]
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
        public async Task<IActionResult> NewPassword(int? id, string newpassword, string oldpassword)
        {
            try
            {
                Models.User user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

                oldpassword = oldpassword.Trim();

                if (user.password.Equals(oldpassword))
                {
                    IFileZillaApi fileZillaApi = new FileZillaApi(IPAddress.Parse("127.0.0.1"), 14147);

                    fileZillaApi.Connect("kiselev1987tarja");

                    var accountSettings = fileZillaApi.GetAccountSettings();

                    var existingUser = accountSettings.Users.FirstOrDefault(x => x.UserName == user.user_name);

                    existingUser.AssignPassword(newpassword, fileZillaApi.ProtocolVersion);

                    fileZillaApi.SetAccountSettings(accountSettings);
                    fileZillaApi.Dispose();
                    user.password = newpassword.Trim();

                    db.Update(user);
                    await db.SaveChangesAsync();
                    return Ok("password change");
                }
                else
                {
                    return BadRequest("Неверный пароль");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // создать пользователя на ftp и бд
        [Authorize]
        [HttpPost]
        [Route("create_user")]
        public async Task<IActionResult> CreateUser([FromForm] string userName, [FromForm] string password,
                [FromForm] string surname, [FromForm] string name, [FromForm] string description, [FromForm] int adminId,
                [FromForm] string organization, [FromForm] string phone, [FromForm] string email,
                [FromForm] string town, [FromForm] string role = "2")
        {
            userName
                .Replace(" ", "")
                .Replace("@", "")
                .Replace("%", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("*", "")
                .Replace("+", "")
                .Replace("-", "");

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
            addUser.Role = role;
            addUser.working_folder = @"C:\clients\" + userName;
            addUser.name = name;
            addUser.surname = surname;
            addUser.description = description;
            addUser.deleted = false;
            addUser.locked = false;
            addUser.adminId = adminId;
            addUser.organization = organization;
            addUser.email = email;
            addUser.town = town;
            addUser.phone = phone;
            addUser.creation_date = DateTime.Now;

            db.Add(addUser);
            await db.SaveChangesAsync();

            return Ok("Пользователь добавлен");
        }

        [Authorize]
        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers(int? adminId)
        {
            var Users = db.Users.Where(u => u.adminId == adminId);
            foreach (Models.User user in Users)
            {
                user.CountPanel = db2.Panels.Count(p => p.user_id == user.Id);
            }
            return Json(Users);
        }
        [Authorize]
        [HttpPost]
        [Route("change_user")]
        public async Task<IActionResult> ChangeUser(Models.User user)
        {
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

            if (u == null) return BadRequest("not found");

            u.locked = user.locked;
            u.deleted = user.deleted;
            u.description = user.description;
            u.organization = user.organization;
            u.email = user.email;
            u.phone = user.phone;
            u.town = user.town;

            db.Update(u);
            await db.SaveChangesAsync();

            return Json(u);
        }

        [Authorize]
        [HttpPost]
        [Route("delete_user")]
        public async Task<IActionResult> DeleteUser([FromForm] string userName)
        {
            Models.User user = await db.Users.FirstOrDefaultAsync(p => p.user_name == userName);
            if (user == null && user.user_name != "Petr") return BadRequest("Пользователь не найден");

            DirectoryInfo dir = new DirectoryInfo(@"C:\clients\" + userName);
            dir.Delete(true);

            db.Remove(user);
            await db.SaveChangesAsync();

            using (IFileZillaApi fileZillaApi = new FileZillaApi(IPAddress.Parse("127.0.0.1"), 14147))
            {
                fileZillaApi.Connect("kiselev1987tarja");
                var serverState = fileZillaApi.GetServerState();

                var accountSettings = fileZillaApi.GetAccountSettings();
                
                accountSettings.Users.RemoveAll(x => x.UserName == userName);

                fileZillaApi.SetAccountSettings(accountSettings);
            }
            return Ok("Пользователь удален");
        }
        [Authorize]
        [HttpGet]
        [Route("GetNewPassword")]
        public async Task<IActionResult> GetNewPassword(string userName) 
        {
            Models.User user = await db.Users.FirstOrDefaultAsync(p => p.user_name == userName);
            if (user != null)
            {
                return Json(user);
            }
            else return BadRequest("not found");
        }

        [Authorize]
        [HttpGet]
        [Route("ChangeMyData")]
        public async Task<IActionResult> ChangeMyData(int UserId,  string Name, string Surname, string Phone, 
                    string Email, string Town, string org)
        {
            Models.User user = await db.Users.FirstOrDefaultAsync(p => p.Id == UserId);

            user.surname = Surname;
            user.name = Name;
            user.phone = Phone;
            user.email = Email;
            user.town = Town;
            user.organization = org;

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyData")]
        public async Task<IActionResult> GetMyData(int UserId)
        {
            Models.User user = await db.Users.FirstOrDefaultAsync(p => p.Id == UserId);

            return Json(user);
        }
    }
}
