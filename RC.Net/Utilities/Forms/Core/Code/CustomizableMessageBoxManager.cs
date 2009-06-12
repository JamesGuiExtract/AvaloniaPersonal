// Code from the following article by Sijin Joseph:
// http://www.codeproject.com/KB/dialog/CustomizableMessageBox.aspx
// It has been modified to meet our standards and changed slightly to better fit
// what we need it to do.  Removed the loading of string resources that would display
// the standard buttons in either English, German, or French depending on your locale.
// The buttons are now only in English.  Modified to use ExtractExceptions and to throw
// ExtractExceptions from all publicly visible properties and methods.  Modified to allow
// specifying which button should be considered the default button.
using System;
using System.IO;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities.Forms
{
	/// <summary>
	/// Manages a collection of MessageBoxes. Basically manages the
	/// saved response handling for messageBoxes.
	/// </summary>
	public static class CustomizableMessageBoxManager
	{
		#region Fields

        /// <summary>
        /// Stores the named message boxes so that they can be reused.
        /// </summary>
        private static Dictionary<string, CustomizableMessageBox> _messageBoxes = 
            new Dictionary<string, CustomizableMessageBox>();

        /// <summary>
        /// Stores the saved responses for each named message box.
        /// </summary>
        private static Dictionary<string, string> _savedResponses =
            new Dictionary<string, string>();

		#endregion

		#region Methods

		/// <summary>
		/// Creates a new message box with the specified name. If null is specified
		/// in the message name then the message box is not managed by the Manager and
		/// will be disposed automatically after a call to Show()
		/// </summary>
		/// <param name="name">The name of the message box. May not be null or empty string.
        /// Must be a unique name for the instance of the program creating it.</param>
		/// <returns>A new message box</returns>
        /// <exception cref="ExtractException">If <paramref name="name"/> is
        /// <see langword="null"/> or empty string.</exception>
        /// <exception cref="ExtractException">If <paramref name="name"/> is not
        /// unique.</exception>
		public static CustomizableMessageBox CreateMessageBox(string name)
		{
            try
            {
                ExtractException.Assert("ELI21631", "Parameter must not be null or empty string!",
                    !string.IsNullOrEmpty(name));

                if (_messageBoxes.ContainsKey(name))
                {
                    ExtractException ee = new ExtractException("ELI21689",
                        "The specified name for the new MessageBox already exists!");
                    ee.AddDebugData("MessageBox name", name, false);
                    throw ee;
                }

                CustomizableMessageBox msgBox = new CustomizableMessageBox();
                msgBox.Name = name;
                _messageBoxes[name] = msgBox;

                return msgBox;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21688", ex);
            }
		}

		/// <summary>
		/// Gets the message box with the specified name
		/// </summary>
		/// <param name="name">The name of the message box to retrieve</param>
		/// <returns>The message box with the specified name or <see langword="null"/>
        /// if a message box with that name does not exist</returns>
		public static CustomizableMessageBox GetMessageBox(string name)
		{
            try
            {
                CustomizableMessageBox messageBox = null;
                return _messageBoxes.TryGetValue(name, out messageBox) ? messageBox : null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21694", ex);
            }
		}

        /// <summary>
        /// Checks if a named <see cref="CustomizableMessageBox"/> with the specified name
        /// is currently being managed by the <see cref="CustomizableMessageBoxManager"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="CustomizableMessageBox"/>
        /// to look for.</param>
        /// <returns><see langword="true"/> if <paramref name="name"/> is currently
        /// being managerd by the <see cref="CustomizableMessageBoxManager"/></returns>
        public static bool ContainsMessageBox(string name)
        {
            try
            {
                if (name != null)
                {
                    return _messageBoxes.ContainsKey(name);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21633", ex);
            }
        }

		/// <summary>
		/// Deletes the <see cref="CustomizableMessageBox"/> with the specified name and
        /// calls its <see cref="CustomizableMessageBox.Dispose"/> method.
        /// <para><b>Note:</b></para>
        /// This will also remove the saved response (if any) for the specified
        /// <see cref="CustomizableMessageBox"/>.
		/// </summary>
		/// <param name="name">The name of the message box to delete</param>
		public static void DeleteMessageBox(string name)
		{
            try
            {
                if (name != null)
                {
                    // Try to find the message box from the collection
                    CustomizableMessageBox msgBox = null;
                    if (_messageBoxes.TryGetValue(name, out msgBox))
                    {
                        // Call dispose and remove it from the collection
                        msgBox.Dispose();
                        _messageBoxes.Remove(name);

                        // If there is a saved response for this message box, remove it as well
                        _savedResponses.Remove(name);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21695", ex);
                ee.AddDebugData("Message box name", name, false);
                throw ee;
            }
		}

        /// <summary>
        /// Deletes all saved <see cref="CustomizableMessageBox"/> objects, calling
        /// <see cref="CustomizableMessageBox.Dispose"/> for each one.
        /// <para><b>Note:</b></para>
        /// This will also clear all of the saved responses.
        /// </summary>
        public static void DeleteAllMessageBoxes()
        {
            try
            {
                // Clear and dispose of all the message boxes
                CollectionMethods.ClearAndDispose(_messageBoxes);

                // Clear the collection of saved responses
                _savedResponses.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21635", ex);
            }
        }

        /// <summary>
        /// This method is not implemented and will just return an exception.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the response to.</param>
        // This method may be implemented in the future to allow writing a saved response
        // to a given stream
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="stream")]
		public static void WriteSavedResponses(Stream stream)
		{
			throw new NotImplementedException("This feature has not yet been implemented.");
		}

        /// <summary>
        /// This method is not implemented and will just return an exception.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the saved response from.</param>
        // This method may be implemented in the future to allow reading a saved response
        // from a given stream
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="stream")]
		public static void ReadSavedResponses(Stream stream)
		{
			throw new NotImplementedException("This feature has not yet been implemented.");
		}

		/// <summary>
		/// Reset the saved response for the message box with the specified name.
		/// </summary>
		/// <param name="messageBoxName">The name of the message box whose
        /// response is to be reset.</param>
		public static void ResetSavedResponse(string messageBoxName)
		{
            try
            {
                if (messageBoxName != null)
                {
                    _savedResponses.Remove(messageBoxName);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21696", ex);
                ee.AddDebugData("Message box name", messageBoxName, false);
                throw ee;
            }
		}

		/// <summary>
		/// Resets the saved responses for all message boxes that are managed by the manager.
		/// </summary>
		public static void ResetAllSavedResponses()
		{
            try
            {
                _savedResponses.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21697", ex);
            }
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Set the saved response for the specified message box
		/// </summary>
		/// <param name="msgBox">The message box whose response is to be set</param>
		/// <param name="response">The response to save for the message box</param>
		internal static void SetSavedResponse(CustomizableMessageBox msgBox, string response)
		{
            if (msgBox.Name != null)
            {
                _savedResponses[msgBox.Name] = response;
            }
		}

		/// <summary>
		/// Gets the saved response for the specified message box
		/// </summary>
		/// <param name="msgBox">The message box whose saved response is to be retrieved</param>
		/// <returns>The saved response if exists, null otherwise</returns>
		internal static string GetSavedResponse(CustomizableMessageBox msgBox)
		{
            if (msgBox.Name != null)
            {
                string response = "";
                return _savedResponses.TryGetValue(msgBox.Name, out response) ? response : null;
            }
            else
            {
                return null;
            }
		}

		#endregion
	}
}
