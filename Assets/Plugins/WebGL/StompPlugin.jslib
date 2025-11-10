var client = null;


mergeInto(LibraryManager.library, {
    // Function to connect to STOMP server
  ConnectStompSocket: function (urlPtr, tokenPtr) {
    const url = UTF8ToString(urlPtr);
    const token = UTF8ToString(tokenPtr);

    // console.log("Connecting to STOMP at:", url);

    const socket = new WebSocket(url);
    client = Stomp.over(socket);
    client.debug = null;

    client.connect({
        Authorization: `Bearer ${token}`
    }, function (frame) {
      // console.log("Connected:", frame);
      SendMessage("StompConnector", "OnConnected", JSON.stringify(frame.headers));
    }, function (error) {
      console.error("STOMP error:", error);
      SendMessage("StompConnector", "OnError", error);
    });
  },

  SubscribeStomp: function (topicPtr, callbackPtr, subscriptionIdPtr) {
    const topic = UTF8ToString(topicPtr);
    const callback = UTF8ToString(callbackPtr);
    const subscriptionId = UTF8ToString(subscriptionIdPtr);

    // console.log("Subscribing to topic:", topic, "with callback:", callback);

    client.subscribe(topic, function (message) {
      // console.log("Received message from", topic, ":", message.body);
      let bodyStr = "";
      
        if (typeof message.body === "string") {
          bodyStr = message.body;
        } else if (typeof message.body === "object") {
          try {
            bodyStr = JSON.stringify(message.body);
          } catch (e) {
            console.warn("Failed to stringify message body:", e);
            bodyStr = message.body.toString();
          }
        } else {
          bodyStr = String(message.body);
        }
      SendMessage("StompConnector", callback, bodyStr);
    }, { id: subscriptionId });
  },

  SendStomp: function (topicPtr, messagePtr) {
    const topic = UTF8ToString(topicPtr);
    const message = UTF8ToString(messagePtr);

    // console.log("Sending message to topic:", topic, "Message:", message);

    client.send(topic, {}, message);
  },

  UnsubscribeStomp: function (subscriptionIdPtr) {
    const subscriptionId = UTF8ToString(subscriptionIdPtr);
    // console.log("Unsubscribing from subscription ID:", subscriptionId);
    client.unsubscribe(subscriptionId);
  },

    DisconnectStomp: function () {
        // console.log("Disconnecting from STOMP");
        if (client) {
        client.disconnect(function () {
          // console.log("Disconnected from STOMP");
          SendMessage("StompConnector", "OnDisconnected", "Disconnected");
        });
        }
    }
});