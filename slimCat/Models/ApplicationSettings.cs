﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationSettings.cs">
//    Copyright (c) 2013, Justin Kadrovach, All rights reserved.
//   
//    This source is subject to the Simplified BSD License.
//    Please see the License.txt file for more information.
//    All other rights reserved.
//    
//    THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//    PARTICULAR PURPOSE.
// </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

namespace Slimcat.Models
{
    #region Usings

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    #endregion

    /// <summary>
    ///     Settings for the entire application
    /// </summary>
    public static class ApplicationSettings
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="ApplicationSettings" /> class.
        /// </summary>
        static ApplicationSettings()
        {
            Volume = 1; // 0.5 sometimes crashes people for some reason beyond me
            ShowNotificationsGlobal = true;
            AllowLogging = true;

            BackLogMax = 125;
            GlobalNotifyTerms = string.Empty;
            SavedChannels = new List<string>();
            Interested = new List<string>();
            NotInterested = new List<string>();

            Langauge = Thread.CurrentThread.CurrentCulture.Name;
            if (!LanguageList.Contains(Langauge))
                Langauge = "en";

            SpellCheckEnabled = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether allow logging.
        /// </summary>
        public static bool AllowLogging { get; set; }

        /// <summary>
        ///     Gets or sets a value indiciating whether to allow user-inputted colors to be displayed.
        /// </summary>
        public static bool AllowColors { get; set; }

        /// <summary>
        ///     Gets or sets the back log max.
        /// </summary>
        public static int BackLogMax { get; set; }

        /// <summary>
        ///     Gets or sets the global notify terms.
        /// </summary>
        public static string GlobalNotifyTerms { get; set; }

        /// <summary>
        ///     Gets the global notify terms list.
        /// </summary>
        public static IEnumerable<string> GlobalNotifyTermsList
        {
            get { return GlobalNotifyTerms.Split(',').Select(word => word.ToLower()); }
        }

        public static bool FriendsAreAccountWide { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show notifications global.
        /// </summary>
        public static bool ShowNotificationsGlobal { get; set; }

        /// <summary>
        ///     Gets or sets the volume.
        /// </summary>
        public static double Volume { get; set; }

        /// <summary>
        ///     Gets the list of characters interesting to this user.
        /// </summary>
        public static IList<string> Interested { get; private set; }

        /// <summary>
        ///     Gets the list of channels saved to our user .
        /// </summary>
        public static IList<string> SavedChannels { get; private set; }

        /// <summary>
        ///     Gets the list of characters not interesting to this user.
        /// </summary>
        public static IList<string> NotInterested { get; private set; }

        // Localization

        /// <summary>
        ///     Gets or sets a value indicate whether to spell check the users' input.
        /// </summary>
        public static bool SpellCheckEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating the users' language.
        /// </summary>
        public static string Langauge { get; set; }

        public static IEnumerable<string> LanguageList
        {
            get { return new[] {"en-US", "en-GB", "de", "es", "fr"}; }
        }

        #endregion
    }
}