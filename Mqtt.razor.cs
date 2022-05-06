using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp1.MqttComponent
{
	public partial class Mqtt
	{
		//数据类型
		public class MqttOptions
		{
			string _username = "";
			string _password = "";
			public string Username
			{
				get { return _username; }
				set { _username = value; }
			}
			public string Password
			{
				get { return _password; }
				set { _password = value; }
			}
		}
		public class Msg
		{
			string _topic = "";
			byte[] _payload = Array.Empty<byte>();
			public string Topic
			{
				get { return _topic; }
				set { _topic = value; }
			}
			public byte[] Payload
			{
				get { return _payload; }
				set { _payload = value; }
			}
		}

		//参数
		[Parameter]
		public MqttOptions? Options { get; set; }
		/// <summary>
		/// MQTT组件的默认配置
		/// </summary>
		public static MqttOptions DefaultOptions
		{
			get
			{
				return _defaultOptions;
			}
			set
			{
				_defaultOptions = value;
			}
		}
		static MqttOptions _defaultOptions = new();

		//事件
		[Parameter]
		public EventCallback OnConnectedCallback { get; set; }
		[Parameter]
		public EventCallback<Msg> OnReceivedCallback { get; set; }
		[Parameter]
		public EventCallback OnConnectFailCallback { get; set; }

		//给 JS 调用的函数
		/// <summary>
		/// 连接成功
		/// </summary>
		[JSInvokable]
		public async void OnConnected()
		{
			await OnConnectedCallback.InvokeAsync();
		}
		/// <summary>
		/// 接收到 mqtt 消息
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="payload"></param>
		[JSInvokable]
		public async void OnReceived(string topic, byte[] payload)
		{
			await OnReceivedCallback.InvokeAsync(new Msg()
			{
				Topic = topic,
				Payload = payload,
			});
		}
		/// <summary>
		/// mqtt.js安装成功
		/// </summary>
		[JSInvokable]
		public void OnInstalled()
		{
			_installed = true;
		}
		bool _installed = false;
		/// <summary>
		/// 连接失败
		/// </summary>
		[JSInvokable]
		public async void OnConnectFail()
		{
			await OnConnectFailCallback.InvokeAsync();
		}

		//提供给父组件的方法
		/// <summary>
		/// 发布消息
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="payload"></param>
		public async void Publish(string topic, byte[] payload)
		{
			if (_mqtt is not null)
			{
				await _mqtt.InvokeVoidAsync("publish", topic, payload);
			}
		}
		/// <summary>
		/// 订阅主题
		/// </summary>
		/// <param name="topic"></param>
		public async void Subscribe(string topic)
		{
			if (_mqtt is not null)
			{
				await _mqtt.InvokeVoidAsync("subscribe", topic);
			}
		}
		/// <summary>
		/// 尝试连接
		/// </summary>
		public async void TryConnect()
		{
			//等待安装成功
			while (!_installed)
			{
				await Task.Delay(1000);
			}
			if (module != null)
			{
				//如果用户没有设置 Options 则使用默认的设置
				if (Options == null)
				{
					_mqtt = await module.InvokeAsync<IJSObjectReference>("getMqtt", _dotnetHelper, DefaultOptions.Username, DefaultOptions.Password);
				}
				else
				{
					_mqtt = await module.InvokeAsync<IJSObjectReference>("getMqtt", _dotnetHelper, Options.Username, Options.Password);
				}
			}
		}

		//生命周期
		IJSObjectReference? module;
		IJSObjectReference? _mqtt;
		DotNetObjectReference<Mqtt>? _dotnetHelper;
		/// <summary>
		/// 组件初始化
		/// </summary>
		/// <param name="firstRender"></param>
		/// <returns></returns>
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				module = await JS.InvokeAsync<IJSObjectReference>("import", "./MqttComponent/Mqtt.razor.js");
				_dotnetHelper = DotNetObjectReference.Create(this);
				await module.InvokeVoidAsync("installMqtt", _dotnetHelper);
			}
		}

	}
}
