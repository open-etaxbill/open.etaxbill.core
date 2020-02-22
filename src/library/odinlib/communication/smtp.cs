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
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace OdinSdk.OdinLib.Communication
{
    /// <summary>
	///
	/// </summary>
	public class Smtp
    {
        //-------------------------------------------------------------------------------------------------------------------------

        #region SMTP 메일 발송 함수

        //-------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 메일발송하는 함수
        /// </summary>
        /// <param name="mail_server">SMTP 서버</param>
        /// <param name="message">MailMessage 생성자</param>
        /// <param name="delay">연속 발송 대비 딜레이</param>
        /// <returns>성공/실패 bool</returns>
        public static bool SendMail(string mail_server, MailMessage message, int delay = 0)
        {
            var _result = false;

            try
            {
                using (SmtpClient _mailClient = new SmtpClient(mail_server)
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                })
                {
                    _mailClient.Send(message);
                    if (delay > 0)
                        Thread.Sleep(delay);
                }

                _result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _result;
        }

        #endregion SMTP 메일 발송 함수
    }
}