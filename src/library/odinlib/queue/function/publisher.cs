/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Messaging;

namespace OdinSdk.OdinLib.Queue.Function
{
    /// <summary>
    ///
    /// </summary>
    public class Publisher
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_path"></param>
        /// <returns></returns>
        public static bool IsRemoteQueuePath(string queue_path)
        {
            return queue_path.Contains("FormatName:Direct=");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="queue_name"></param>
        /// <returns></returns>
        public static string GetRemoteQueuePath(string protocol, string ip_address, string queue_name)
        {
            return String.Format(@"FormatName:Direct={0}:{1}\private$\{2}", protocol, ip_address, queue_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_path"></param>
        /// <returns></returns>
        public static MessageQueue CreateServiceQueue(string queue_path)
        {
            MessageQueue _result = null;

            if (IsRemoteQueuePath(queue_path) == false && MessageQueue.Exists(queue_path) == false)
            {
                _result = MessageQueue.Create(queue_path, true);
                _result.SetPermissions("Administrators", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            }
            else
            {
                _result = new MessageQueue(queue_path);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_path"></param>
        /// <returns></returns>
        public MessageQueue OpenQueue(string queue_path)
        {
            MessageQueue _result = null;

            try
            {
                _result = CreateServiceQueue(queue_path);

                // Enable the AppSpecific field in the messages.
                _result.MessageReadPropertyFilter.AppSpecific = true;

                // Set the formatter to binary.
                //_result.Formatter = new BinaryMessageFormatter();

                // Set the formatter to Xml.
                _result.Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) });
            }
            catch (MessageQueueException ex)
            {
                throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
            }
            catch (Exception)
            {
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message_queue"></param>
        /// <param name="message"></param>
        /// <param name="label"></param>
        /// <param name="app_specific"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool WriteQueue(MessageQueue message_queue, object message, string label, int app_specific, MessagePriority priority)
        {
            var _result = false;

            //if (message_queue.Transactional == true)
            {
                using (MessageQueueTransaction _qTransaction = new MessageQueueTransaction())
                {
                    _qTransaction.Begin();

                    try
                    {
                        using (Message _message = new Message
                        {
                            Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) }),
                            //Formatter = new BinaryMessageFormatter(),
                            Body = message,
                            Label = label,
                            AppSpecific = app_specific,
                            Priority = priority
                        })
                        {
                            message_queue.Send(_message, _qTransaction);
                        }
                        _qTransaction.Commit();

                        _result = true;
                    }
                    catch (MessageQueueException ex)
                    {
                        _qTransaction.Abort();
                        throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}