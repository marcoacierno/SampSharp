﻿// SampSharp
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.Tools;
using SampSharp.GameMode.World;

namespace SampSharp.GameMode.Display
{
    /// <summary>
    ///     Represents a SA:MP dialog.
    /// </summary>
    public class Dialog : IDialog
    {
        private const int DialogId = 10000;
        private const int DialogHideId = -1;
        private static readonly Dictionary<int, Dialog> OpenDialogs = new Dictionary<int, Dialog>();

        private readonly ASyncWaiter<GtaPlayer, DialogResponseEventArgs> _aSyncWaiter =
            new ASyncWaiter<GtaPlayer, DialogResponseEventArgs>();

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the Dialog class.
        /// </summary>
        /// <param name="style">The style of the dialog.</param>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="message">The text to display in the main dialog. Use \n to start a new line and \t to tabulate.</param>
        /// <param name="button1">The text on the left button.</param>
        /// <param name="button2">The text on the right button. Leave it blank to hide it.</param>
        public Dialog(DialogStyle style, string caption, string message, string button1, string button2 = null)
        {
            if (caption == null) throw new ArgumentNullException("caption");
            if (message == null) throw new ArgumentNullException("message");
            if (button1 == null) throw new ArgumentNullException("button1");

            Style = style;
            Caption = caption;
            Message = message;
            Button1 = button1;
            Button2 = button2;
        }

        #endregion

        #region Properties of Dialog

        /// <summary>
        ///     Gets all opened dialogs.
        /// </summary>
        public static IEnumerable<Dialog> All
        {
            get { return OpenDialogs.Values; }
        }

        #endregion

        #region Methods of Dialog

        /// <summary>
        ///     Hides all dialogs for the specified <paramref name="player" />.
        /// </summary>
        /// <param name="player">The Player to hide all dialogs from.</param>
        public static void Hide(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            var openDialog = GetOpenDialog(player);

            if (openDialog == null) return;

            OpenDialogs.Remove(player.Id);

            openDialog._aSyncWaiter.Cancel(player);

            Native.ShowPlayerDialog(player.Id, DialogHideId, (int) DialogStyle.MessageBox, string.Empty,
                string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        ///     Gets the dialog currently being shown to the specified <paramref name="player" />.
        /// </summary>
        /// <param name="player">The player whose dialog to get.</param>
        /// <returns>The dialog currently being shown to the specified <paramref name="player" />.</returns>
        public static Dialog GetOpenDialog(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            return OpenDialogs.ContainsKey(player.Id) ? OpenDialogs[player.Id] : null;
        }

        #endregion

        #region Implementation of IDialog

        /// <summary>
        ///     Gets or sets the style.
        /// </summary>
        public virtual DialogStyle Style { get; set; }

        /// <summary>
        ///     Gets or sets the caption.
        /// </summary>
        /// <remarks>
        ///     The length of the caption can not exceed more than 64 characters before it
        ///     starts to cut off.
        /// </remarks>
        public virtual string Caption { get; set; }

        /// <summary>
        ///     Gets the message displayed.
        /// </summary>
        public virtual string Message { get; private set; }

        /// <summary>
        ///     Gets or sets the text on the left button.
        /// </summary>
        public virtual string Button1 { get; set; }

        /// <summary>
        ///     Gets or sets the text on the right button.
        /// </summary>
        /// <remarks>
        ///     Leave it blank to hide it.
        /// </remarks>
        public virtual string Button2 { get; set; }

        /// <summary>
        ///     Occurs when a player responds to a dialog by either clicking a button, pressing ENTER/ESC or double-clicking a list
        ///     item.
        /// </summary>
        public event EventHandler<DialogResponseEventArgs> Response;

        /// <summary>
        ///     Shows the dialog box to a Player.
        /// </summary>
        /// <param name="player">The Player to show the dialog to.</param>
        public void Show(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            // Hide previously opened dialogs.
            Hide(player);

            // Store this dialog as the opened dialog.
            OpenDialogs[player.Id] = this;


            // Show the dialog to the player.
            Native.ShowPlayerDialog(player.Id, DialogId, (int) Style, Caption, Message, Button1,
                Button2 ?? string.Empty);
        }

        /// <summary>
        ///     Shows the dialog box to a Player asynchronously.
        /// </summary>
        /// <param name="player">The Player to show the dialog to.</param>
        public async Task<DialogResponseEventArgs> ShowAsync(GtaPlayer player)
        {
            Show(player);

            return await _aSyncWaiter.Result(player);
        }

        /// <summary>
        ///     Raises the <see cref="IDialog.Response" /> event.
        /// </summary>
        /// <param name="e">An <see cref="DialogResponseEventArgs" /> that contains the event data. </param>
        public void OnResponse(DialogResponseEventArgs e)
        {
            if (OpenDialogs.ContainsKey(e.Player.Id))
                OpenDialogs.Remove(e.Player.Id);

            _aSyncWaiter.Fire(e.Player, e);

            if (Response != null)
                Response(this, e);
        }

        #endregion
    }
}