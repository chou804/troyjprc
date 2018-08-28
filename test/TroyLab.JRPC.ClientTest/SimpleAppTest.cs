using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TroyLab.JRPC.Client;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace TroyLab.JRPC.ClientTest
{
    public class SimpleAppTest
    {
        static Uri simpleAppUri = new Uri("http://localhost:50000");

        [Fact]
        public async Task EchoService_Test()
        {
            StartProcesses();

            var (e, v) = await JClient.CallAsync<EchoView>("echo", new { Msg = "Hello" }, serviceUri: simpleAppUri);
            Assert.Null(e);
            Assert.Equal("Hello", v.Msg);

            CloseProcesses();
        }

        public class EchoView
        {
            public string Msg { get; set; }
        }

        [Fact]
        public async Task MoreService_Test()
        {
            StartProcesses();

            {
                var (e, v) = await JClient.CallAsync<string>("Func11_1", "Hello", serviceUri: simpleAppUri);
                Assert.Null(e);
                Assert.Equal("Hello_func11_1", v);
            }

            {
                var (e, v) = await JClient.CallAsync<string>("Func10_1", "Hello", serviceUri: simpleAppUri);
                Assert.Null(e);
                Assert.Equal("Hello", v);
            }

            {
                var (e, v) = await JClient.CallAsync<string>("Func01_1", "Hello", serviceUri: simpleAppUri);
                Assert.Null(e);
                Assert.Equal("func01_1", v);
            }

            {
                var (e, v) = await JClient.CallAsync<string>("Func00_1", null, serviceUri: simpleAppUri);
                Assert.Null(e);
                Assert.Equal("Func00_1", v);
            }

            {
                var e = await JClient.CallAsync("Func11_0", "Hello", serviceUri: simpleAppUri);
                Assert.Null(e);
            }

            {
                var e = await JClient.CallAsync("Func10_0", "Hello", serviceUri: simpleAppUri);
                Assert.Null(e);
            }

            {
                var e = await JClient.CallAsync("Func01_0", null, serviceUri: simpleAppUri);
                Assert.Null(e);
            }

            {
                var e = await JClient.CallAsync("Func00_0", null, serviceUri: simpleAppUri);
                Assert.Null(e);
            }

            // no method error
            {
                var e = await JClient.CallAsync("NoSuchMethod", "Hello", serviceUri: simpleAppUri);
                Assert.Equal(-32601, e.Code);
            }

            CloseProcesses();
        }

        #region SimpleAppProcess
        public Process SimpleAppProcess;
        private void StartProcesses()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var xtesterPath = Path.GetDirectoryName(path);

            var APIPath = xtesterPath.Replace(@"test\TroyLab.JRPC.ClientTest", @"example\SimpleApp");
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet.exe",
                Arguments = "SimpleApp.dll --server.urls http://0.0.0.0:50000",
                WorkingDirectory = APIPath,
                UseShellExecute = true,
                CreateNoWindow = false,
            };
            SimpleAppProcess = Process.Start(processInfo);
        }

        private void CloseProcesses()
        {
            try
            {
                if (SimpleAppProcess != null) SimpleAppProcess.CloseMainWindow();
            }
            catch { };
        }
        #endregion
    }
}
