using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

using ChatSrv;

namespace tests;

[TestClass]
public class UnitTestChat
{
    List<Message>? RecvMsgs(ClientWebSocket ws) {
        var recv = new Memory<byte>(new byte[1024 * 4]);
        var result = ws.ReceiveAsync(recv, CancellationToken.None).Result;
        var resp = Encoding.UTF8.GetString(recv.Slice(0, result.Count).ToArray());
        Console.WriteLine($"received length: {result.Count}, {resp}");
        var msgs = JsonConvert.DeserializeObject<List<Message>>(resp);
        return msgs;
    }

    [TestMethod]
    public void TestChatOkMultipleUser()
    {
        Uri uri = new Uri("ws://localhost:4649/chat");
        var client1 = new ClientWebSocket();
        client1.Options.SetRequestHeader("uname", "Alice");
        client1.ConnectAsync(uri, CancellationToken.None).Wait();
        Assert.AreEqual(WebSocketState.Open, client1.State);

        var msgs = RecvMsgs(client1);
        Assert.IsTrue(msgs?.Count > 0);

        var client2 = new ClientWebSocket();
        client2.Options.SetRequestHeader("uname", "Bob");
        client2.ConnectAsync(uri, CancellationToken.None).Wait();
        Assert.AreEqual(WebSocketState.Open, client2.State);
        msgs = RecvMsgs(client2);
        Assert.IsTrue(msgs?.Count > 0);

        var cmd = new ChatCommand() {
            s = new SendMessage("Hello, Bob")
        };
        var buf = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cmd));
        client1.SendAsync(buf, WebSocketMessageType.Text, false, CancellationToken.None).Wait();
        msgs = RecvMsgs(client2);
        Console.WriteLine($"TestChatOkMultipleUser: msgs.Count: {msgs?.Count}");
        // Assert.IsTrue(msgs?.Count > 1);

        client1.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
        client2.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
    }
    public void TestChatOkWithUNameHeader()
    {
        Uri uri = new Uri("ws://localhost:4649/chat");
        var client = new ClientWebSocket();
        client.Options.SetRequestHeader("uname", "Alice");
        client.ConnectAsync(uri, CancellationToken.None).Wait();
        Assert.AreEqual(WebSocketState.Open, client.State);

        var recv = new Memory<byte>(new byte[1024 * 4]);
        var result = client.ReceiveAsync(recv, CancellationToken.None).Result;
        var resp = Encoding.UTF8.GetString(recv.Slice(0, result.Count).ToArray());
        Console.WriteLine($"received length: {result.Count}, {resp}");
        var msgs = JsonConvert.DeserializeObject<List<Message>>(resp);
        Assert.IsTrue(msgs?.Count > 0);
    }

    [TestMethod]
    public void TestChatFailNoUNameHeader()
    {
        Uri uri = new Uri("ws://localhost:4649/chat");
        var client = new ClientWebSocket();
        // should fail with 401
        try {
            client.ConnectAsync(uri, CancellationToken.None).Wait();
            Assert.Fail("TestChatFailNoUserHeader: should fail with 401");
        }
        catch(AggregateException e) {
            Console.WriteLine($"status code: {client.HttpStatusCode}");
            foreach(var ie in e.InnerExceptions) {
                Console.WriteLine($"TestChatFailNoUserHeader: {ie.GetType()}");
                Console.WriteLine($"TestChatFailNoUserHeader: {ie.Message}");
                if(ie is WebSocketException) {
                    var wse = ie as WebSocketException;
                    Console.WriteLine($"TestChatFailNoUserHeader wse: {wse?.ErrorCode}");
                    Console.WriteLine($"TestChatFailNoUserHeader wse: {wse?.WebSocketErrorCode}");
                }
            }
            // inner exception should be WebSocketException, and should abort with 401
            Assert.AreEqual(typeof(WebSocketException), e.InnerException?.GetType());
            Assert.AreEqual("The server returned status code '401' when status code '101' was expected.", e.InnerException?.Message);
        }
    }
}
