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

namespace OdinSdk.OdinLib.Configuration
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 클라이언트 또는 서버
    /// </summary>
    public enum MKindOfLocation
    {
        /// <summary></summary>
        Empty = 0,

        /// <summary></summary>
        Client,

        /// <summary></summary>
        Server
    }

    /// <summary>
    /// Profucts는
    /// </summary>
    public enum MKindOfCategory
    {
        /// <summary></summary>
        Empty = 0,

        /// <summary>단품으로 판매 및 설치가 가능한 단위 모듈</summary>
        Products,

        /// <summary>일시적으로 개발되어지며 다른 사용자에게는 설치가 불가능한 모듈</summary>
        Projects,

        /// <summary>다른 모듈들을 지원하기 위해 개발 되어지는 모듈들</summary>
        Services,

        /// <summary>여러가지의 Product를 조합하여 한개의 솔루션을 만듭니다.</summary>
        Solutions
    }

    /// <summary>
    /// 레지스터리 또는 Licsense Server의 Constant를 읽어 오기 위한 Key값들 입니다.
    /// </summary>
    public enum MKindOfAppKey
    {
        /// <summary></summary>
        Empty = 0,

        /// <summary></summary>
        CurrentCompanyID,

        /// <summary></summary>
        DefaultLogType,

        /// <summary></summary>
        DefaultLogDir,

        /// <summary></summary>
        DefaultWSUrl,

        /// <summary></summary>
        ServerURI,

        /// <summary></summary>
        ConnectionString,

        /// <summary></summary>
        DirectionOfInterface,

        /// <summary></summary>
        FrameWSUrl,

        /// <summary></summary>
        KindOfDB
    }

    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
}