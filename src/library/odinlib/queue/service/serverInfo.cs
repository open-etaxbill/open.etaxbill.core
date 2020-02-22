using OdinSdk.OdinLib.Configuration;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    /// 서버의 정의는
    ///  - 개발 되어진 제품(product)이
    ///  - 특정 호스트(host)에서
    /// 설치 되어 있는 상태을 의미 한다.
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        ///
        /// </summary>
        public ServerInfo()
            : this((HostInfo)null, (ProductInfo)null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_info"></param>
        /// <param name="product_info"></param>
        public ServerInfo(HostInfo host_info = null, ProductInfo product_info = null)
        {
            host = host_info ?? new HostInfo();
            product = product_info ?? new ProductInfo();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_info"></param>
        /// <param name="product_info"></param>
        public ServerInfo(HostElement host_info, ProductElement product_info)
        {
            host = new HostInfo(host_info);
            product = new ProductInfo(product_info);
        }

        /// <summary>
        /// 서버 정보
        /// </summary>
        public HostInfo host
        {
            get;
            set;
        }

        /// <summary>
        /// 제품 정보
        /// </summary>
        public ProductInfo product
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="server_info">서버 정보</param>
        /// <returns></returns>
        public bool IsSameServer(ServerInfo server_info)
        {
            return this.IsSameServer(server_info.host.hostName, server_info.product.productId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="product_id">제품 ID</param>
        /// <returns></returns>
        public bool IsSameServer(string host_name, string product_id)
        {
            return this.host.hostName == host_name && this.product.productId == product_id;
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

            ServerInfo p = (ServerInfo)obj;
            return p.host.Equals(this.host) && p.product.Equals(this.product);
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