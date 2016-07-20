using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanCloud.Internal;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Net;

namespace LeanCloud
{
	interface IToDictionary
	{
		IDictionary<string, object> ToDictionary();
	}
	internal class AVAnalyticActivity : IToDictionary
	{
		public long du { get; set; }

		public string name { get; set; }

		public long ts { get; set; }

		internal AVAnalyticActivity()
		{

		}

		public IDictionary<string, object> ToDictionary()
		{
			IDictionary<string, object> rtn = new Dictionary<string, object>();

			rtn["du"] = du;
			rtn["name"] = name;
			rtn["ts"] = ts;

			return rtn;
		}
	}
	internal class AVAnalyticEvent : IToDictionary
	{
		public long du { get; set; }

		public string name { get; set; }

		public string sessionId { get; set; }

		public string tag { get; set; }

		public long ts { get; set; }

		public bool stop { get; set; }

		public IDictionary<string, string> attributes { get; set; }


		internal AVAnalyticEvent()
		{

		}
		public IDictionary<string, object> ToDictionary()
		{
			IDictionary<string, object> rtn = new Dictionary<string, object>();

			rtn["du"] = du;
			rtn["name"] = name;
			rtn["sessionId"] = sessionId;
			rtn["tag"] = tag;
			rtn["ts"] = ts;
			if(attributes != null)
				rtn ["attributes"] = attributes;
			return rtn;
		}
	}
	/// <summary>
	/// Provides an interface to AVOSCloud's logging and analytics backend.
	/// Methods will return immediately and cache requests (along with timestamps)
	/// to be handled in the background. 
	/// </summary>
	public class AVAnalytics
	{
		private static readonly int AVANALYTIC_BATCH = 30;
		private static readonly int AVANALYTIC_INTERVAL = 20;
		private static readonly string AVCACHE_ANALYTICDATA_KEY = "AVAnalyticData";
		internal static readonly System.Object mutex = new System.Object();
		private static IList<AVAnalyticEvent> eventTics;
		private static IList<AVAnalyticActivity> activityTics;
		private static DateTime oppendTime;
		private static IDictionary<string, object> launch;
		private static string sessionId;
		private static string appId;

		internal static IDictionary<string,object> AnalyticData{ get; set; }

		internal static string AnalyticDataString
		{
			get
			{
				AnalyticData = GetCurrentAnalyticData();
				return AVClient.SerializeJsonString(AnalyticData);
			}
		}
		/// <summary>
		/// 1=批量发送;
		/// 7=启动发送;
		/// 6=按最小时间间隔发送。
		/// </summary>
		private static int transStrategy;
		private static bool toggleTrack;
		private static string transStrategyUrl = "/statistics/apps/{0}/sendPolicy";

		static AVAnalytics()
		{
			Initialize();
		}

		internal static void Initialize()
		{
			SetSeesion();
			appId = AVClient.ApplicationId;
			InitializeAVAnalytic();
		}

		internal static void SetSeesion()
		{

			sessionId = Guid.NewGuid().ToString();

			activityTics = new List<AVAnalyticActivity>();
			eventTics = new List<AVAnalyticEvent>();

			launch = new Dictionary<string, object>()
			{ 
				{ "date" ,UnixTimestampFromDateTime(DateTime.UtcNow)},
				{ "sessionId" , sessionId }
			};
			oppendTime = DateTime.Now;
		}

		internal static void ResetSession()
		{
			sessionId = Guid.NewGuid().ToString();
			activityTics = new List<AVAnalyticActivity>();
			eventTics = new List<AVAnalyticEvent>();

			launch = new Dictionary<string, object>()
			{ 
				{ "date" ,UnixTimestampFromDateTime(DateTime.UtcNow)},
				{ "sessionId" , sessionId }
			};
			oppendTime = DateTime.Now;
			if (transStrategy == 7)
			{
				SendCacheToServer();
			}
		}

		internal static void CloseSession()
		{
			sessionId = Guid.Empty.ToString();

			activityTics = new List<AVAnalyticActivity>();
			eventTics = new List<AVAnalyticEvent>();
		}

		internal static void SendCacheToServer(IDictionary<string,object> data)
		{
			try
			{
				SendAnalyticDataAsync(data);
			} catch
			{

			}
		}

