using System;
using LeanMessage.Internal;
using LeanCloud;

namespace LeanMessage.Unity.Internal
{
	internal class WebSocketClient: IWebSocketClient
	{
		
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
				return false;
			}
		}

		public void Close()
		{
		}

		public void Open(string url, string protocol = null)
		{
		}
			

		public void Send(string message)
		{
		}
	}
}

