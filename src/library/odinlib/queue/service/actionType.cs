namespace OdinSdk.OdinLib.Queue.Service
{
    /// <summary>
    ///
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        ///
        /// </summary>
        noack,

        /// <summary>
        ///
        /// </summary>
        transfer,

        /// <summary>
        /// memory 캐시에 추가 요청
        /// </summary>
        insert,

        /// <summary>
        /// memory 캐시에서 삭제 요청
        /// </summary>
        delete,

        /// <summary>
        /// memory 캐시 내용 update 요청
        /// </summary>
        update,

        /// <summary>
        /// 처음 상태로 되돌리는 명령
        /// config => removekey and app.config reload
        /// </summary>
        reload,

        /// <summary>
        /// 중앙에서 관리하는 서비스가 각각의 서비스에게
        /// 현재 보유하고 있는 정보를 전달하도록 요청 함
        /// </summary>
        report,

        /// <summary>
        /// report 요청에 대한 대답
        /// </summary>
        issued,

        /// <summary>
        /// 큐에 연결된 중앙 서비스에게
        /// 자신과 관계있는 데이터를 전송 해주기를 요청 함
        /// </summary>
        request,

        /// <summary>
        /// request 요청에 대한 대답
        /// </summary>
        reponse,

        /// <summary>
        /// 본인이 alive 함을 알려 달라는 요청
        /// </summary>
        ping,

        /// <summary>
        /// ping에 대한 대답
        /// </summary>
        pong
    }
}