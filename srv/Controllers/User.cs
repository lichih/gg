using Microsoft.AspNetCore.Mvc;
using wssrv;
namespace wssrv.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpGet(Name = "GetUsers")]
    public IEnumerable<ChatUser> Get()
    {
        return MessageController.chatroom.messages.GroupBy(m => m.Sender).Select(g => new ChatUser
        {
            Name = g.Key,
            UserId = Guid.NewGuid().ToString(),
            Lines = g.Select(m => m.Content).ToList(),
        }).ToArray();
        // return Enumerable.Range(1, 5).Select(index => new ChatUser
        // {
        //     Name = $"User {index}",
        //     UserId = Guid.NewGuid().ToString(),
        //     Lines = new List<string> { "Hello", "World" }
        // })
        // .ToArray();
    }
}
