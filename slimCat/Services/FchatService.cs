﻿#region Copyright

// <copyright file="FchatService.cs">
//     Copyright (c) 2013-2015, Justin Kadrovach, All rights reserved.
// 
//     This source is subject to the Simplified BSD License.
//     Please see the License.txt file for more information.
//     All other rights reserved.
// 
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//     PARTICULAR PURPOSE.
// </copyright>

#endregion

namespace slimCat.Services
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Timers;
    using Microsoft.Practices.Prism.Events;
    using Models;
    using Newtonsoft.Json;
    using SimpleJson;
    using SuperSocket.ClientEngine;
    using Utilities;
    using WebSocket4Net;
    using static Utilities.Constants.Errors;
    using static Utilities.Constants.ServerCommands;

    #endregion

    public class FchatService : IHandleChatConnection, IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isManagedDispose)
        {
            socket.Close();
        }

        #region Fields

        private const int TimeoutTimeMs = 30*1000; // 30 seconds

        private readonly Timer autoPingTimer = new Timer(45*1000); // every 45 seconds

        private readonly int[] errsThatDisconnect;

        private readonly int[] errsThatPreventReconnect;

        // these are types of commands which produce a lot not useful noise in debug logs

        private readonly IEventAggregator events;

        private readonly string[] noisyTypes;

        private readonly IGetTickets service;

        private readonly Random random = new Random();

        private readonly Queue<KeyValuePair<string, object>> resendQueue = new Queue<KeyValuePair<string, object>>();

        private readonly Timer staggerTimer;

        private readonly Timer timeoutTimer;

        private bool isAuthenticated;

        private int retryAttemptCount;

        private WebSocket socket;

        private bool intentionallyDisconnected;

        #endregion

        #region Constructors

        public FchatService(IAccount user, IEventAggregator eventagg, WebSocket socket, IGetTickets service)
        {
            this.socket = socket;
            this.service = service;
            Account = user.ThrowIfNull("user");
            events = eventagg.ThrowIfNull("eventagg");

            events.GetEvent<CharacterSelectedLoginEvent>()
                .Subscribe(ConnectToChat, ThreadOption.BackgroundThread, true);

            errsThatDisconnect = new[]
            {
                NoLoginSlots,
                NoServerSlots,
                KickedFromServer,
                SimultaneousLoginKick,
                BannedFromServer,
                BadLoginInfo,
                TooManyConnections,
                UnknownLoginMethod,
                TimedOutFromServer
            };

            errsThatPreventReconnect = new[]
            {
                BannedFromServer,
                TooManyConnections,
                KickedFromServer,
                UnknownLoginMethod,
                SimultaneousLoginKick,
                TimedOutFromServer
            };

            noisyTypes = new[]
            {
                UserJoin,
                UserLeave,
                UserStatus,
                PublicChannelList,
                PrivateChannelList,
                UserList,
                ChannelMessage,
                ChannelAd
            };

            autoPingTimer.Elapsed += (s, e) => TrySend(Constants.ClientCommands.SystemPing);

            staggerTimer = new Timer(GetNextConnectDelay()); // first reconnect is 5 seconds
            staggerTimer.Elapsed += (s, e) => DoReconnect();

            timeoutTimer = new Timer(TimeoutTimeMs); // 30 seconds
            timeoutTimer.Elapsed += (s, e) => OnTimeout();
        }

        private void OnTimeout()
        {
            socket.Close();

            socket = new WebSocket(Account.ServerHost);

            DoReconnect();

            timeoutTimer.Stop();
        }

        #endregion

        #region Properties

        public IAccount Account { get; }

        public string Character { get; private set; }

        #endregion

        #region Methods

        public void SendMessage(object command, string type)
        {
            if (type.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(type), "Command type must be 3 characters long");

            var ser = JsonConvert.SerializeObject(command);

            TrySend(type, ser);
        }

        public void SendMessage(IDictionary<string, object> command)
        {
            var type = command.Get(Constants.Arguments.Type);

            command.Remove(Constants.Arguments.Type);

            var ser = JsonConvert.SerializeObject(command);

            TrySend(type, ser);
        }

        public void SendMessage(string commandType)
        {
            if (commandType.Length > 3 || commandType.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(commandType), "Command type must be 3 characters long");

            TrySend(commandType);
        }

        #endregion

        #region Connection Management

        public void Disconnect()
        {
            socket.Closed -= ConnectionClosed;
            socket.Error -= ConnectionError;
            socket.MessageReceived -= ConnectionMessageReceived;

            if (socket.State == WebSocketState.Open)
            {
                socket.Close();
            }
            else
            {
                socket = new WebSocket(Account.ServerHost);
            }

            events.GetEvent<ConnectionClosedEvent>().Publish(true);
            isAuthenticated = false;
            autoPingTimer.Stop();
            staggerTimer.Stop();
            timeoutTimer.Stop();
            autoPingTimer.Stop();
            retryAttemptCount = 0;
            intentionallyDisconnected = true;
        }

        #region logging

        [Conditional("DEBUG")]
        private void Log(string type, object payload = null, bool isSent = true)
        {
            Logging.Log(type + (payload != null ? " " + payload.GetHashCode() : ""),
                "chat {0}".FormatWith(isSent ? "OUT" : "IN"));

            if (!(noisyTypes.Contains(type) || (type == "IDN" && isSent)))
            {
                Logging.LogObject(payload);
            }

            Logging.Log();
        }

        #endregion

        private void ConnectToChat(string character)
        {
            if (character == null) return;
            Character = character;
            intentionallyDisconnected = false;

            if (socket.State == WebSocketState.Open) return;

            Logging.LogLine("opening socket", "chat");

            socket = new WebSocket(Account.ServerHost);

            // define socket behavior
            socket.Opened += ConnectionOpened;
            socket.Error += ConnectionError;
            socket.MessageReceived += ConnectionMessageReceived;
            socket.Closed += ConnectionClosed;

            // start connection
            socket.Open();
        }

        private void TrySend(string type, object args = null)
        {
            if (socket.State == WebSocketState.Open)
            {
                var s = args as string;
                if (s != null)
                {
                    var objArgs = SimpleJson.DeserializeObject(s);
                    Log(type, objArgs);
                }
                else
                    Log(type, args);

                if (args != null)
                    socket.Send(type + " " + args);
                else
                    socket.Send(type);

                return;
            }

            Log("requeuing " + type);
            resendQueue.Enqueue(new KeyValuePair<string, object>(type, args));

            if (socket.State != WebSocketState.Connecting)
                AttemptReconnect();
        }

        /// <summary>
        ///     When our connection was closed.
        /// </summary>
        private void ConnectionClosed(object s, EventArgs e)
        {
            Logging.LogLine("socket closed", "chat");

            if (!isAuthenticated)
            {
                events.GetEvent<LoginFailedEvent>().Publish("Server closed the connection");
                AttemptReconnect();
                return;
            }

            events.GetEvent<ConnectionClosedEvent>().Publish(false);
            AttemptReconnect();
            autoPingTimer.Stop();
        }

        /// <summary>
        ///     When we got something from the server!
        /// </summary>
        private void ConnectionMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!isAuthenticated)
            {
                isAuthenticated = true;
                events.GetEvent<LoginAuthenticatedEvent>().Publish(null);

                SendQueue();
                retryAttemptCount = 0;

                timeoutTimer.Stop();
                autoPingTimer.Start();
            }

            if (e.Message.Length <= 3)
            {
                events.GetEvent<ChatCommandEvent>().Publish(null);
                return;
            }

            var commandType = e.Message.Substring(0, 3); // type of command sent

            var message = e.Message; // actual arguments sent


            // if it has arguments...
            message = message.Remove(0, 4); // chop off the command type

            var json = (IDictionary<string, object>) SimpleJson.DeserializeObject(message);

            Log(commandType, json, false);

            // de-serialize it to an object model
            json.Add(Constants.Arguments.Command, commandType);

            // add back in the command type so our models can listen for them
            if (json.Get(Constants.Arguments.Command) == SystemError
                && json.ContainsKey("number"))
            {
                var err = Convert.ToInt32((long) json["number"]);

                if (errsThatDisconnect.Contains(err))
                {
                    isAuthenticated = false;
                    Disconnect();
                }

                if (errsThatPreventReconnect.Contains(err))
                {
                    Exceptions.ShowErrorBox(
                        "slimCat will now exit. \nReason: {0}".FormatWith(json.Get("message")),
                        "slimCat Fatal Error");

                    Environment.Exit(-1);
                }

                if (err == BadLoginInfo)
                {
                    service.ShouldGetNewTicket = true;
                    AttemptReconnect();
                }
            }

            events.GetEvent<ChatCommandEvent>().Publish(json);
        }

        /// <summary>
        ///     When something done goofed itself
        /// </summary>
        private void ConnectionError(object sender, ErrorEventArgs e)
        {
            Logging.LogLine("socket exception: " + e.Exception.Message, "chat");

            events.GetEvent<LoginFailedEvent>().Publish(e.Exception.Message);
            AttemptReconnect();
        }

        /// <summary>
        ///     When we have connection to F-chat.
        /// </summary>
        private void ConnectionOpened(object sender, EventArgs e)
        {
            Logging.LogLine("connection opened", "chat");

            // Handshake completed, send login command
            object authRequest =
                new
                {
                    ticket = service.Ticket,
                    method = "ticket",
                    account = service.Account.AccountName,
                    character = Character,
                    cname = Constants.ClientId,
                    cversion = $"{Constants.ClientNickname} {Constants.ClientVersion}"
                };

            SendMessage(authRequest, Constants.ClientCommands.SystemAuthenticate);

            staggerTimer.Stop();
        }

        /// <summary>
        ///     If our connection failed, try to reconnect
        /// </summary>
        private void AttemptReconnect()
        {
            if (staggerTimer.Enabled || socket.State == WebSocketState.Open || intentionallyDisconnected) return;

            staggerTimer.Start();
            isAuthenticated = false;

            events.GetEvent<ReconnectingEvent>().Publish((int) staggerTimer.Interval/1000);
            events.SendUserCommand("join", new[] {"home"});
            autoPingTimer.Stop();

            timeoutTimer.Interval = staggerTimer.Interval + (TimeoutTimeMs);
            timeoutTimer.Start();
        }

        private void DoReconnect()
        {
            Logging.LogLine("Attempting reconnect #" + (retryAttemptCount + 1), "chat");

            if (retryAttemptCount >= 21)
            {
                Exceptions.ShowErrorBox(
                    "slimCat will now exit. \nReason: Reconnect Attempts exhausted. \n\n"
                    + "Please wait a few minutes then restart the client.",
                    "Reconnect attempts exhausted!");

                Environment.Exit(-2);
            }

            ConnectToChat(Character);
            retryAttemptCount++;
            staggerTimer.Interval = GetNextConnectDelay();
            staggerTimer.Stop();
        }

        private void SendQueue()
        {
            // disable resend queue for now.
            // previous attempts could result in an infinite loop;
            // not really worth the small gain.
            resendQueue.Clear();
        }

        private int GetNextConnectDelay()
        {
            const int oneSecond = 1000;
            // first retry is 5 seconds
            if (retryAttemptCount == 0) return 5*oneSecond;

            // next 5 are between 5 and 15 seconds
            if (retryAttemptCount <= 5) return (random.Next(10) + 5)*oneSecond;

            // next 5 are between 25 and 35
            if (retryAttemptCount <= 10) return (random.Next(10) + 25)*oneSecond;

            // next 5 are between 60 and 70
            if (retryAttemptCount <= 15) return (random.Next(10) + 60)*oneSecond;

            // next 5 are between 300 and 360 (five and six minutes)
            if (retryAttemptCount <= 20) return (random.Next(60) + 300)*oneSecond;

            // we're not going to reconnect at this point
            return 60*60*oneSecond;
        }

        #endregion
    }
}