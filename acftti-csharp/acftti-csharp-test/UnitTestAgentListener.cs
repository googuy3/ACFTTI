using acftti_csharp_lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace acftti_csharp_test
{
    [TestClass]
    public class UnitTestAgentListener
    {
        private const int UDP_TEST_PORT = 10000;

        [TestMethod]
        public void TestSingleMessageRead()
        {
            AgentListener al = new AgentListener();
            //al.Listen(UDP_TEST_PORT);
            Thread listenThread = new Thread(() => al.Listen(UDP_TEST_PORT));
            listenThread.Start();
            helperSendUDPMessageToLocalhost("test1");
            Thread.Sleep(1000);
            Assert.AreEqual("test1", al.GetNextMessage());
        }

        private void helperSendUDPMessageToLocalhost(string message)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress broadcast = IPAddress.Parse("192.168.1.255");

            byte[] sendbuf = Encoding.ASCII.GetBytes(message);
            IPEndPoint ep = new IPEndPoint(broadcast, UDP_TEST_PORT);

            s.SendTo(sendbuf, ep);
            s.Close();
        }
    }
}
