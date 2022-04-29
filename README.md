# MqttComponent-Blazor
## 更改代码中的相对路径
使用前需要修改一处代码。在 Mqtt.razor 文件中有一处代码如下图所示
![image](https://user-images.githubusercontent.com/96368006/165929920-06644510-2885-4a03-bb45-ef9de988034c.png)
这个路径是用来导入 Mqtt.razor.js 的。想要生成项目后 .NET 运行时能够找到 Mqtt.razor.[后缀名] 这种文件需要遵守一定的规则。

规则：
除了 wwwroot 文件夹中的文件，其他文件 想要在 .razor 文件被中引用，并且让 .NET 运行时能够找到，必须以项目文件夹为根路径，使用相对路径来定位该文件。
例如：将 MqttComponent 文件夹放在项目文件夹中，则 MqttComponent 文件夹中的 Mqtt.razor 文件想要找到 Mqtt.razor.js 文件，需要使用路径 `"./MqttComponent/Mqtt.razor.js"`

## 包含命名空间
项目名称为 BlazorApp1，该组件被放在项目根文件夹中的 MqttComponent 文件夹中，则需要包含以下两个命名空间
```c#
@using BlazorApp1.MqttComponent
@using static BlazorApp1.MqttComponent.Mqtt

```
## 使用示例
```razor
@page "/"
@using BlazorApp1.MqttComponent
@using static BlazorApp1.MqttComponent.Mqtt
@inject IJSRuntime JS
@inject NavigationManager NavigationManager

<PageTitle>Index</PageTitle>
<Login></Login>
<Mqtt Options="@_options" @ref="_mqtt"
OnConnectedCallback="@(()=>
{
	Console.WriteLine("已连接");
	_mqtt?.Subscribe("esp32/temperature");
})"
OnReceivedCallback="@((msg)=>
{
	double temp = BitConverter.ToDouble(msg._payload, 0);
	Console.WriteLine(temp);
})"></Mqtt>

@code
{
	MqttOptions _options = new MqttOptions()
		{
			_username = "hjc",
			_password = "123",
		};

	Mqtt? _mqtt;

	
}
```
