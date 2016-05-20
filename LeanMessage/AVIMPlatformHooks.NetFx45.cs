﻿using LeanMessage.Internal;
using LeanMessage.NetFx45.Internal;

namespace LeanMessage
{
    partial class AVIMPlatformHooks: IAVIMPlatformHooks
    {
        private IWebSocketClient websocketClient = null;
        public IWebSocketClient WebSocketClient
        {
            get
            {
                websocketClient = websocketClient ?? new WebSocketClient();
                return websocketClient;
            }
        }
    }
}
