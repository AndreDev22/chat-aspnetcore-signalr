using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChatApp.Models;
using ChatApp.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ChatApp.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        private AppDbContext context;

        public HomeController(AppDbContext _context) => context = _context;

        public IActionResult Index()
        {
            var chats = context.Chats
            .Include(x => x.Users)
            .Where(x => !x.Users
                .Any(y => y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value))
            .ToList();
            return View(chats);
        }

        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = context.Chats
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Id == id);
            return View(chat);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int chatId, string message)
        {
            var Message = new Message
            {
                ChatId = chatId,
                Text = message,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };

            context.Messages.Add(Message);

            await context.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = chatId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room
            };

            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Admin
            });

            context.Chats.Add(chat);

            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Find()
        {
            var users = context.Users.Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value).ToList();

            return View(users);
        }

        public IActionResult Private()
        {
            var chats = context.Chats
            .Include(x => x.Users)
            .ThenInclude(x => x.User)
            .Where(x => x.Type == ChatType.Private && x.Users
            .Any(y => y.UserId == User
            .FindFirst(ClaimTypes.NameIdentifier).Value))
            .ToList();

            return View(chats);
        }

        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };

            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });

            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value
            });

            context.Chats.Add(chat);

            await context.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = chat.Id });
        }

        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {
            var chatUser = new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Member
            };

            context.ChatUsers.Add(chatUser);
            try
            {
                await context.SaveChangesAsync();
            }
            catch { }
            return RedirectToAction("Chat", "Home", new { id = id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
