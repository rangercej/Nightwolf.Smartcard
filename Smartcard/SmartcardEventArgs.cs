namespace Nightwolf.Smartcard
{
    using System;

    /// <summary>
    /// Smartcard event parameters
    /// </summary>
    /// <inheritdoc cref="EventArgs"/>
    public sealed class SmartcardEventArgs : EventArgs
    {
        /// <summary>
        /// Smartcard associated with the event
        /// </summary>
        public readonly Smartcard SmartCard;

        /// <summary>
        /// Smartcard reader that trigged the event
        /// </summary>
        public readonly string ReaderName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartcardEventArgs"/> class. 
        /// </summary>
        /// <param name="scard">Smartcard associated with the event</param>
        /// <param name="reader">Smartcard reader that triggered the event</param>
        /// <inheritdoc cref="EventArgs()"/>
        public SmartcardEventArgs(Smartcard scard, string reader)
        {
            this.SmartCard = scard;
            this.ReaderName = reader;
        }
    }
}
