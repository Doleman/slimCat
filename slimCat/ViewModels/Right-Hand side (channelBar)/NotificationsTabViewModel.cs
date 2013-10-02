﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationsTabViewModel.cs" company="Justin Kadrovach">
//   Copyright (c) 2013, Justin Kadrovach
//   All rights reserved.
//   
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions are met:
//       * Redistributions of source code must retain the above copyright
//         notice, this list of conditions and the following disclaimer.
//       * Redistributions in binary form must reproduce the above copyright
//         notice, this list of conditions and the following disclaimer in the
//         documentation and/or other materials provided with the distribution.
//   
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//   WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//   DISCLAIMED. IN NO EVENT SHALL JUSTIN KADROVACH BE LIABLE FOR ANY
//   DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//   LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//   ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <summary>
//   This is the tab labled "notifications" in the channel bar, or the bar on the right-hand side
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Slimcat.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Events;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.Unity;

    using Slimcat.Libraries;
    using Slimcat.Models;
    using Slimcat.Utilities;
    using Slimcat.Views;

    /// <summary>
    ///     This is the tab labled "notifications" in the channel bar, or the bar on the right-hand side
    /// </summary>
// ReSharper disable ClassNeverInstantiated.Global
    public class NotificationsTabViewModel : ChannelbarViewModelCommon
// ReSharper restore ClassNeverInstantiated.Global
    {
        #region Constants

        /// <summary>
        ///     The notifications tab view.
        /// </summary>
        public const string NotificationsTabView = "NotificationsTabView";

        #endregion

        #region Fields
        private readonly FilteredCollection<NotificationModel, IViewableObject> notificationManager; 

        private RelayCommand clearNoti;

        private bool isSelected;

        private RelayCommand killNoti;

        private string search = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsTabViewModel"/> class.
        /// </summary>
        /// <param name="cm">
        /// The cm.
        /// </param>
        /// <param name="contain">
        /// The contain.
        /// </param>
        /// <param name="regman">
        /// The regman.
        /// </param>
        /// <param name="eventagg">
        /// The eventagg.
        /// </param>
        public NotificationsTabViewModel(
            IChatModel cm, IUnityContainer contain, IRegionManager regman, IEventAggregator eventagg)
            : base(contain, regman, eventagg, cm)
        {
            this.Container.RegisterType<object, NotificationsTabView>(NotificationsTabView);

            this.notificationManager =
                new FilteredCollection<NotificationModel, IViewableObject>(
                    this.ChatModel.Notifications, this.MeetsFilter, true);

            this.notificationManager.Collection.CollectionChanged += (sender, args) => 
            { 
                this.OnPropertyChanged("HasNoNotifications");
                this.OnPropertyChanged("NeedsAttention");
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the clear notifications command.
        /// </summary>
        public ICommand ClearNotificationsCommand
        {
            get
            {
                return this.clearNoti
                       ?? (this.clearNoti = new RelayCommand(args => this.ChatModel.Notifications.Clear()));
            }
        }

        // removed useless code which kept unread count of notifications

        /// <summary>
        ///     Gets a value indicating whether has no notifications.
        /// </summary>
        public bool HasNoNotifications
        {
            get
            {
                return this.notificationManager.Collection.Count == 0;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.OnPropertyChanged("NeedsAttention");
            }
        }

        /// <summary>
        ///     Gets the remove notification command.
        /// </summary>
        public ICommand RemoveNotificationCommand
        {
            get
            {
                return this.killNoti ?? (this.killNoti = new RelayCommand(this.RemoveNotification));
            }
        }

        /// <summary>
        ///     Gets or sets the search string.
        /// </summary>
        public string SearchString
        {
            get
            {
                return this.search.ToLower();
            }

            set
            {
                this.search = value;
                this.OnPropertyChanged("SearchString");
                this.notificationManager.RebuildItems();
            }
        }

        /// <summary>
        ///     Gets the sorted notifications.
        /// </summary>
        public ObservableCollection<IViewableObject> CurrentNotifications
        {
            get
            {
                return this.notificationManager.Collection;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The remove notification.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public void RemoveNotification(object args)
        {
            var model = args as NotificationModel;
            if (model != null)
            {
                this.ChatModel.Notifications.Remove(model);
            }
        }

        #endregion

        #region Methods
        private bool MeetsFilter(NotificationModel item)
        {
            return item.ToString().ContainsOrdinal(this.search);
        }
        #endregion
    }
}