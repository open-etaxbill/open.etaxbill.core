namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    /// 서비스의 정의는
    ///  - 개발 되어진 제품(product)이
    ///  - 필요로 하는 사용자(company)에 공급되어
    ///  - 특정 서버(server)에서
    /// 실행 되고 있는 상태을 의미 한다.
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        ///
        /// </summary>
        public ServiceInfo()
            : this(null, null, false, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 정보</param>
        /// <param name="server_info">서버 정보</param>
        /// <param name="l4_using">로드 밸런스용 스위치 사용 유무</param>
        /// <param name="switch">스위치 호스트 정보</param>
        public ServiceInfo(CompanyInfo company_id = null, ServerInfo server_info = null, bool? l4_using = null, HostInfo host_info = null)
        {
            company = company_id ?? new CompanyInfo();
            server = server_info ?? new ServerInfo();

            L4Using = l4_using ?? false;
            L4Switch = host_info ?? new HostInfo("", "", "", "", "");
        }

        /// <summary>
        /// 사용자 정보
        /// </summary>
        public CompanyInfo company
        {
            get;
            set;
        }

        /// <summary>
        /// 서버 정보
        /// </summary>
        public ServerInfo server
        {
            get;
            set;
        }

        /// <summary>
        /// 이 값이 true 인 경우 permit-service 알림 요청이 있는 경우
        /// server의 ipAddress가 아닌 L4Switch의 IpAddress를 리턴한다
        /// </summary>
        public bool L4Using
        {
            get;
            set;
        }

        /// <summary>
        /// host infor for load-balancing
        /// </summary>
        public HostInfo L4Switch
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service_info">서비스 정보</param>
        /// <returns></returns>
        public bool IsSameService(ServiceInfo service_info)
        {
            return this.IsSameService(
                    service_info.company.companyId, service_info.company.corporateId,
                    service_info.server.host.hostName, service_info.server.product.productId
                );
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 id</param>
        /// <param name="corporate_id">법인 ID</param>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="product_id">제품 ID</param>
        /// <returns></returns>
        public bool IsSameService(string company_id, string corporate_id, string host_name, string product_id)
        {
            return this.company.companyId == company_id && this.company.corporateId == corporate_id
                    && this.server.host.hostName == host_name && this.server.product.productId == product_id;
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

            ServiceInfo p = (ServiceInfo)obj;
            return p.company.Equals(this.company) && p.server.Equals(this.server) && p.L4Switch.Equals(this.L4Switch);
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