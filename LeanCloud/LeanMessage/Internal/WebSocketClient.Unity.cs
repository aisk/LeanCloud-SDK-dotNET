using System;
using LeanMessage.Internal;
using WebSocketSharp;
using LeanCloud;

namespace LeanMessage.Unity.Internal
{
	internal class WebSocketClient: IWebSocketClient
	{
		WebSocket ws;
		public WebSocketClient()
		{

		}

		public event Action OnClosed;
		public event Action<string> OnError;
		public event Action<string> OnLog;

		private Action m_OnOpened;
		public event Action OnOpened
		{
			add
			{
				m_OnOpened += value;
			}
			remove
			{
				m_OnOpened -= value;
			}
		}
		private Action<string> m_OnMessage;
		public event Action<string> OnMessage
		{
			add
			{
				m_OnMessage+= value;
			}
			remove
			{
				m_OnMessage -= value;
			}
		}

		public bool IsOpen
		{
			get
			{
				return ws.IsAlive;
			}
		}

		public void Close()
		{
			ws?.Close ();
		}

		public void Open(string url, string protocol = null)
		{
			ws = new WebSocket (url, protocol);
			ws.Connect ();
			ws.OnOpen += Ws_OnOpen;
			ws.OnMessage += Ws_OnMessage;
			ws.OnError += Ws_OnError;
			ws.OnClose += Ws_OnClose;
		}

		void Ws_OnClose (object sender, CloseEventArgs e)
		{
			if (AVClient.enabledLog) {
				AVClient.LogTracker (e.Reason);
			}
		}

		void Ws_OnError (object sender, ErrorEventArgs e)
		{
			if (AVClient.enabledLog) {
				AVClient.LogTracker (e.Message);
			}
		}

		void Ws_OnOpen (object sender, EventArgs e)
		{
			if (m_OnOpened != null) {
				m_OnOpened ();
			}
		}

		void Ws_OnMessage (object sender, MessageEventArgs e)
		{
			if (m_OnMessage != null) {
				m_OnMessage (e.Data);
			}
		}

		public void Send(string message)
		{
			if (ws.IsAlive) {
				ws.Send (message);
			}
		}
	}
}

