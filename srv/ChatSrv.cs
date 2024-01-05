namespace ChatSrv
{
    public record Message(string sender, string content);
    public record Login(string name);
    public record SendMessage(string s, string c);
    public record GetMessage(string exc);

    public record ChatCommand {
        // public string command = ""; // SendMessage, GetMessage
        public Login? l = null;
        public SendMessage? s = null;
        public GetMessage? g = null;
    }
    public record ChatRoom {
        public List<Message> messages = new List<Message>();
        public void SendMessage(string sender, string content) {
            messages.Add(new Message(sender, content));
        }
        public IEnumerable<Message> GetAllMessages() {
            return messages;
        }
        public IEnumerable<Message> GetMessages(string exc) {
            return messages.Where(m => m.sender != exc);
        }
    }
}
