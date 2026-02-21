using UnityEngine;

namespace Scripts.Global.Util
{
    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            string wrapped = "{\"Items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper.Items;
        }
        [System.Serializable]
        private class Wrapper<T> {
            public T[] Items;
        }
    }
}