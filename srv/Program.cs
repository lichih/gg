using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:4649");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseWebSockets();
// app.Use(async (conext, next) => {
//     if(conext.WebSockets.IsWebSocketRequest){
//         using var ws = await conext.WebSockets.AcceptWebSocketAsync();
//         // echo back anything we receive
//         while(true){
//             var buffer = new byte[1024 * 4];
//             var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
//             if(result.MessageType == WebSocketMessageType.Close){
//                 await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
//                 break;
//             }
//             await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
//         }
//     }else{
//         conext.Response.StatusCode = 400;
//     }
// });

await app.RunAsync();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
// app.UseHttpsRedirection();
// app.UseAuthorization();
// app.MapControllers();
// app.Run();
