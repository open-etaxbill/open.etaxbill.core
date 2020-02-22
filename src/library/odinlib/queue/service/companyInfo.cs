using OdinSdk.OdinLib.Configuration;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public class CompanyInfo
    {
        /// <summary>
        ///
        /// </summary>
        public CompanyInfo()
            : this(null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 id</param>
        /// <param name="corporate_id">법인 ID</param>
        public CompanyInfo(string company_id = null, string corporate_id = null)
        {
            companyId = company_id ?? MyAppService.Value.companyInfo.companyId;
            corporateId = corporate_id ?? MyAppService.Value.companyInfo.corporateId;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 정보</param>
        public CompanyInfo(CompanyElement company_id)
        {
            companyId = company_id.companyId;
            corporateId = company_id.corporateId;
        }

        /// <summary>
        ///
        /// </summary>
        public string companyId
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string corporateId
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

            CompanyInfo p = (CompanyInfo)obj;
            return p.companyId == this.companyId && p.corporateId == this.corporateId;
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