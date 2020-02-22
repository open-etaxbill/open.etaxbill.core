using OdinSdk.OdinLib.Configuration;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public class HostInfo
    {
        /// <summary>
        ///
        /// </summary>
        public HostInfo()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="product_id">제품 ID</param>
        /// <param name="domain"></param>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="mac_address"></param>
        public HostInfo(string product_id = null, string domain = null, string host_name = null, string ip_address = null, string mac_address = null)
        {
            this.productId = product_id ?? MyAppService.Value.productInfo.id;

            this.domain = domain ?? CfgHelper.SNG.UserDomainName;
            this.hostName = host_name ?? CfgHelper.SNG.MachineName;

            this.ipAddress = ip_address ?? CfgHelper.SNG.IPAddress;
            this.macAddress = mac_address ?? CfgHelper.SNG.MacAddress;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_element"></param>
        public HostInfo(HostElement host_element)
        {
            productId = host_element.productId;

            domain = host_element.domain;
            hostName = host_element.hostName;

            ipAddress = host_element.ipAddress;
            macAddress = host_element.macAddress;
        }

        /// <summary>
        ///
        /// </summary>
        public string productId
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string domain
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string hostName
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ipAddress
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string macAddress
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_info"></param>
        /// <returns></returns>
        public bool IsSameHost(HostInfo host_info)
        {
            return this.IsSameHost(host_info.hostName, host_info.productId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="product_id">제품 ID</param>
        /// <returns></returns>
        public bool IsSameHost(string host_name, string product_id)
        {
            return this.hostName == host_name && this.productId == product_id;
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

            HostInfo h = (HostInfo)obj;
            return h.productId == this.productId
                && h.domain == this.domain && h.hostName == this.hostName
                && h.ipAddress == this.ipAddress && h.macAddress == this.macAddress;
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