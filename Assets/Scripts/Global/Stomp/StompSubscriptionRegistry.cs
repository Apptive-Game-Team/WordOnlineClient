using System;
using System.Collections.Generic;

namespace Global.Stomp
{
    /// <summary>
    /// 구독 정보를 보관하고, 메시지 수신 시 콜백으로 라우팅.
    /// 재연결 시 ResubscribeAll()로 한 번에 재구독 가능.
    /// </summary>
    public class StompSubscriptionRegistry
    {
        private class Entry
        {
            public readonly string Topic;
            public readonly string SubscriptionId;
            public readonly Action<string> Callback;

            public Entry(string topic, string subscriptionId, Action<string> callback)
            {
                Topic = topic;
                SubscriptionId = subscriptionId;
                Callback = callback;
            }
        }

        private readonly List<Entry> _entries = new List<Entry>();

        public void Register(string topic, string subscriptionId, Action<string> callback)
        {
            // 동일 subscriptionId 중복 등록 방지
            _entries.RemoveAll(e => e.SubscriptionId == subscriptionId);
            _entries.Add(new Entry(topic, subscriptionId, callback));
        }

        public void Unregister(string subscriptionId)
        {
            _entries.RemoveAll(e => e.SubscriptionId == subscriptionId);
        }

        /// <summary>subscriptionId에 해당하는 콜백으로 body를 전달</summary>
        public void Dispatch(string subscriptionId, string body)
        {
            var entry = _entries.Find(e => e.SubscriptionId == subscriptionId);
            entry?.Callback?.Invoke(body);
        }

        /// <summary>재연결 후 기존 구독을 transport에 다시 등록</summary>
        public void ResubscribeAll(IStompTransport transport)
        {
            foreach (var entry in _entries)
                transport.Subscribe(entry.Topic, entry.SubscriptionId);
        }

        public void Clear() => _entries.Clear();
    }
}
