using Microsoft.AspNetCore.Mvc;
using wssrv;
namespace wssrv.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;

    public MessageController(ILogger<MessageController> logger)
    {
        _logger = logger;
    }
    public static ChatRoom chatroom = new ChatRoom();

    [HttpGet(Name = "GetMessages")]
    public IEnumerable<Message> GetMessages()
    {
        return chatroom.GetAllMessages();
    }

    [HttpPost(Name = "PostMessage")]
    public void PostMessage(string sender, string content)
    {
        chatroom.SendMessage(sender, content);
    }
}

