class Mqtt {
    /**
     * 如果没有传递参数，则会使用匿名连接
     * @param {string} username
     * @param {string} password
     */
    constructor(dotnetHelper, username, password) {
        let options = {
            // Clean session
            clean: true,
            // Auth
            username,
            password,
        }

        this.dotnetHelper = dotnetHelper;

        this.client = mqtt.connect("ws://127.0.0.1:8083/mqtt", options);

        this.client.on("connect", () => {
            this.dotnetHelper.invokeMethodAsync("OnConnected");
            });

        //在收到数据后将数据传给.NET方法
        this.client.on("message", (topic, payload) => {
            payload = Uint8Array.from(payload.toJSON().data);
            this.dotnetHelper.invokeMethodAsync("OnReceived", topic, payload);
        });

    }

    /**
     * 发布主题
     * @param {string} topic
     * @param {Uint8Array} payload
     */
    publish(topic, payload) {
        this.client.publish(topic, payload);
    };

    subscribe(topic) {
        this.client.subscribe(topic, (error, granted) => {
            if (error) {
                console.log(error);
            } else {
                console.log("订阅了" + granted[0].topic);
            }
        });
    }
}

export function getMqtt(dotnetHelper) {
    return new Mqtt(dotnetHelper, "hjc", "123456");
}

/**
 * 从CDN获取mqtt.js并加载，加载完成后触发事件，调用 .NET 方法
 * @param {any} dotnetHelper
 */
export function installMqtt(dotnetHelper) {
    let script = document.createElement('script');
    //script.src = "https://unpkg.com/mqtt/dist/mqtt.min.js";
    script.src = "/js/mqtt.min.js";
    script.async = true;
    script.onload = () => {
        dotnetHelper.invokeMethodAsync("OnInstalled");
    }
    document.head.append(script);
}
