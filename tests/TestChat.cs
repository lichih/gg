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
    [TestMethod]
    public void TestChatOk()
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
    public void TestChatFailNoUserHeader()
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
