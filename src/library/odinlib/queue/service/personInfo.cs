using OdinSdk.OdinLib.Configuration;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public class PersonInfo
    {
        /// <summary>
        ///
        /// </summary>
        public PersonInfo()
            : this(null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 정보</param>
        /// <param name="login_id"></param>
        public PersonInfo(CompanyInfo company_id = null, string login_id = null)
        {
            company = company_id ?? new CompanyInfo();
            loginId = login_id ?? CfgHelper.SNG.UserName;
        }

        /// <summary>
        /// 회사정보
        /// </summary>
        public CompanyInfo company
        {
            get;
            set;
        }

        /// <summary>
        /// 사용자 ID
        /// </summary>
        public string loginId
        {
            get;
            set;
        }

        /// <summary>
        /// 성명
        /// </summary>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// 사원증 번호 또는 ID
        /// </summary>
        public string employeeId
        {
            get;
            set;
        }

        /// <summary>
        /// rfid 사원 카드 일련번호
        /// </summary>
        public string cardId
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            PersonInfo p = (PersonInfo)obj;
            return p.company.Equals(this.company) && p.loginId == this.loginId;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}