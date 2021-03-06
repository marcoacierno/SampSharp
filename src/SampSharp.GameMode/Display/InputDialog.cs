using System;
using SampSharp.GameMode.Definitions;

namespace SampSharp.GameMode.Display
{
    public class InputDialog : Dialog
    {
        private string _message;

        /// <summary>
        ///     Initializes a new instance of the Dialog class.
        /// </summary>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="message">The text to display in the main dialog. Use \n to start a new line and \t to tabulate.</param>
        /// <param name="isPassword">if set to <c>true</c> the input will be hidden on the user's screen.</param>
        /// <param name="button1">The text on the left button.</param>
        /// <param name="button2">The text on the right button. Leave it blank to hide it.</param>
        public InputDialog(string caption, string message, bool isPassword, string button1, string button2 = null)
            : base(isPassword ? DialogStyle.Password : DialogStyle.Input, caption, message, button1, button2)
        {
            _message = message;
        }

        #region Overrides of Dialog

        /// <summary>
        ///     Gets the message displayed.
        /// </summary>
        public override string Message
        {
            get { return _message; }
        }

        #endregion

        public void SetMessage(string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            _message = value;
        }
    }
}