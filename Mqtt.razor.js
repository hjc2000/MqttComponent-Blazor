﻿class Mqtt
{
	/**
	 * 如果没有传递参数，则会使用匿名连接
	 * @param {string} username
	 * @param {string} password
	 */
	constructor(dotnetHelper, username, password, clientId)
	{
		let options = {
			// Clean session
			clean: true,
			connectTimeout: 3000,
			keepalive: 15,
			// Auth
			username,
			password,
			clientId,
		}
		this.dotnetHelper = dotnetHelper;
		this.client = mqtt.connect("ws://127.0.0.1:8083/mqtt", options);
		this.b_HadBeenConnected = false;//过去曾经连接成功过
		this.client.on("reconnect", () =>
		{
			/**
			 * 请求被emqx拒绝后并不会进入 error 事件，而是不断进入
			 * reconnect 事件
			 * 没办法，只能通过这个事件来通知用户连接失败。为了区分到底是连接失
			 * 败还是短线重连，需要设置一个标志位 b_HadBeenConnected
			 */
			if (!this.b_HadBeenConnected)
			{
				//触发 .NET 的连接失败事件
				this.dotnetHelper.invokeMethodAsync("OnConnectFail");
				this.client.end(true);//关闭客户端，防止一直重连
			}
			else
			{
				console.log("重连");
			}
		});
		this.client.on("connect", () =>
		{
			this.b_HadBeenConnected = true;
			this.dotnetHelper.invokeMethodAsync("OnConnected");
		});
		//在收到数据后将数据传给.NET方法
		this.client.on("message", (topic, payload) =>
		{
			payload = Uint8Array.from(payload.toJSON().data);
			this.dotnetHelper.invokeMethodAsync("OnReceived", topic, payload);
		});
	}
	/**
	 * 发布主题
	 * @param {string} topic
	 * @param {Uint8Array} payload
	 */
	publish(topic, payload)
	{
		this.client.publish(topic, payload, { qos: 1 });
	};
	/**
	 * 订阅主题
	 * @param {any} topic
	 */
	subscribe(topic)
	{
		this.client.subscribe(topic, { qos: 1 }, (error, granted) =>
		{
			if (error)
			{
				console.log(error);
			}
			else
			{
				console.log("订阅了" + granted[0].topic);
			}
		});
	}

}

export function getMqtt(dotnetHelper, username, password, clientId)
{
	return new Mqtt(dotnetHelper, username, password, clientId);
}
