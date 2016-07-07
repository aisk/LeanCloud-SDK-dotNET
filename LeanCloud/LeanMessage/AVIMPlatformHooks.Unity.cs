using System;
using LeanMessage.Unity.Internal;
using LeanMessage.Internal;

namespace LeanMessage
{
	partial class AVIMPlatformHooks: IAVIMPlatformHooks
	{
		private IWebSocketClient websocketClient = null;

		public string ua
		{
			get
			{
				return "unity/";
			}
		}

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
