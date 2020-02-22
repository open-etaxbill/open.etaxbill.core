using System;

namespace OdinSdk.OdinLib.Data
{
    /// <summary>
    ///
    /// </summary>
    public class DbResult
    {
        /// <summary>
        ///
        /// </summary>
        public bool result;

        /// <summary>
        ///
        /// </summary>
        public int status;

        /// <summary>
        ///
        /// </summary>
        public Exception error;

        /// <summary>
        ///
        /// </summary>
        public string message;

        /// <summary>
        ///
        /// </summary>
        public object output;

        /// <summary>
        ///
        /// </summary>
        public DbResult(Exception error = null, bool result = false, int status = 0, string message = "")
        {
            this.error = error;
            this.result = result;
            this.status = status;
            this.message = message;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="error"></param>
        /// <param name="result"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void Update(Exception error = null, bool result = false, int status = 0, string message = "")
        {
            this.error = error;
            this.result = result;
            this.status = status;
            this.message = message;
        }
    }
}