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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OdinSdk.OdinLib.Configuration;
using System;

namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        ///
        /// </summary>
        public ProductInfo()
            : this(null, null, null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="product_id">제품 ID</param>
        /// <param name="version"></param>
        /// <param name="product_name"></param>
        /// <param name="product_type"></param>
        public ProductInfo(string product_id = null, string version = null, string product_name = null, ProductType? product_type = null)
        {
            productId = product_id ?? MyAppService.Value.productInfo.id;
            version = version ?? MyAppService.Value.productInfo.version;
            product_name = product_name ?? MyAppService.Value.productInfo.name;

            if (product_type == null)
                productType = (ProductType)Enum.Parse(typeof(ProductType), MyAppService.Value.productInfo.type);
            else
                productType = product_type.Value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="product"></param>
        public ProductInfo(ProductElement product)
        {
            productId = product.id;
            version = product.version;
            product_name = product.name;
            productType = (ProductType)Enum.Parse(typeof(ProductType), product.type);
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
        public string version
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string product_name
        {
            get;
            set;
        }

        /// <summary>
        /// type of permit service
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ProductType productType
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

            ProductInfo p = (ProductInfo)obj;
            return p.productId == this.productId && p.version == this.version;
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