using System;
using System.Collections.Generic;
namespace wssrv;

public class ChatUser
{
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public List<string> Lines { get; set; } = new();

}

public class Message
{
    public string? Sender { get; set; }
    public string? Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChatRoom
{
    public List<Message> messages;

    public ChatRoom()
    {
        Console.WriteLine("ChatRoom created");
        messages = new List<Message>();
    }

    public void SendMessage(string sender, string content)
    {
        var message = new Message
        {
            Sender = sender,
            Content = content,
            Timestamp = DateTime.Now
        };

        messages.Add(message);
    }

    public List<Message> GetAllMessages()
    {
        return messages;
    }

    public Message? GetLatestMessage()
    {
        if (messages.Count > 0)
        {
            return messages[messages.Count - 1];
        }

        return null;
    }
}
