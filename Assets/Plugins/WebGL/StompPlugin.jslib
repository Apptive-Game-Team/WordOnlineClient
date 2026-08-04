var client = null;

mergeInto(LibraryManager.library, {

  // STOMP 서버에 WebSocket으로 연결
  ConnectStompSocket: function (urlPtr, tokenPtr) {
    const url = UTF8ToString(urlPtr);
    const token = UTF8ToString(tokenPtr);

    const socket = new WebSocket(url);
    client = Stomp.over(socket);
    client.debug = null;

    client.connect(
      { Authorization: 'Bearer ' + token },
      function (frame) {
        SendMessage('StompConnector', 'OnConnected', JSON.stringify(frame.headers));
      },
      function (error) {
        console.error('STOMP error:', error);
        SendMessage('StompConnector', 'OnError', String(error));
      }
    );
  },

  // 토픽 구독 – 수신 메시지는 항상 OnSubscriptionMessage({ id, body })로 라우팅
  SubscribeStomp: function (topicPtr, subscriptionIdPtr) {
    const topic = UTF8ToString(topicPtr);
    const subscriptionId = UTF8ToString(subscriptionIdPtr);

    if (!client || !client.connected) {
      console.warn('SubscribeStomp: STOMP client is not connected.');
      SendMessage('StompConnector', 'OnError', 'STOMP client is not connected.');
      return;
    }

    client.subscribe(topic, function (message) {
      var bodyStr;
      if (typeof message.body === 'string') {
        bodyStr = message.body;
      } else if (message.body && typeof message.body === 'object') {
        try { bodyStr = JSON.stringify(message.body); }
        catch (e) { bodyStr = String(message.body); }
      } else {
        bodyStr = String(message.body);
      }

      var payload = JSON.stringify({ id: subscriptionId, body: bodyStr });
      SendMessage('StompConnector', 'OnSubscriptionMessage', payload);
    }, { id: subscriptionId });
  },

  // 메시지 전송
  SendStomp: function (topicPtr, messagePtr) {
    const topic = UTF8ToString(topicPtr);
    const message = UTF8ToString(messagePtr);

    if (!client || !client.connected) {
      console.warn('SendStomp: STOMP client is not connected.');
      SendMessage('StompConnector', 'OnError', 'STOMP client is not connected.');
      return;
    }

    client.send(topic, {}, message);
  },

  // 구독 해제
  UnsubscribeStomp: function (subscriptionIdPtr) {
    const subscriptionId = UTF8ToString(subscriptionIdPtr);
    if (client) {
      client.unsubscribe(subscriptionId);
    }
  },

  // 연결 종료
  DisconnectStomp: function () {
    if (client) {
      // Called by StompConnector.OnDestroy during an intentional scene change.
      // The GameObject no longer exists when this asynchronous callback runs,
      // so an OnDisconnected SendMessage would be both stale and misleading.
      client.disconnect(function () {});
      client = null;
    }
  }
});
