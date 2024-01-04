using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Text;

namespace tests;

[TestClass]
public class UnitTestEcho
{
    // 測試 websocket server, url: /echo
    // 分為兩階段，先進行 websocket 連線
    // 再對成功的websocket連線進行寄送訊息測試
    [TestMethod]
    public void TestEcho()
    {
        Uri uri = new Uri("ws://localhost:4649/echo");
        var client = new ClientWebSocket();
        client.ConnectAsync(uri, CancellationToken.None).Wait();
        var msg = "hello";
        var send = Encoding.UTF8.GetBytes(msg);
        client.SendAsync(new ArraySegment<byte>(send), WebSocketMessageType.Text, true, CancellationToken.None).Wait();

        var recv = new Memory<byte>(new byte[1024 * 4]);
        var result = client.ReceiveAsync(recv, CancellationToken.None).Result;
        var resp = Encoding.UTF8.GetString(recv.Slice(0, result.Count).ToArray());
        Console.WriteLine($"received length: {result.Count}, {resp}");
        Assert.AreEqual(msg, resp);
    }
}
