﻿#region Copyright

// <copyright file="ChannelBarViewModelCommon.cs">
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

namespace slimCat.ViewModels
{
    #region Usings

    using Models;
    using Services;

    #endregion

    /// <summary>
    ///     Contains some things the channelbar viewmodels have in common
    /// </summary>
    public class ChannelbarViewModelCommon : ViewModelBase
    {
        #region Constructors and Destructors

        protected ChannelbarViewModelCommon(IChatState chatState)
            : base(chatState)
        {
            ChatModel.SelectedChannelChanged += (s, e) => OnPropertyChanged("HasUsers");
        }

        #endregion

        #region Fields

        #endregion

        #region Public Properties

        public bool HasUsers
        {
            get
            {
                if (ChatModel.CurrentChannel == null)
                    return false;

                return (ChatModel.CurrentChannel.Type != ChannelType.PrivateMessage)
                       && ChatModel.CurrentChannel.Type != ChannelType.Utility;
            }
        }

        public GenericSearchSettingsModel SearchSettings { get; } = new GenericSearchSettingsModel();

        #endregion
    }
}