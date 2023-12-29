using System.Net;
using System.Net.WebSockets;

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
    if(ctx.WebSockets.IsWebSocketRequest) {
        var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        // echo back
        while(ws.State == WebSocketState.Open) {
            var buffer = new byte[1024 * 4];
            var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if(result.MessageType == WebSocketMessageType.Close) {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, result.EndOfMessage, CancellationToken.None);
            }
        }
    }
    else {
        ctx.Response.StatusCode = 400;
    }
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
