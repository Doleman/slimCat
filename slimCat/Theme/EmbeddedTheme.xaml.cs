﻿#region Copyright

// <copyright file="EmbeddedTheme.xaml.cs">
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

namespace slimCat.Theme
{
    #region Usings

    using System.Windows.Controls;
    using System.Windows.Input;
    using Utilities;

    #endregion

    public partial class EmbeddedTheme
    {
        private void OnMouseRightButtonUpForIcon(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            sender.TryOpenRightClickMenuCommand<Grid>(1);
        }

        private void OnMouseRightButtonUpForName(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            sender.TryOpenRightClickMenuCommand<Grid>(2);
        }
    }
}