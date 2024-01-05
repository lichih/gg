using System;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;

using hinata;
using ChatSrv;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4649");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseWebSockets();

// var cmd_sample = new ChatCommand {
//     s = new SendMessage("Alice", "Hello"),
//     g = new GetMessage("Alice"),
// };
var cmd_sample = new ChatCommand {
    l = new Login("Alice"),
    s = new SendMessage("Alice", "Hello"),
    g = new GetMessage("Bob"),
};
var s = JsonConvert.SerializeObject(cmd_sample);
Console.WriteLine($"ChatSrv.OnOpen: sample:\n{s}");
// var serilizer = new SerializerBuilder()
//     .WithNamingConvention(CamelCaseNamingConvention.Instance)
//     .WithIndentedSequences()
//     .Build();
// string cmd_sample_yaml = serilizer.Serialize(cmd_sample);
// Console.WriteLine($"ChatSrv.OnOpen: sample:\n{cmd_sample_yaml}");

// var deserializer = new DeserializerBuilder()
//     .WithNamingConvention(CamelCaseNamingConvention.Instance)
//     .Build();
// var cmd = deserializer.Deserialize<ChatCommand>(cmd_sample_yaml);
// Console.WriteLine($"ChatSrv.OnOpen: sample:\n{cmd}");

ChatRoom room = new ChatRoom();

app.Map("/chat", async ctx => {
    if(!ctx.WebSockets.IsWebSocketRequest) {
        ctx.Response.StatusCode = 400;
        return;
    }

    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    var bufRecv = new byte[1024 * 4];
    string uname = "";
    while(ws.State == WebSocketState.Open) {
        var recv = await ws.ReceiveAsync(bufRecv, CancellationToken.None);
        if(recv.MessageType == WebSocketMessageType.Close) {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            break;
        }
        else if(recv.MessageType == WebSocketMessageType.Text) {
            var msg = Encoding.UTF8.GetString(bufRecv, 0, recv.Count);
            try {
                Console.WriteLine($"ChatSrv.OnMessage: Got\n{msg}");
                // cmd = deserializer.Deserialize<ChatCommand>(msg);
                // Console.WriteLine($"ChatSrv.OnMessage: Parsed:\n{cmd}");
                var cmd = JsonConvert.DeserializeObject<ChatCommand>(msg);
                Console.WriteLine($"ChatSrv.OnMessage: Parsed:\n{cmd}");
                if(cmd == null) {
                    Console.WriteLine($"ChatSrv.OnMessage: cmd is null");
                    continue;
                }
                if(cmd.l != null) {
                    Console.WriteLine($"ChatSrv.OnMessage: uname={uname}");
                    room.SendMessage("System", $"{cmd.l.name} joined");
                    Console.WriteLine($"ChatSrv.OnMessage: {cmd.l.name} joined");
                }
                if(cmd.s != null) {
                    if(uname == "") {
                        continue;
                    }
                    room.SendMessage(uname, cmd.s.c);
                }
                if(cmd.g != null) {
                    if(uname == "") {
                        continue;
                    }
                    var msgs = room.GetMessages(uname);
                    var resp = JsonConvert.SerializeObject(msgs);
                    var bufResp = Encoding.UTF8.GetBytes(resp);
                    await ws.SendAsync(bufResp, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch(Exception e) {
                Console.WriteLine($"ChatSrv.OnMessage Exception: {e}");
            }
        }
    }
    bufRecv = null;
    Console.WriteLine("EchoSrv.OnClose");
});

app.Map("/echo", async ctx => {
    if(!ctx.WebSockets.IsWebSocketRequest) {
        ctx.Response.StatusCode = 400;
        return;
    }
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    BaseWebSocketSrv echo = new EchoSrv(ws);
    echo.OnOpen(ws);
    var bufRecv = new byte[1024 * 4];
    while(ws.State == WebSocketState.Open) {
        var recv = await ws.ReceiveAsync(bufRecv, CancellationToken.None);
        if(recv.MessageType == WebSocketMessageType.Close) {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            break;
        }
        else if(recv.MessageType == WebSocketMessageType.Text) {
            var msg = Encoding.UTF8.GetString(bufRecv, 0, recv.Count);
            echo.OnMessage(recv, msg);
            // string resp = echo.OnMessage(recv, msg);
            // var bufResp = Encoding.UTF8.GetBytes(resp);
            // await ws.SendAsync(bufResp, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    bufRecv = null;
    Console.WriteLine("EchoSrv.OnClose");
});
app.Map("/echo/yaml", async ctx => {
    if(!ctx.WebSockets.IsWebSocketRequest) {
        ctx.Response.StatusCode = 400;
        return;
    }
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    BaseWebSocketSrv echo = new EchoYamlSrv(ws);
    echo.OnOpen(ws);
    while(ws.State == WebSocketState.Open) {
        var bufRecv = new byte[1024 * 4];
        var recv = await ws.ReceiveAsync(bufRecv, CancellationToken.None);
        if(recv.MessageType == WebSocketMessageType.Close) {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            break;
        }
        else if(recv.MessageType == WebSocketMessageType.Text) {
            var msg = Encoding.UTF8.GetString(bufRecv, 0, recv.Count);
            echo.OnMessage(recv, msg);
            // var resp = echo.OnMessage(recv, msg);
            // var bufResp = Encoding.UTF8.GetBytes(resp);
            // await ws.SendAsync(bufResp, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    Console.WriteLine("EchoYamlSrv.OnClose");
});
Console.WriteLine("Hinata is listening on port 4649, and providing WebSocket services:");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
// app.Run();

await app.RunAsync();
