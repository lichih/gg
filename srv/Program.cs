using System;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using hinata;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4649");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseWebSockets();
app.Map("/echo", async ctx => {
    if(!ctx.WebSockets.IsWebSocketRequest) {
        ctx.Response.StatusCode = 400;
        return;
    }
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    BaseWebSocketSrv echo = new EchoSrv();
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
            string resp = echo.OnMessage(recv, msg);
            var bufResp = Encoding.UTF8.GetBytes(resp);
            await ws.SendAsync(bufResp, WebSocketMessageType.Text, true, CancellationToken.None);
            
        }
    }
    Console.WriteLine("EchoSrv.OnClose");
});
app.Map("/echo/yaml", async ctx => {
    if(!ctx.WebSockets.IsWebSocketRequest) {
        ctx.Response.StatusCode = 400;
        return;
    }
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    BaseWebSocketSrv echo = new EchoYamlSrv();
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
            var resp = echo.OnMessage(recv, msg);
            var bufResp = Encoding.UTF8.GetBytes(resp);
            await ws.SendAsync(bufResp, WebSocketMessageType.Text, true, CancellationToken.None);
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
