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
using System.Security.Cryptography.X509Certificates;

namespace OdinSdk.eTaxBill.Security.Notice
{
    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    ///
    /// </summary>
    public struct Party
    {
        private string m_bizId;
        private string m_bizName;
        private string m_kecRegId;

        /// <summary>
        /// 사업자 등록번호
        /// </summary>
        public string ID
        {
            get
            {
                return m_bizId;
            }
            set
            {
                m_bizId = value;
            }
        }

        /// <summary>
        /// 사업자 명
        /// </summary>
        public string Name
        {
            get
            {
                return m_bizName;
            }
            set
            {
                m_bizName = value;
            }
        }

        /// <summary>
        /// 국세청 등록 번호
        /// </summary>
        public string KecRegId
        {
            get
            {
                return m_kecRegId;
            }
            set
            {
                m_kecRegId = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">사업자 등록번호</param>
        /// <param name="name">사업자 명</param>
        public Party(string id, string name)
        {
            m_bizId = id;
            m_bizName = name;
            m_kecRegId = "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">사업자 등록번호</param>
        /// <param name="name">사업자 명</param>
        /// <param name="regId">국세청 등록 번호</param>
        public Party(string id, string name, string regId)
        {
            m_bizId = id;
            m_bizName = name;
            m_kecRegId = regId;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    ///
    /// </summary>
    public class Header
    {
        /// <summary>
        /// 응답 메시지 여부
        /// </summary>
        public bool IsRecv
        {
            get;
            set;
        }

        /// <summary>
        /// MessageID
        /// </summary>
        public string MessageId
        {
            get;
            set;
        }

        /// <summary>
        /// 응답 메시지인 경우 MessageID
        /// </summary>
        public string RelatesTo
        {
            get;
            set;
        }

        /// <summary>
        /// 메시지 수신자의 주소정보
        /// </summary>
        public string ToAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 전송 메시지 유형별로 정의 된 Action 값
        /// </summary>
        public string Action
        {
            get;
            set;
        }

        /// <summary>
        /// 전자세금계산서 통신규약 버전
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// 메시지 전송 사업자의 정보
        /// </summary>
        public Party FromParty
        {
            get;
            set;
        }

        /// <summary>
        /// 메시지 수신 사업자의 정보
        /// </summary>
        public Party ToParty
        {
            get;
            set;
        }

        /// <summary>
        /// 전자세금계산서 처리 결과를 비동기식으로 받을 사업자의 메시지 수신 Endpoint
        /// 사업자가 전자세금계산서를 국세청에 전송할 때는 반드시 이 정보를 기술하여야 함
        /// </summary>
        public string ReplyTo
        {
            get;
            set;
        }

        /// <summary>
        /// Operation Type
        /// </summary>
        public string OperationType
        {
            get;
            set;
        }

        /// <summary>
        /// Message Type
        /// </summary>
        public string MessageType
        {
            get;
            set;
        }

        private DateTime m_timeStamp = DateTime.MinValue;

        /// <summary>
        ///
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                if (m_timeStamp == DateTime.MinValue)
                    m_timeStamp = DateTime.Now;
                return m_timeStamp;
            }
            set
            {
                m_timeStamp = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public X509Certificate2 X509Cert2
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public byte[] DigestValue1
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public byte[] DigestValue2
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public byte[] SignatureValue
        {
            get;
            set;
        }

        //public string ReferenceId
        //{
        //    get;
        //    set;
        //}
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    ///
    /// </summary>
    public class Body
    {
        #region *** Properties ***

        /// <summary>
        /// SOAP Fault 메시지에서 Fault를 식별하는 코드
        /// </summary>
        public string FaultCode
        {
            get;
            set;
        }

        /// <summary>
        /// SOAP Fault 메시지에서 Fault를 설명하는 내용
        /// </summary>
        public string FaultString
        {
            get;
            set;
        }

        /// <summary>
        /// 전자세금계산서 제출 시 전체 프로세스를 구분하기 위한 ID
        /// </summary>
        public string SubmitID
        {
            get;
            set;
        }

        /// <summary>
        /// 첨부 MIME Header의 Content-ID 값
        /// </summary>
        public string ReferenceID
        {
            get;
            set;
        }

        /// <summary>
        /// 첨부 전자세금계산서의 총 개수
        /// </summary>
        public int TotalCount
        {
            get;
            set;
        }

        /// <summary>
        /// 사업자가 전자세금계산서를 제출했을 때 설정한 SubmitID 값
        /// </summary>
        public string RefSubmitID
        {
            get;
            set;
        }

        /// <summary>
        /// 처리결과 전송 시 처리결과 송수신을 구분하기 위한 ID
        /// </summary>
        public string ResultID
        {
            get;
            set;
        }

        /// <summary>
        /// 국세청이 처리결과를 보냈을 떄 설정한 ResultID 값
        /// </summary>
        public string RefResultID
        {
            get;
            set;
        }

        /// <summary>
        /// 공개키 획득을 위한 사업자 정보
        /// </summary>
        public Party RequestParty
        {
            get;
            set;
        }

        /// <summary>
        /// 공개키 첨부 방식을 정의
        /// </summary>
        public string FileType
        {
            get;
            set;
        }

        #endregion *** Properties ***
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//
}