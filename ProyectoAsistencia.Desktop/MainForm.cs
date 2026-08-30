using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace ProyectoAsistencia.Desktop
{
    public class MainForm : Form
    {
        private readonly WebView2 _webView = new WebView2();
        private readonly Puente _puente = new Puente();

        // Nombre "virtual" con el que el WebView2 va a ver la carpeta wwwroot.
        // Así el HTML puede pedir cosas como https://app.local/index.html
        // en vez de usar rutas file:// (que traen problemas de permisos/CORS).
        private const string HostVirtual = "app.local";

        public MainForm()
        {
            Text = "Sistema de Registro de Asistencia";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            _webView.Dock = DockStyle.Fill;
            Controls.Add(_webView);

            Load += async (s, e) => await InicializarWebViewAsync();
        }

        private async System.Threading.Tasks.Task InicializarWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async(null);

            string carpetaWwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                HostVirtual,
                carpetaWwwroot,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            // Cada vez que el JS hace window.chrome.webview.postMessage(objeto),
            // este evento se dispara con el JSON recibido.
            _webView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string jsonRecibido = e.WebMessageAsJson;
                string jsonRespuesta = _puente.ProcesarMensaje(jsonRecibido);

                // Se devuelve la respuesta al JS. El HTML escucha esto con:
                // window.chrome.webview.addEventListener('message', ...)
                _webView.CoreWebView2.PostWebMessageAsJson(jsonRespuesta);
            };

            _webView.CoreWebView2.Navigate($"https://{HostVirtual}/index.html");
        }
    }
}
