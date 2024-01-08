using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace ChatSrv
{
    public record Message(string sender, string content);
    public record SendMessage(string c);
    public record GetMessage();

    public record ChatCommand {
        // public string command = ""; // SendMessage, GetMessage
        public SendMessage? s = null;
        public GetMessage? g = null;
    }
    public record ChatRoom {
        public List<WebSocket> clients = new List<WebSocket>();
        public List<Message> messages = new List<Message>();
        public void SendMessage(string sender, string content) {
            messages.Add(new Message(sender, content));
        }
        public IEnumerable<Message> GetAllMessages() {
            return messages;
        }
        public IEnumerable<Message> GetMessages(string? exc=null) {
            return messages.Where(m => m.sender != exc);
        }
    }
}