		internal static Task InitializeAVAnalytic()
		{
			SendCacheToServer();
			string currentSessionToken = AVUser.CurrentSessionToken;
			CancellationToken cancellationToken = new CancellationToken();
			return AVClient.RequestAsync("GET", String.Format(transStrategyUrl, appId), currentSessionToken, null, cancellationToken).ContinueWith(t =>
				{
					var result = t.Result;
					if (result.Item1 == System.Net.HttpStatusCode.OK)
					{
						toggleTrack = Boolean.Parse(result.Item2 ["enable"].ToString());
						transStrategy = int.Parse(result.Item2 ["policy"].ToString());
					} else
					{

					}
				}).ContinueWith(s =>
					{
						if (toggleTrack)
						{
							if (transStrategy == 6)
							{
								Timer timer = new Timer((t =>
									{
										Task.Factory.StartNew<Task>(SendAnalyticDataAsync, null);
									}), null, Timeout.Infinite, AVANALYTIC_INTERVAL * 1000);

							} else if (transStrategy == 7)
							{
								//SendCacheToServer();
							}
						}
					});
		}

		public static Task SendAnalyticDataAsync(object state)
		{
			IDictionary<string, object> data = null;
			if (state != null)
			{
				data = state as IDictionary<string, object>;
			} else
			{
				data = GetCurrentAnalyticData();
			}
			string currentSessionToken = AVUser.CurrentSessionToken;
			CancellationToken cancellationToken = new CancellationToken();
			return AVClient.RequestAsync("POST", "/stats/collect", currentSessionToken, data, cancellationToken).ContinueWith(t =>
				{

					var r = t.Result;
					if (r.Item1 == HttpStatusCode.OK)
					{
						activityTics = new List<AVAnalyticActivity>();
						eventTics = new List<AVAnalyticEvent>();
					} else
					{
						//cache for send next time.
						//AVPersistence.AppendCacheList(AVCACHE_ANALYTICDATA_KEY, data);
					}
				});
		}

		public static IDictionary<string,object> GetDeviceInfo()
		{
			var rtn = AVClient.DeviceHook;
			if (!rtn.ContainsKey("app_version"))
			{
				rtn.Add("app_version", AVClient.BundelVersion);
			} else
			{
				rtn ["app_version"] = AVClient.BundelVersion;
			}
			if (!rtn.ContainsKey("channel"))
			{
				rtn.Add("channel", AVClient.Channel);
			} else
			{
				rtn ["channel"] = AVClient.Channel;
			}
			if (!rtn.ContainsKey("channel"))
			{
				rtn.Add("display_name", AVClient.bundleDisplayName);
			} else
			{
				rtn ["display_name"] = AVClient.bundleDisplayName;
			}
			return rtn;
		}

		public static IDictionary<string, object> GetCurrentAnalyticData()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			try
			{
				lock (mutex)
				{
					var pageTerminates = new Dictionary<string, object>();
					pageTerminates.Add("sessionId", sessionId);
					var device = GetDeviceInfo();
					pageTerminates.Add("activities", activityTics.ToListDictionary<AVAnalyticActivity>());
					pageTerminates.Add("duration", (DateTime.Now - oppendTime).TotalMilliseconds);
					var events = new Dictionary<string, object>()
					{
						{"event" ,eventTics.ToListDictionary<AVAnalyticEvent>()},
						{"launch",launch},
						{"terminate",pageTerminates}
					};
					data.Add("device", device);
					data.Add("events", events);
					return data;
				}
			} catch (Exception e)
			{
				return data;
			}
		}

		public static void SaveCurrentSession()
		{
			if (toggleTrack)
			{
				if (transStrategy == 7)
				{
					var currentSessionData = GetCurrentAnalyticData();
					//CloseSession();
				}
			}
		}

		void RealTimeCheck()
		{
			if (transStrategy == 1)
			{
				if (eventTics.Count + activityTics.Count >= AVANALYTIC_BATCH)
				{
					Task.Factory.StartNew<Task>(SendAnalyticDataAsync, null);
				}
			}
		}

		/// <summary>
		/// Tracks this application being launched.
		/// </summary>
		/// <returns>An Async Task that can be waited on or ignored.</returns>
		public static void TrackAppOpened()
		{
			//return AVAnalytics.TrackAppOpenedWithPushHashAsync(null);
			launch = new Dictionary<string, object>()
			{ 
				{ "date" , UnixTimestampFromDateTime(DateTime.UtcNow)},
				{ "sessionId" , sessionId }
			};

			TrackEvent("!AV!AppOpen");
		}

		/// <summary>
		/// Tracks the occurrence of a custom event with additional dimensions.
		/// AVOSCloud will store a data point at the time of invocation with the
		/// given event name.
		/// Dimensions will allow segmentation of the occurrences of this
		/// custom event.
		/// To track a user signup along with additional metadata, consider the
		/// following:
		/// <code>
		/// IDictionary&lt;string, string&gt; dims = new Dictionary&lt;string, string&gt; {
		///   { "gender", "m" },
		///   { "source", "web" },
		///   { "dayType", "weekend" }
		/// };
		/// AVAnalytics.TrackEvent("signup", dims);
		/// </code>
		/// There is a default limit of 4 dimensions per event tracked.
		/// </summary>
		/// <param name="name">The name of the custom event to report to AVClient
		/// as having happened.</param>
		/// <returns>An Async Task that can be waited on or ignored.</returns>
		public static void TrackEvent(string name)
		{
			IDictionary<string,string> dic = new Dictionary<string,string>();
			AVAnalytics.TrackEvent(name, dic);
		}

