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
    protected WebSocket ws;
    public BaseWebSocketSrv(WebSocket ws) => this.ws = ws;
    public abstract void OnOpen(WebSocket ws);
    public abstract void OnMessage(WebSocketReceiveResult result, string msg);
    protected void Send(string msg)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(msg);
        ws.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

public class EchoSrv : BaseWebSocketSrv
{
    public EchoSrv(WebSocket ws) : base(ws) {}
    public override void OnOpen(WebSocket ws)
    {
        Console.WriteLine("EchoSrv.OnOpen");
    }
    public override void OnMessage(WebSocketReceiveResult result, string msg)
    {
        var resp = new string(msg);
        Console.WriteLine($"EchoSrv.OnMessage: {resp}");
        Send(resp);
        // return resp;
        // var data = new Message("echo", msg);
        // var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        // return json;
    }
}

public class EchoYamlSrv : BaseWebSocketSrv
{
    public EchoYamlSrv(WebSocket ws) : base(ws) {}
    public override void OnOpen(WebSocket ws)
    {
        Console.WriteLine("EchoYamlSrv.OnOpen");
    }
    public override void OnMessage(WebSocketReceiveResult result, string msg)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        try {
            var data = deserializer.Deserialize<List<MyMessage>>(msg);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            // return json;
            Send(json);
        }
        catch (Exception ex) {
            Console.WriteLine($"failed to parse as YAML: [{msg}][{msg.Length}] ex: {ex.Message}");
        }
    }
}

