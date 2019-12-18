using System;

namespace OpenTax.Engine.Library.Data
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
        public DbResult(Exception p_error = null, bool p_result = false, int p_status = 0, string p_message = "")
        {
            error = p_error;
            result = p_result;
            status = p_status;
            message = p_message;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_error"></param>
        /// <param name="p_result"></param>
        /// <param name="p_status"></param>
        /// <param name="p_message"></param>
        public void Update(Exception p_error = null, bool p_result = false, int p_status = 0, string p_message = "")
        {
            error = p_error;
            result = p_result;
            status = p_status;
            message = p_message;
        }
    }
}