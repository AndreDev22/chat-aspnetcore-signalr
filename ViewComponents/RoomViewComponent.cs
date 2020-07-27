using System.Linq;
using System.Security.Claims;
using ChatApp.Database;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.ViewComponents
{
    public class RoomViewComponent : ViewComponent
    {
        private AppDbContext context;

        public RoomViewComponent(AppDbContext _context)
        {
            context = _context;
        }
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = context.ChatUsers
                .Include(x => x.Chat)
                .Where(x => x.UserId == userId && x.Chat.Type == ChatType.Room)
                .Select(x => x.Chat)
                .ToList();

            return View(chats);
        }
    }
}