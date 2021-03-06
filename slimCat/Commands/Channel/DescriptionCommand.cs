﻿#region Copyright

// <copyright file="DescriptionCommand.cs">
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

namespace slimCat.Models
{
    #region Usings

    using Utilities;

    #endregion

    public class ChannelDescriptionChangedEventArgs : ChannelUpdateEventArgs
    {
        public override string ToString()
        {
            return "{0} has a new description.".FormatWith(GetChannelBbCode());
        }
    }
}

namespace slimCat.Services
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Windows;
    using Models;
    using Utilities;

    #endregion

    public partial class ServerCommandService
    {
        private void ChannelDescriptionCommand(IDictionary<string, object> command)
        {
            var channel = FindChannel(command);
            var description = command.Get("description");

            if (channel == null)
            {
                RequeueCommand(command);
                return;
            }

            var isInitializer = string.IsNullOrWhiteSpace(channel.Description);

            if (string.Equals(channel.Description, description, StringComparison.Ordinal))
                return;

            channel.Description = WebUtility.HtmlDecode(WebUtility.HtmlDecode(description));

            if (isInitializer)
                return;

            var args = new ChannelDescriptionChangedEventArgs();
            Events.NewChannelUpdate(channel, args);
        }
    }

    public partial class UserCommandService
    {
        private void OnChannelDescriptionRequested(IDictionary<string, object> command)
        {
            if (cm.CurrentChannel.Id.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                events.NewError("Poor home channel, with no description to speak of...");
                return;
            }

            var channel = cm.CurrentChannel as GeneralChannelModel;
            if (channel != null)
            {
                Clipboard.SetData(DataFormats.Text, channel.Description);
                events.NewError("Channel's description copied to clipboard.");
            }
            else
                events.NewError("Hey! That's not a channel.");
        }
    }
}