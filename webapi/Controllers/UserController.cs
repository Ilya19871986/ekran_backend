using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                User person = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                return Json(person);
            }
            return BadRequest("Пользователь не найден");
        }
        // список пользователей
        [Authorize(Roles = "1")]
        [Route("GetUserList")]
        public async Task<IActionResult> GetUserList()
        {
            List<User> list = await db.Users.ToListAsync();
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
            User user =  await db.Users.FirstOrDefaultAsync(x => x.user_name == username && x.password == password);
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
                User user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
                user.password = newpassword;
                db.Update(user);
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(id);
        }
       
    }
}
