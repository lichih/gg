using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

using ChatSrv;

namespace tests;

[TestClass]
public class UnitTestEcho
{
    [TestMethod]
    public void TestEcho()
    {
        Uri uri = new Uri("ws://localhost:4649/echo");
        var client = new ClientWebSocket();
        client.ConnectAsync(uri, CancellationToken.None).Wait();
        Assert.AreEqual(WebSocketState.Open, client.State);

        var buf = Encoding.UTF8.GetBytes("Hello, World!");
        client.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        var recv = new Memory<byte>(new byte[1024 * 4]);
        var result = client.ReceiveAsync(recv, CancellationToken.None).Result;
        var resp = Encoding.UTF8.GetString(recv.Slice(0, result.Count).ToArray());
        Console.WriteLine($"received length: {result.Count}, {resp}");
        Assert.AreEqual("Hello, World!", resp);

        client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
    }
}
