using RExiled.API.Features;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DreamPlugin
{
    public class WebServer : IDisposable
    {
        private HttpListener _listener;
        private bool _running;

        public void Start()
        {
            if (_running) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:80008/");
                _listener.Start();
                _running = true;

                Task.Run(ListenAsync);
            }
            catch (Exception ex)
            {
                Log.Error($"启动Web服务器失败: {ex}");
            }
        }

        private async Task ListenAsync()
        {
            while (_running && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    HandleRequest(context);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warn($"处理请求出错: {ex}");
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (path.Equals("/913", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var response = context.Response;
                    response.ContentType = "application/json; charset=utf-8";

                    int current = Player.List.Count();
                    int max = 60;

                    string json = $"{{\"maxPlayers\":{max},\"currentPlayers\":{current}}}";
                    byte[] buffer = Encoding.UTF8.GetBytes(json);

                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();
                }
                catch (Exception ex)
                {
                    Log.Warn($"发送响应失败: {ex}");
                    SendErrorResponse(context, 500);
                }
            }
            else
            {
                SendErrorResponse(context, 404);
            }
        }

        private void SendErrorResponse(HttpListenerContext context, int statusCode)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            response.Close();
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
            _listener?.Close();
            _listener = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
