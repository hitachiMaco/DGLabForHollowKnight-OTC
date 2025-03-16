using Modding;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HollowKnightMod
{
    public class DGLabForHollowKnight : Mod
    {
        private readonly SemaphoreSlim _wsLock = new SemaphoreSlim(1, 1);
        private ClientWebSocket _webSocket;
        private readonly Uri _serverUri = new Uri("ws://192.168.1.3:60536/1");  //改为手机OTC控制器上写的ip
        private readonly SocketController _controller;
        private CancellationTokenSource _cts;

        public DGLabForHollowKnight()
        {
            _controller = new SocketController();
            _cts = new CancellationTokenSource();
        }

        public override string GetVersion() => "0.1a";

        public override void Initialize()
        {
            Log("Harry's Mod Load Successfully!");
            ModHooks.AfterTakeDamageHook += AfterTakeDamage;    //受击
            ModHooks.BeforePlayerDeadHook += BeforePlayerDead;  //死亡
            ModHooks.SlashHitHook += OnSlashHit;                //攻击
            _ = ConnectWebSocketAsync();
        }

        private async Task ConnectWebSocketAsync()
        {
            lock (_wsLock)
            {
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
            }
            try
            {
                Log($"尝试连接至 {_serverUri}");
                await _webSocket.ConnectAsync(_serverUri, _cts.Token);
                Log("WebSocket 连接成功");
                _controller.SetWebSocket(_webSocket);
                _ = Task.Run(() => _controller.ReceiveMessagesAsync(_cts.Token));
            }
            catch (Exception e)
            {
                Log($"WebSocket 连接失败: {e.Message}");
                CleanupWebSocket();
                throw;
            }
        }

        private void CleanupWebSocket()
        {
            lock (_wsLock)
            {
                _webSocket?.Dispose();
                _webSocket = null;
                _controller.ClearWebSocket();
            }
        }

        public int AfterTakeDamage(int hazardType, int damageAmount)
        {
            Log($"玩家受到了 {damageAmount} 点伤害");
            if (IsWebSocketReady)
            {
                if (PlayerData.instance.health == 0)
                {
                    return damageAmount;
                }
                else if (damageAmount == 1)
                {
                    Task.Run(() => _controller.SendPatternSafe("example", 75, 10));  //（模式名，百分比强度，持续时间）
                }                                                                    // 10 = 1s
                else if (damageAmount >= 2)
                {
                    Task.Run(() => _controller.SendPatternSafe("example", 85, 16));
                }
            }
            return damageAmount;
        }

        public void BeforePlayerDead()
        {
            Log("玩家死亡");
            if (IsWebSocketReady)
            {
                Task.Run(() => _controller.SendPatternSafe("example", 100, 50));
            }
        }

        public void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {
            if (IsWebSocketReady)
            {
                Task.Run(() => _controller.SendPatternSafe("example", 50, 10));
            }
        }

        private bool IsWebSocketReady =>
            _webSocket?.State == WebSocketState.Open &&
            !_cts.IsCancellationRequested;
    }
}
