﻿#region Copyright

// <copyright file="IMessage.cs">
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

    using System;

    #endregion

    /// <summary>
    ///     Represents data for F-chat messages.
    /// </summary>
    public interface IMessage : IDisposable, IViewableObject
    {
        #region Public Properties

        /// <summary>
        ///     Gets the message.
        /// </summary>
        string Message { get; }

        /// <summary>
        ///     Gets the posted time.
        /// </summary>
        DateTimeOffset PostedTime { get; }

        /// <summary>
        ///     Gets the poster.
        /// </summary>
        ICharacter Poster { get; }

        /// <summary>
        ///     Gets the time stamp.
        /// </summary>
        string TimeStamp { get; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        MessageType Type { get; }

        /// <summary>
        ///     Gets if the message is a history message.
        /// </summary>
        bool IsHistoryMessage { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this message is of interest to our user.
        /// </summary>
        bool IsOfInterest { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this message was the last viewed in the channel.
        /// </summary>
        bool IsLastViewed { get; set; }

        #endregion
    }
}