		public static void StartEvent(string name)
		{
			IDictionary<string,string> dic = new Dictionary<string,string>();
			StartEvent(name, dic);
		}

		public static void StartEvent(string name, IDictionary<string, string> dimensions)
		{
			lock (mutex)
			{
				if (eventTics.Any(item => item.name.Equals(name)))
				{
					var eventInstance = eventTics.First(item => item.name.Equals(name));
					if (!eventInstance.stop)
					{
						eventInstance.ts = UnixTimestampFromDateTime(DateTime.UtcNow);
						return;
					}
				}
				var eventStart = new AVAnalyticEvent()
				{
					du = 0,
					name = name,
					sessionId = sessionId,
					tag = name,
					ts = UnixTimestampFromDateTime(DateTime.UtcNow),
					attributes = dimensions
				};
				eventTics.Add(eventStart);
			}
		}

		public static void StartEvent(string name, IDictionary<string, object> dimensions)
		{
			var stringDimensions = new Dictionary<string,string>();
			if (dimensions != null)
			{
				foreach(var kv in dimensions)
				{
					stringDimensions.Add(kv.Key,kv.Value.ToString());
				}
			}
			StartEvent(name, stringDimensions);
		}

		public static void StopEvent(string name)
		{
			StopEvent(name, null);
		}

		public static void StopEvent(string name, IDictionary<string, object> dimensions)
		{
			lock (mutex)
			{
				if (eventTics.Any(item => item.name.Equals(name)))
				{
					var eventInstance = eventTics.First(item => item.name.Equals(name));
					eventInstance.stop = true;
					eventInstance.du = UnixTimestampFromDateTime(DateTime.UtcNow) - eventInstance.ts;
				}
			}
		}

		/// <summary>
		/// 记录一个自定义事件的产生
		/// </summary>
		/// <param name="name">事件名称</param>
		/// <param name="dimensions">自定义参数字典</param>
		public static void TrackEvent(string name, IDictionary<string, object> dimensions)
		{
			var stringDimensions = new Dictionary<string,string>();
			if (dimensions != null)
			{
				foreach(var kv in dimensions)
				{
					stringDimensions.Add(kv.Key,kv.Value.ToString());
				}
			}
			TrackEvent(name, stringDimensions);
		}
		/// <summary>
		/// 记录一个自定义事件的产生
		/// </summary>
		/// <param name="name">事件名称</param>
		/// <param name="dimensions">自定义参数字典</param>
		public static void TrackEvent(string name, IDictionary<string, string> dimensions)
		{
			lock (mutex)
			{
				if (name == null || name.Trim().Length == 0)
				{
					throw new ArgumentException("A name for the custom event must be provided.");
				}

				var eventTic = new AVAnalyticEvent()
				{
					du = 0,
					name = name,
					sessionId = sessionId,
					tag = name,
					ts = UnixTimestampFromDateTime(DateTime.UtcNow),
					attributes = dimensions
				};

				eventTics.Add(eventTic);
			}
		}

		private static long UnixTimestampFromDateTime(DateTime date)
		{
			long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
			unixTimestamp /= TimeSpan.TicksPerMillisecond;
			return unixTimestamp;
		}

		public static void OnSceneStart(string pageName)
		{
			if (pageName == null || pageName.Trim().Length == 0)
			{
				throw new ArgumentException("A name for the custom event must be provided.");
			}

			var activity = new AVAnalyticActivity()
			{
				du = 0,
				name = pageName,
				ts = UnixTimestampFromDateTime(DateTime.UtcNow)
			};
			activityTics.Add(activity);

		}

		public static void OnSceneEnd(string pageName)
		{
			if (pageName == null || pageName.Trim().Length == 0)
			{
				throw new ArgumentException("A name for the custom event must be provided.");
			}

			if (activityTics.Any(item => item.name.Equals(pageName)))
			{
				var activityInstance = activityTics.First(item => item.name.Equals(pageName));
				activityInstance.du = UnixTimestampFromDateTime(DateTime.UtcNow) - activityInstance.ts;
			}
		}

		public static void OnAppSetBackgroud()
		{
			SaveCurrentSession();
			CloseSession();
		}

		public static void OnExit()
		{
			SaveCurrentSession();
			CloseSession();
		}
	}
		
}
