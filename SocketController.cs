using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modding;

namespace HollowKnightMod
{
    public class SocketController
    {
        private readonly SemaphoreSlim _wsLock = new SemaphoreSlim(1, 1);
        private ClientWebSocket _webSocket;
        private readonly Dictionary<string, List<Pattern>> DIY_patterns;

        public SocketController()
        {
            DIY_patterns = LoadPatternsFromFile("config.json");
        }

        private Dictionary<string, List<Pattern>> LoadPatternsFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Modding.Logger.LogError($"配置文件 {filePath} 不存在");
                    return new Dictionary<string, List<Pattern>>();
                }

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, List<Pattern>>>(json);
            }
            catch (Exception ex)
            {
                Modding.Logger.LogError($"加载模式失败: {ex.Message}");
                return new Dictionary<string, List<Pattern>>();
            }
        }

        public async Task SendPatternSafe(string patternName, int intensity, int ticks)
        {
            try
            {
                await SendPattern(patternName, intensity, ticks);
            }
            catch (Exception ex)
            {
                Modding.Logger.LogError($"发送模式失败: {ex.Message}");
            }
        }

        private async Task SendPattern(string patternName, int intensity, int ticks)
        {
            if (!IsWebSocketReady) return;

            var message = CreateMessage(patternName, intensity, ticks);
            await SendMessage(message);
        }

        private string CreateMessage(string patternName, int intensity, int ticks)
        {
            if (DIY_patterns.TryGetValue(patternName, out var patterns))
            {
                return JsonConvert.SerializeObject(new
                {
                    cmd = "set_pattern",
                    pattern_units = patterns,
                    intensity,
                    ticks
                });
            }

            return JsonConvert.SerializeObject(new
            {
                cmd = "set_pattern",
                pattern_name = patternName,
                intensity,
                ticks
            });
        }

        private async Task SendMessage(string message)
        {
            try
            {
                await _wsLock.WaitAsync();

                if (_webSocket?.State != WebSocketState.Open)
                    return;

                var buffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                Modding.Logger.LogError($"发送消息失败: {ex.Message}");
                throw;
            }
            finally
            {
                _wsLock.Release();
            }
        }

        public async Task ReceiveMessagesAsync(CancellationToken ct)
        {
            byte[] buffer = new byte[1024];
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", ct);
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Modding.Logger.Log($"收到消息: {message}");
                }
                catch (Exception e)
                {
                    Modding.Logger.Log($"WebSocket 监听错误: {e.Message}");
                    break;
                }
            }
        }

        public void SetWebSocket(ClientWebSocket ws)
        {
            _webSocket = ws;
        }

        public void ClearWebSocket()
        {
            _webSocket = null;
        }

        private bool IsWebSocketReady => _webSocket?.State == WebSocketState.Open;
    }
}
