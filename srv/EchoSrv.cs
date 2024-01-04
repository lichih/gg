﻿using System;
using System.Net;
using System.Net.WebSockets;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace hinata;

[Serializable]
public record Message(string type, string data);

public record MyMessage
{
    public string sender = "";
    public string line = "";
}

public abstract class BaseWebSocketSrv
{
    public abstract void OnOpen(WebSocket ws);
    public abstract string OnMessage(WebSocketReceiveResult result, string msg);
}
public class EchoYamlSrv : BaseWebSocketSrv
{
    public override void OnOpen(WebSocket ws)
    {
        Console.WriteLine("EchoYamlSrv.OnOpen");
    }
    public override string OnMessage(WebSocketReceiveResult result, string msg)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        try {
            var data = deserializer.Deserialize<List<MyMessage>>(msg);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            return json;
        }
        catch (Exception ex) {
            return $"failed to parse as YAML: [{msg}][{msg.Length}] ex: {ex.Message}";
        }
    }
}

public class EchoSrv : BaseWebSocketSrv
{
    public override void OnOpen(WebSocket ws)
    {
        Console.WriteLine("EchoSrv.OnOpen");
    }
    public override string OnMessage(WebSocketReceiveResult result, string msg)
    {
        var data = new Message("echo", msg);
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        return json;
    }
}
