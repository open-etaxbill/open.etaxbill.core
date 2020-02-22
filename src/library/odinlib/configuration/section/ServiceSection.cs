using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// Define a custom section containing an individual element and a collection of elements.
    /// </summary>
    public class ServiceSection : ConfigurationSection
    {
        [ConfigurationProperty("name", DefaultValue = "default", IsRequired = true, IsKey = true)]
        public string name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("certsvc", IsRequired = true)]
        public string certsvc
        {
            get
            {
                return (string)this["certsvc"];
            }
            set
            {
                this["certsvc"] = value;
            }
        }

        [ConfigurationProperty("companyInfo")]
        public CompanyElement companyInfo
        {
            get
            {
                return (CompanyElement)this["companyInfo"];
            }
        }

        [ConfigurationProperty("productInfo")]
        public ProductElement productInfo
        {
            get
            {
                return (ProductElement)this["productInfo"];
            }
        }

        [ConfigurationProperty("queues", IsDefaultCollection = false)]
        public QueueCollection queues
        {
            get
            {
                return (QueueCollection)base["queues"];
            }
        }

        [ConfigurationProperty("redises", IsDefaultCollection = false)]
        public RedisCollection redises
        {
            get
            {
                return (RedisCollection)base["redises"];
            }
        }

        [ConfigurationProperty("servers", IsDefaultCollection = false)]
        public HostCollection servers
        {
            get
            {
                return (HostCollection)base["servers"];
            }
        }
    }
}