﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace acftti_csharp_lib
{
    /// <summary>
    /// Listens for data from the Python agent running inside AC. Stores messages until they can be processed
    /// by another parser class.
    /// </summary>
    public class AgentListener
    {
        /// <summary>
        /// An internal queue for storing messages until the GUI is ready to process them.
        /// </summary>
        private ConcurrentQueue<string> messages;
        /// <summary>
        /// UDP Client that listens for requests.
        /// </summary>
        private UdpClient listener;
        /// <summary>
        /// UDP port to listen on. Generally should be a number larger than 1000, and should always be less than 65,536. If this
        /// is changed, the Python agent should be updated to match this value.
        /// </summary>
        private const int UDP_PORT = 5005;

        /// <summary>
        /// Default constructor. Initializes the internal queue.
        /// </summary>
        public AgentListener()
        {
            this.messages = new ConcurrentQueue<string>();
        }

        /// <summary>
        /// Starts listening on the default port defined by UDP_PORT. This is a blocking method that should be run on it's own thread
        /// as the queue accessor functions are all thread-safe.
        /// </summary>
        public void Listen()
        {
            this.Listen(UDP_PORT);
        }

        /// <summary>
        /// Starts listening on the specified port. This is a blocking method that should be run on it's own thread as the queue accessor
        /// functions are all thread-safe.
        /// </summary>
        /// <param name="port">UDP port to listen on. Should be between 1000 and 65,536.</param>
        public void Listen(int port)
        {
            // Implementation borrowed from: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services

            listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while(true)
                {
                    byte[] bytes = listener.Receive(ref groupEP);
                    this.addMessageToQueue(bytes);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Error occured while listening on UDP port:");
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }

        /// <summary>
        /// Checks if there are messages waiting to be read.
        /// </summary>
        /// <returns>If there are messages ready to be read from the UDP socket.</returns>
        public bool MessageIsReady()
        {
            return this.messages.Count > 0;
        }

        /// <summary>
        /// Gets the number of messages that are ready.
        /// </summary>
        /// <returns>Number of messages ready to be read.</returns>
        public int MessagesReady()
        {
            return this.messages.Count;
        }

        /// <summary>
        /// Gets the next message recieved from the message queue. This function is thread-safe. If no message is available, or if
        /// access to the queue is currently blocked, this function returns "";
        /// </summary>
        /// <returns>Returns the next unread message, or "" if there is none.</returns>
        public string GetNextMessage()
        {
            try
            {
                if(!this.MessageIsReady())
                {
                    throw new Exception("No new messages are ready.");
                }
                string result;
                bool didDequeue = this.messages.TryDequeue(out result);
                if (!didDequeue)
                {
                    throw new Exception("Unable to dequeue.");
                }
                return result;
            }
            catch (Exception e)
            {
                // Not able to pull from the queue, probably a concurrency problem. Try again later.
                throw new Exception("Unable to read message.", e);
            }
        }

        /// <summary>
        /// Cleans up the network connection and other artifacts that could be left behind.
        /// </summary>
        public void Cleanup()
        {
            listener.Close();
        }

        /// <summary>
        /// Internal function to add a value to the queue. Does not check if there is space on the queue, and may throw an exception
        /// if there are too many unread messages.
        /// </summary>
        /// <param name="payload">Raw byte payload from UDP socket.</param>
        private void addMessageToQueue(byte[] payload)
        {
            string message = Encoding.ASCII.GetString(payload, 0, payload.Length);
            this.messages.Enqueue(message);
        }
    }
}
