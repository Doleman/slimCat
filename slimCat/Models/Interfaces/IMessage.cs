﻿namespace Slimcat.Models
{
    using System;

    /// <summary>
    ///     The Message interface.
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
        #endregion
    }
}