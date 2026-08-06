mergeInto(LibraryManager.library, {
  ConnectMatchEventStream: function (objectNamePtr, urlPtr, tokenPtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var url = UTF8ToString(urlPtr);
    var token = UTF8ToString(tokenPtr);

    if (window.wordOnlineMatchEventAbortController) {
      window.wordOnlineMatchEventAbortController.abort();
    }

    var controller = new AbortController();
    window.wordOnlineMatchEventAbortController = controller;

    fetch(url, {
      headers: {
        "Accept": "text/event-stream",
        "Authorization": "Bearer " + token
      },
      signal: controller.signal
    }).then(function (response) {
      if (!response.ok || !response.body) throw new Error("SSE HTTP " + response.status);
      var reader = response.body.getReader();
      var decoder = new TextDecoder();
      var buffer = "";

      function read() {
        return reader.read().then(function (result) {
          if (result.done) throw new Error("SSE stream closed");
          buffer += decoder.decode(result.value, { stream: true }).replace(/\r\n/g, "\n");
          var boundary;
          while ((boundary = buffer.indexOf("\n\n")) >= 0) {
            var block = buffer.substring(0, boundary);
            buffer = buffer.substring(boundary + 2);
            var data = [];
            block.split("\n").forEach(function (line) {
              if (line.indexOf("data:") === 0) data.push(line.substring(5).trimStart());
            });
            if (data.length) {
              SendMessage(objectName, "OnMatchSseEvent", JSON.stringify({ data: data.join("\n") }));
            }
          }
          return read();
        });
      }
      return read();
    }).catch(function (error) {
      if (error.name !== "AbortError") SendMessage(objectName, "OnMatchSseDisconnected", error.message);
    });
  },

  DisconnectMatchEventStream: function () {
    if (window.wordOnlineMatchEventAbortController) {
      window.wordOnlineMatchEventAbortController.abort();
      window.wordOnlineMatchEventAbortController = null;
    }
  }
});
