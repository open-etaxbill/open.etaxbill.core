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

using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public class CompanyElement : ConfigurationElement
    {
        /// <summary>
        ///
        /// </summary>
        public CompanyElement()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">회사 id</param>
        /// <param name="corporate_id">법인 ID</param>
        public CompanyElement(string company_id, string corporate_id)
        {
            companyId = company_id;
            corporateId = corporate_id;
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("companyId", DefaultValue = "HQ", IsRequired = true)]
        public string companyId
        {
            get
            {
                return (string)this["companyId"];
            }
            set
            {
                this["companyId"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("corporateId", DefaultValue = "SC", IsRequired = true)]
        public string corporateId
        {
            get
            {
                return (string)this["corporateId"];
            }
            set
            {
                this["corporateId"] = value;
            }
        }
    }
}