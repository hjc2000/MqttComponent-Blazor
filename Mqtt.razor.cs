using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp1.MqttComponent
{
	public partial class Mqtt
	{
		[Inject]
		IJSRuntime? JS { get; set; }

		//数据类型
		public class MqttOptions
		{
			public string Username { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}
		public class Msg
		{
			public string Topic { get; set; } = string.Empty;
			public byte[] Payload { get; set; } = Array.Empty<byte>();
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
			string clientId = DefaultOptions.Username + "-" + Guid.NewGuid().ToString();
			//等待安装成功
			var tryConnect = async Task<bool> () =>
			  {
				  try
				  {
					  if (module != null)
					  {
						  Console.WriteLine("moudle不为空，尝试连接");
						  //如果用户没有设置 Options 则使用默认的设置
						  if (Options == null)
						  {
							  _mqtt = await module.InvokeAsync<IJSObjectReference>("getMqtt", _dotnetHelper, DefaultOptions.Username, DefaultOptions.Password, clientId);
						  }
						  else
						  {
							  _mqtt = await module.InvokeAsync<IJSObjectReference>("getMqtt", _dotnetHelper, Options.Username, Options.Password, clientId);
						  }
						  return true;
					  }
					  else
					  {
						  Console.WriteLine("moudle为空");
						  return false;
					  }
				  }
				  catch
				  {
					  Console.WriteLine("异常");
					  return false;
				  }
			  };
			while (!await tryConnect())
			{
				await Task.Delay(1000);
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
				while (JS == null)
				{
					await Task.Delay(1000);
				}
				Console.WriteLine("MQTT组件已加载");
				module = await JS.InvokeAsync<IJSObjectReference>("import", "./MqttComponent/Mqtt.razor.js");
				_dotnetHelper = DotNetObjectReference.Create(this);
				TryConnect();
			}
		}

	}
}
