using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerCmd
{
    public const int AUTH_USER_LOGIN = 1000;            // 계정 로그인
    public const int AUTH_GAME_LOGIN = 1001;            // 게임 로그인
    public const int AUTH_EXISTS_USER = 1010;           // 계정 존재 유무
    public const int AUTH_GAME_LEAVE = 9901;            // 계정 삭제/탈퇴
    public const int AUTH_USER_RESTORE = 9902;

    public const int USER_BASIC_INFO = 2100;                // 기본정보 요청
    public const int USER_HEART_PRODUCTION_INFO = 2101;     // 생명력 생산 정보 요청
    public const int USER_CRYSTAL_PRODUCTION_INFO = 2102;   // 결정 생산 정보 요청
    public const int USER_UNLOCK_LIST = 2103;               // 언락 정보 - 서버에서 관리하는 일부 언락 정보
    public const int USER_OFFLINE_REWARD = 2104;            // 오프라인 보상 획득
    public const int SAVE_TUTORIAL = 2105;                  // 튜토리얼 저장 요청
    public const int USER_RAW_DATA = 2106;                  // 클라이언트 통데이터
    public const int PAGE_TUTORIAL_IDX = 2107;              // 완료된 페이지 튜토리얼 인덱스.

    public const int USER_DATA_SAVE = 2110;                 // 유저 데이터 저장
    public const int USER_DATA_LOAD = 2111;                 // 유저 데이터 로드

    public const int CORAL_LIST = 2200;         // 산호 리스트
    public const int CORAL_BUY = 2201;          // 산호 구매
    public const int CORAL_LEVEL_UP = 2202;     // 산호 레벨업
    public const int STONE_LEVEL_UP = 2210;     // 산호석 레벨업
    public const int HEART_HARVEST = 2211;      // 생명력 수확
    public const int HEART_TOUCH = 2220;        // 터치하여 생명력 생산 주기 감소 정보

    public const int FOOD_LIST = 2250;          //소라게 먹이 리스트
    public const int FOOD_USE = 2251;           //소라게 밥 먹이기
    public const int CRYSTAL_HARVEST = 2252;    //결정 수확

    public const int FISH_STORAGE_LIST = 2300;          // 물고기 보관함 정보
    //const FISH_TANK_LIST  = 2301;
    public const int FISH_BUY = 2302;                   // 물고기 구매 요청
    public const int FISH_MOVE_TANK = 2303;             // 물고기 수조로 이동 요청 (수조로 배치)
    public const int FISH_MOVE_STORAGE = 2304;          // 물고기 보관함으로 이동 요청
    public const int FISH_RANDOM_MOVE_TANK = 2305;      // 물고기 수조로 랜덤 이동 요청 (수조로 랜덤 배치)
    public const int FISH_TOTAL_MOVE_STORAGE = 2306;    // 물고기 모두 보관함으로 이동 요청
    public const int FISH_NOW_SEASON_SETTING = 2307;    // 이번 시즌 물고기 배치 요청
    public const int FISH_CHANGE_ARRANGE_SETTING = 2310;    // 물고기 배치 세팅 요청
    public const int FISH_BUBBLE_LIST = 2320;           // 물고기 버블 리스트
    public const int FISH_BUBBLE_REWARD = 2321;         // 물고기 버블 보상 요청

    //public const int WEMIX_GET_TRADE_INFO = 2400;       // 토큰 교환 정보
    //public const int WEMIX_INGAME_TO_FT = 2401;         // 인게임재화 -> FT
    //public const int WEMIX_FT_TO_INGAME = 2402;         // FT -> 인게임재화
    //public const int WEMIX_CONNECT = 2403;              // 위믹스연동
    //public const int WEMIX_NFT_ANIPANG_ACTIVE = 2404;   // 애니팡 NFT 활성화

    public const int WEMIX_TRADE_INFO = 2400;           // 토큰 교환 정보
    public const int WEMIX_TRADE_HASH_TO_FT = 2401;     // 인게임재화 -> FT sign hash 요청
    public const int WEMIX_TRADE_HASH_TO_INGAME = 2402; // FT -> 인게임재화 sign hash 요청
    public const int WEMIX_CONNECT = 2403;              // 위믹스연동    
    public const int WEMIX_TRADE = 2404;
    public const int WEMIX_APPROVE_HASH = 2405;
    public const int WEMIX_APPROVE = 2406;              // FT 교환 인출허가 최종트랜젝션    

    public const int CARD_PACK_LIST = 2500;     // 카드팩 리스트.
    public const int CARD_PACK_OPEN = 2501;     // 카드팩 오픈.
    public const int CARD_LIST = 2502;          // 카드 리스트.
    public const int CARD_BAG_EXTEND = 2503;    // 카드 가방 확장 구매.
    public const int CARD_SYNTHESIS = 2504;     // 카드 가방 합성 구매.
    public const int CARD_CHANGE_LOCK = 2505;   // 카드 잠금 상태 변경.
    public const int CARD_USE = 2506;           // 카드 사용.
    public const int CARD_SELL = 2507;           // 카드 판매.

    public const int WELCOME_CARD_INFO = 2510;     // 웰컴카드 정보
    public const int WELCOME_CARD_REWARD = 2511;     // 웰컴카드 보상받기



    public const int SKILL_LIST = 2600;         // 스킬 리스트
    public const int SKILL_LEVEL_UP = 2601;     // 스킬 레벨업
    public const int SKILL_USE = 2602;          // 스킬 사용

    public const int ARTIFACT_LIST = 2610;      // 아티팩트 리스트
    public const int ARTIFACT_BUY = 2611;       // 아티팩트 구매
    public const int ARTIFACT_LEVEL_UP = 2612;  // 아티팩트 레벨업

    public const int DAILY_MISSION_LIST = 2650;     // 일일 미션 리스트
    public const int DAILY_MISSION_REWARD = 2651;   // 일일 미션 보상 요청

    public const int AD_LIST = 2660;            // 광고 리스트
    public const int AD_REWARD = 2661;          // 광고 보상 요청

    public const int THEME_DYE_COSTUME_LIST = 2670; // 테마, 염색, 코스튬 리스트.
    public const int THEME_DYE_COSTUME_BUY = 2671; // 테마, 염색, 코스튬 구입.
    public const int THEME_DYE_COSTUME_EQUIP = 2672; // 테마, 염색, 코스튬 적용.

    public const int COLLECTION_LIST = 2680;        // 컬렉션 리스트
    public const int COLLECTION_REWARD = 2681;   // 컬렉션 보상 요청

    public const int EVENT_INFO = 2700;                     // 이벤트 정보
    public const int EVENT_SEASONPASS_REWARD = 2701;        // 이벤트 시즌패스 보상받기
    public const int EVENT_SEASONPASS_CHANGE = 2702;        // 이벤트 시즌패스 재화 치환
    public const int EVENT_SEASONPASS_LEVEL_REWARD = 2703;        // 이벤트 시즌패스 레벨 보상

    public const int FARM_DATA = 2800;        // 농장, 농장물 정보
    public const int FARM_BUY = 2801;        // 농장 구매.
    public const int FARM_COLLECT = 2802;        // 농장물 수확.

    public const int EXPAND_DATA = 2900;            // 확장물 정보.
    public const int EXPAND_BUY = 2901;             // 확장물 구매.
    public const int TILE_BUY = 2902;               // 타일 구매.
    public const int EXPAND_EDIT = 2903;            // 확장물 설치, 수정, 삭제.
    public const int EXPAND_LOCATE_INIT = 2904;     // 확장물 설치 전체 초기화.
    public const int EXPAND_LOCATE_DATA = 2905;     // 확장물 설치 정보.


    public const int ATTENDANCE_INFO = 6000;    // 출석 정보
    public const int ATTENDANCE_REWARD = 6001;   // 출석 보상

    public const int SHOP_LIST = 7001;                  // 상점 정보 조회
    public const int SHOP_TERM_ITEM_LIST = 7002;        // 기간제 상품 조회
    public const int SHOP_BUY = 7003;                   // 상품 구매

    public const int SUBSCRIPTION_STATE = 7105;     // 구독 상태 요청
    public const int SUBSCRIPTION_COUPON = 7030;    //구독 쿠폰 발급 요청    

    public const int COUPON_REWAED = 7010;      // 쿠폰 보상 요청
    public const int COUPON_CREATE = 7040;    // 쿠폰 발급 요청

    public const int BILLING_INIT = 7100;       //  결제 준비 요청
    public const int BILLING_RECEIPT = 7101;    // 영수증 검증 요청
    public const int BILLING_END = 7102;

    public const int SENDBOX_GET_LIST = 8000;           // 우편 목록 요청
    public const int SENDBOX_RECEIVE_REWARD = 8001;     // 우편 한개 받기
    public const int SENDBOX_RECEIVE_REWARD_ALL = 8002; // 우편 전체 받기

}