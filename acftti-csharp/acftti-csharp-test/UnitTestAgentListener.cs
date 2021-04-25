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
        public void TestSingleMessageIsRecieved()
        {
            AgentListener al = new AgentListener();
            Thread listenThread = new Thread(() => al.Listen(UDP_TEST_PORT));
            listenThread.Start();
            Thread.Sleep(100);
            helperSendUDPMessageToLocalhost("test1");
            Thread.Sleep(100);
            Assert.IsTrue(al.MessageIsReady());
            listenThread.Abort();
            al.Cleanup();
        }

        [TestMethod]
        public void TestSingleMessageIsRecievedAndDequeued()
        {
            AgentListener al = new AgentListener();
            Thread listenThread = new Thread(() => al.Listen(UDP_TEST_PORT));
            listenThread.Start();
            Thread.Sleep(100);
            helperSendUDPMessageToLocalhost("test1");
            Thread.Sleep(100);
            Assert.IsTrue(al.MessageIsReady());
            Assert.AreEqual("test1", al.GetNextMessage());
            listenThread.Abort();
            al.Cleanup();
        }

        [TestMethod]
        public void TestMultiMessageRecieved()
        {
            AgentListener al = new AgentListener();
            Thread listenThread = new Thread(() => al.Listen(UDP_TEST_PORT));
            listenThread.Start();
            Thread.Sleep(100);
            helperSendUDPMessageToLocalhost("test1");
            helperSendUDPMessageToLocalhost("test2");
            helperSendUDPMessageToLocalhost("test3");
            Thread.Sleep(100);
            Assert.AreEqual(3, al.MessagesReady());
            listenThread.Abort();
            al.Cleanup();
        }

        [TestMethod]
        public void TestMultiMessageRecievedAndDequeued()
        {
            AgentListener al = new AgentListener();
            Thread listenThread = new Thread(() => al.Listen(UDP_TEST_PORT));
            listenThread.Start();
            Thread.Sleep(100);
            helperSendUDPMessageToLocalhost("test1");
            helperSendUDPMessageToLocalhost("test2");
            helperSendUDPMessageToLocalhost("test3");
            Thread.Sleep(100);
            // Note that this test might fail because UDP messages are not proccessed by the OS in order.
            Assert.AreEqual("test1", al.GetNextMessage());
            Assert.AreEqual("test2", al.GetNextMessage());
            Assert.AreEqual("test3", al.GetNextMessage());
            listenThread.Abort();
            al.Cleanup();
        }

        private void helperSendUDPMessageToLocalhost(string message)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress broadcast = IPAddress.Parse("127.0.0.1");

            byte[] sendbuf = Encoding.ASCII.GetBytes(message);
            IPEndPoint ep = new IPEndPoint(broadcast, UDP_TEST_PORT);

            s.SendTo(sendbuf, ep);
            s.Close();
        }
    }
}
