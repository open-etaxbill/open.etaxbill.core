using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public class DataToken<T>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="action_type"></param>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="data"></param>
        public DataToken(ActionType? action_type = null, HostInfo sender = null, HostInfo receiver = null, T data = default(T))
        {
            this.action = action_type ?? ActionType.noack;

            this.sender = sender ?? new HostInfo();
            this.receiver = receiver ?? new HostInfo("", "", "", "", "");

            this.data = data;
        }

        /// <summary>
        ///
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType action
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public HostInfo sender
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public HostInfo receiver
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public T data
        {
            get;
            set;
        }
    }
}