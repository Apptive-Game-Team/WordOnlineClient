using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.Data.Sse
{

    public class SseDownloadHandler : DownloadHandlerScript
    {
        private readonly Action<string> onLine;
        private StringBuilder buffer = new StringBuilder();

        public SseDownloadHandler(Action<string> onLine)
        {
            this.onLine = onLine;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
                return false;

            string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            buffer.Append(chunk);

            // 줄 단위 처리
            string[] lines = buffer.ToString().Split('\n');

            // 마지막 줄은 아직 완료되지 않았을 수 있음
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("data:"))
                {
                    string message = line.Substring("data:".Length).Trim();
                    Debug.Log(message);
                    onLine?.Invoke(message);
                }
            }

            // 마지막 조각을 버퍼에 남김
            buffer.Clear();
            buffer.Append(lines[^1]);

            return true;
        }
    }

}