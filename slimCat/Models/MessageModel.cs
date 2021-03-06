﻿#region Copyright

// <copyright file="MessageModel.cs">
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
    using System.Windows.Documents;
    using Views;

    #endregion

    /// <summary>
    ///     A model to hold data on messages
    /// </summary>
    public class MessageModel : MessageBase, IMessage
    {
        private bool isLastViewed;
        private bool isOfInterest;

        #region Methods

        protected override void Dispose(bool isManaged)
        {
            if (!isManaged)
                return;

            Poster = null;
        }

        #endregion

        #region Constructors and Destructors

        public MessageModel(ICharacter poster, string message, MessageType type = MessageType.Normal)
        {
            Poster = poster;
            Message = message;
            Type = type;
        }


        public MessageModel(string message)
        {
            Poster = new CharacterModel {Name = string.Empty};
            IsHistoryMessage = true;
            Type = message.StartsWith("[")
                ? MessageType.Normal
                : MessageType.Ad;

            Message = message;
        }

        public MessageModel(ICharacter poster, string message, DateTimeOffset posted)
            : base(posted)
        {
            Poster = poster;
            Message = message;
            Type = MessageType.Normal;
        }

        #endregion

        #region Public Properties

        public string Message { get; }

        public ICharacter Poster { get; set; }

        public MessageType Type { get; }

        public bool IsHistoryMessage { get; }

        public bool IsOfInterest
        {
            get { return isOfInterest; }
            set
            {
                isOfInterest = value;
                OnPropertyChanged(string.Empty);
            }
        }

        public bool IsLastViewed
        {
            get { return isLastViewed; }
            set
            {
                isLastViewed = value;
                OnPropertyChanged(string.Empty);
            }
        }

        public MessageModel This => this;

        public Block View
        {
            get
            {
                if (IsHistoryMessage)
                    return new HistoryView {DataContext = Message};

                return new MessageView {DataContext = this};
            }
        }

        #endregion
    }
}