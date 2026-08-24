var client = null;

// 지금 유효한 연결 세대. ConnectStompSocket과 DisconnectStomp가 세대를 올린다.
// 옛 소켓이 뒤늦게 올리는 콜백을 걸러내는 데 쓴다.
var clientGeneration = 0;

mergeInto(LibraryManager.library, {

  // STOMP 서버에 WebSocket으로 연결
  ConnectStompSocket: function (urlPtr, tokenPtr) {
    const url = UTF8ToString(urlPtr);
    const token = UTF8ToString(tokenPtr);

    // 세대를 먼저 올린다. 바로 아래에서 옛 소켓을 닫을 때 터지는 콜백은 옛 세대를
    // 들고 있으므로 여기서부터 걸러진다.
    const generation = ++clientGeneration;

    // 앞선 연결이 남아 있으면 반드시 먼저 닫는다. 닫지 않으면 전역 client가 새 소켓을
    // 가리키는 동안 옛 소켓이 참조 없이 열린 채 남아 서버에 STOMP 세션이 누수되고,
    // 옛 소켓의 핸드셰이크가 뒤늦게 끝나면 IsConnected만 true가 된 채 구독과 전송은
    // 아직 연결되지 않은 새 소켓으로 라우팅된다.
    if (client) {
      const stale = client;
      client = null;
      try { stale.disconnect(); } catch (e) {}
      try { if (stale.ws) stale.ws.close(); } catch (e) {}
    }

    const socket = new WebSocket(url);
    const stompClient = Stomp.over(socket);
    client = stompClient;
    stompClient.debug = null;

    stompClient.connect(
      { Authorization: 'Bearer ' + token },
      function (frame) {
        if (generation !== clientGeneration) return;
        SendMessage('StompConnector', 'OnConnected', JSON.stringify(frame.headers));
      },
      function (error) {
        if (generation !== clientGeneration) return;
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
    if (!client) return;

    // 세대를 올려 종료 과정에서 터지는 옛 소켓 콜백을 막는다.
    const stale = client;
    client = null;
    clientGeneration++;

    try {
      stale.disconnect(function () {
        SendMessage('StompConnector', 'OnDisconnected', 'Disconnected');
      });
    } catch (e) {
      // 핸드셰이크 도중이면 DISCONNECT 프레임 전송이 실패한다. 소켓만 닫고 알린다.
      try { if (stale.ws) stale.ws.close(); } catch (closeError) {}
      SendMessage('StompConnector', 'OnDisconnected', 'Disconnected');
    }
  }
});
