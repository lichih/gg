using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace hinata;

[Serializable]
public record Message(string type, string data);

public class EchoYamlSrv : WebSocketBehavior
{
    protected override void OnOpen()
    {
        var msg = new Message("N", $"opened connection: {ID}, convering YAML to JSON");
        Send(JsonConvert.SerializeObject(msg));
        // Send(msg);
    }
    protected override void OnMessage(MessageEventArgs e)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        try {
            var data = deserializer.Deserialize<Dictionary<string, string>>(e.Data);
            var msg = $"pasrsed as YAML: [{e.Data}][{e.Data.Length}]";
            Send(msg);
            var json = JsonConvert.SerializeObject(data);
            msg = $"converted to JSON({json.Length}): {json}";
            Send(msg);
        }
        catch (Exception ex) {
            var msg = $"failed to parse as YAML: [{e.Data}][{e.Data.Length}] ex: {ex.Message}";
            Send(msg);
        }
    }
}

public class EchoSrv : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        var msg = $"received data: [{e.Data}][{e.Data.Length}]";
        Send(msg);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var wssv = new WebSocketServer("ws://localhost:4649");
        wssv.AddWebSocketService<EchoSrv>("/echo");
        wssv.AddWebSocketService<EchoYamlSrv>("/echo/yaml");
        wssv.Start();
        Console.WriteLine("Hinata is listening on port 4649, and providing WebSocket services:");
        Console.ReadKey(true);
        wssv.Stop();
    }    
}
