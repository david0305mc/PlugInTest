using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Protocols.Response
{
    public class ErrorData
    {
        public int code;
        public JToken message;
        public T GetResult<T>() => message.ToObject<T>();
    }

    public class AlertData
    {
        public int code;
        public JToken message;
        public T GetResult<T>() => message.ToObject<T>();
    }
    #region ########### Auth ###########
    public class SignIn
    {
        public ulong uno;
        public string token;
        public string platform_id;
        /// <summary>탈퇴 유예시간 -> 0이 아닐 경우 탈퇴중 팝업 표시, 게임 로그인 진행하지 않음</summary>
        public double leave_time;
        public string country;
        /// <summary>최초 계정 생성여부 (0=재로그인, 1=최초 계정 생성)</summary>
        public int first_login;
    }

    public class Login
    {
        public string session;
    }
    #endregion
}
