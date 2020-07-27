using System;
using System.Threading.Tasks;
using ChatApp.Database;
using ChatApp.Hubs;
using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<ChatHub> chat;

        public ChatController(IHubContext<ChatHub> _chat)
        {
            chat = _chat;
        }

        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> JoinRoom(string connectionId, string roomId)
        {
            await chat.Groups.AddToGroupAsync(connectionId, roomId);
            return Ok();
        }

        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomName)
        {
            await chat.Groups.RemoveFromGroupAsync(connectionId, roomName);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(string message, int roomId, [FromServices] AppDbContext context)
        {

            var Message = new Message
            {
                ChatId = roomId,
                Text = message,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };

            context.Messages.Add(Message);

            await context.SaveChangesAsync();


            await chat.Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", new
            {
                Text = Message.Text,
                Name = Message.Name,
                Timestamp = Message.Timestamp.ToString("dd/MM/yyyy hh:mm:ss")
            });

            return Ok();
        }
    }
}