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

using System.Collections;
using System.Text.RegularExpressions;

namespace OdinSdk.eTaxBill.Net.Core
{
    #region class _Parameter

    /// <summary>
    ///
    /// </summary>
    public class Parameter
    {
        private string m_param_name = "";
        private string m_param_value = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="param_name"></param>
        /// <param name="param_value"></param>
        public Parameter(string param_name, string param_value)
        {
            m_param_name = param_name;
            m_param_value = param_value;
        }

        #region Properties implementation

        /// <summary>
        ///
        /// </summary>
        public string param_name
        {
            get
            {
                return m_param_name;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string param_value
        {
            get
            {
                return m_param_value;
            }
        }

        #endregion Properties implementation
    }

    #endregion class _Parameter

    /// <summary>
    /// Summary description for _ParamParser.
    /// </summary>
    public class ParamParser
    {
        #region function Paramparser_NameValue

        /// <summary>
        /// Parses name-value params.
        /// </summary>
        /// <param name="source">Parse source.</param>
        /// <param name="expressions">Expressions importance order. NOTE: must contain param and value groups.</param>
        public static Parameter[] Paramparser_NameValue(string source, string[] expressions)
        {
            var tmp = source.Trim();

            ArrayList param = new ArrayList();
            foreach (string exp in expressions)
            {
                Regex r = new Regex(exp, RegexOptions.IgnoreCase);
                Match m = r.Match(tmp);
                if (m.Success)
                {
                    param.Add(new Parameter(m.Result("${param}").Trim(), m.Result("${value}")));

                    // remove matched string part form tmp
                    tmp = tmp.Replace(m.ToString(), "").Trim();
                }
            }

            // There are some unparsed params, add them as UnParsed
            if (tmp.Trim().Length > 0)
            {
                param.Add(new Parameter("UNPARSED", tmp));
            }

            Parameter[] retVal = new Parameter[param.Count];
            param.CopyTo(retVal);

            return retVal;
        }

        #endregion function Paramparser_NameValue
    }
}