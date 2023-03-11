using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using UnityEngine;

namespace BepInPluginSample
{
    internal class Warriors_Of_The_Nile_2 : MonoBehaviour
    {
        #region GUI
        public static ManualLogSource logger;
        public static ConfigFile Config;

        static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isGUIOnKey;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isOpenKey;

        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isOpen;
        private ConfigEntry<float> uiW;
        private ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string title = "";
        public string windowName = ""; // 변수용 
        public string FullName = "Plugin"; // 창 펼쳤을때
        public string ShortName = "P"; // 접었을때

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;
        #endregion

        internal static Warriors_Of_The_Nile_2 Instance { get; private set; }

        #region 변수
        // =========================================================

        private static ConfigEntry<bool> noDeadConfirm;
        private static ConfigEntry<bool> isCurrencyEnough;
        // private static ConfigEntry<float> uiW;
        // private static ConfigEntry<float> xpMulti;

        // =========================================================
        #endregion

        internal static void Setup(ManualLogSource Logger, ConfigFile config)
        {
            logger = Logger;
            Config = config;

            GameObject gameObject = new GameObject("Sample");
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            Warriors_Of_The_Nile_2.Instance = gameObject.AddComponent<Warriors_Of_The_Nile_2>();
        }

        public void Awake()
        {
            #region GUI

            logger.LogMessage("Awake");

            isGUIOnKey = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            isOpenKey = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;
            uiW = Config.Bind("GUI", "uiW", 300f);
            uiH = Config.Bind("GUI", "uiH", 600f);

            if (isOpen.Value)
                windowRect = new Rect(Screen.width - 65, 0, uiW.Value, 800);
            else
                windowRect = new Rect(Screen.width - uiW.Value, 0, uiW.Value, 800);

            IsOpen_SettingChanged(null, null);



            #endregion

            #region 변수 설정
            // =========================================================

            noDeadConfirm = Config.Bind("game", "noDeadConfirm", true);
            isCurrencyEnough = Config.Bind("game", "isCurrencyEnough", true);
            // xpMulti = Config.Bind("game", "xpMulti", 2f);

            // =========================================================
            #endregion
        }

        #region GUI
        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = isGUIOnKey.Value.ToString() + "," + isOpenKey.Value.ToString();
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                title = "";
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                windowRect.x += (uiW.Value - 64);
            }
        }
        #endregion

        public void OnEnable()
        {
            logger.LogWarning("OnEnable");
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Warriors_Of_The_Nile_2));
                //StartCoroutine(coroutine(1));
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        //딜레이를 할 함수를 만들어 줍니다. 반환값을 IEnumerator 로 해주어야 합니다.
        private IEnumerator coroutine(float waitTime)
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTime); //waitTime 만큼 딜레이후 다음 코드가 실행된다.
                CurBattleMap = (BattleMap)typeof(BattleManager).GetField("CurBattleMap").GetValue(Singleton<BattleManager>.Instance);
                if (CurBattleMap)
                {
                    factionPawnMap = (Dictionary<FactionType, List<Pawn>>)typeof(BattleMap).GetField("factionPawnMap").GetValue(CurBattleMap);
                }
                if (factionPawnMap.ContainsKey(FactionType.Hero))
                    pawns = factionPawnMap[FactionType.Hero] as List<Pawn>;
                logger.LogWarning($"coroutine ; {pawns.Count}");
                //While문을 빠져 나가지 못하여 waitTime마다 Shot함수를 반복실행 됩니다.
            }
        }

        public void Update()
        {
            #region GUI
            if (isGUIOnKey.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (isOpenKey.Value.IsUp())// 단축키가 일치할때
            {
                if (isGUIOn.Value)
                {
                    isOpen.Value = !isOpen.Value;
                }
                else
                {
                    isGUIOn.Value = true;
                    isOpen.Value = true;
                }
            }
            #endregion
        }

        #region GUI
        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            // 창 나가는거 방지
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }
        #endregion

        public virtual void WindowFunction(int id)
        {
            #region GUI
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
                                        // 라벨 추가
                                        //GUILayout.Label(windowName, GUILayout.Height(20));
                                        // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            if (isOpen.Value) GUILayout.Label(title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { isOpen.Value = !isOpen.Value; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn.Value = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!isOpen.Value) // 닫혔을때
            {
            }
            else // 열렸을때
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                #endregion

                #region 여기에 GUI 항목 작성
                // =========================================================

                if (GUILayout.Button($"noDeadConfirm {noDeadConfirm.Value}")) { noDeadConfirm.Value = !noDeadConfirm.Value; }
                if (GUILayout.Button($"isCurrencyEnough {isCurrencyEnough.Value}")) { isCurrencyEnough.Value = !isCurrencyEnough.Value; }
                if (GUILayout.Button($"SilverCoin +10000")) { InventoryManager.AddCurrency(CurrencyType.SilverCoin, 10000);  }
                if (GUILayout.Button($"PharaohCoin +100")) { InventoryManager.AddCurrency(CurrencyType.PharaohCoin, 100);  }

                GUILayout.Label($"pawns ; {pawns.Count}");
                foreach (var item in pawns)
                {
                    GUILayout.Label($"{item.name}");
                }
                GUILayout.Label($"---");
                // GUILayout.BeginHorizontal();
                // GUILayout.Label($"ammoMulti {ammoMulti.Value}");
                // if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { ammoMulti.Value += 1; }
                // if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { ammoMulti.Value -= 1; }
                // GUILayout.EndHorizontal();

                // =========================================================
                #endregion

                #region GUI
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
            #endregion
        }


        public void OnDisable()
        {
            logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
        }

        #region Harmony
        // ====================== 하모니 패치 샘플 ===================================

        public static List<Pawn> pawns = new List<Pawn>();
        public static Dictionary<FactionType, List<Pawn>> factionPawnMap = new Dictionary<FactionType, List<Pawn>>();
        public static BattleMap CurBattleMap;

        [HarmonyPatch(typeof(BattleMap), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void BattleMap_ctor(BattleMap __instance, Dictionary<FactionType, List<Pawn>> ___factionPawnMap)
        {
            factionPawnMap = ___factionPawnMap;
            pawns = factionPawnMap[FactionType.Hero] as List<Pawn>;
            logger.LogWarning($"BattleMap_ctor ; {___factionPawnMap.Count} ; {pawns.Count} ");
            //BattleManager.GetCurrentBattleMap().GetPawnsByFaction(FactionType.Hero,ref pawns);
        }

        [HarmonyPatch(typeof(BattleMap), "GenManagerPawn")]
        [HarmonyPostfix]
        public static void GenManagerPawn(BattleMap __instance, TmxMap tmx, Dictionary<FactionType, List<Pawn>> ___factionPawnMap)
        {
            logger.LogWarning($"GenManagerPawn ; {___factionPawnMap.Count}");
        /*
            factionPawnMap = ___factionPawnMap;
            pawns = factionPawnMap[FactionType.Hero] as List<Pawn>;
            //BattleManager.GetCurrentBattleMap().GetPawnsByFaction(FactionType.Hero,ref pawns);
        */
        }
        
        [HarmonyPatch(typeof(BattleMap), "LoadMap", typeof(string), typeof(bool))]
        [HarmonyPostfix]
        public static void LoadMap(BattleMap __instance, string mapName, bool withPawn , Dictionary<FactionType, List<Pawn>> ___factionPawnMap)
        {
            logger.LogWarning($"GenManagerPawn ; {___factionPawnMap.Count}");
            factionPawnMap = ___factionPawnMap;
            pawns = factionPawnMap[FactionType.Hero] as List<Pawn>;
            //BattleManager.GetCurrentBattleMap().GetPawnsByFaction(FactionType.Hero,ref pawns);
        /*
        */
        }

        [HarmonyPatch(typeof(BattleManager), "SetCurrentBattleMap")]
        [HarmonyPostfix]
        public static void SetCurrentBattleMap(BattleManager __instance, BattleMap map)
        {
            
            logger.LogWarning($"SetCurrentBattleMap ; {map.GetPawnCountOfFaction(FactionType.Hero)}");
        /*
            CurBattleMap = map;
            factionPawnMap=(Dictionary < FactionType, List < Pawn >> )typeof(BattleMap).GetField("factionPawnMap").GetValue(map);
            pawns = factionPawnMap[FactionType.Hero] as List<Pawn>;
            //BattleManager.GetCurrentBattleMap().GetPawnsByFaction(FactionType.Hero,ref pawns);
        */
        }

        // public static void CheckDeadPawn(Pawn attacker, Pawn mainTarget = null, bool isGenDeadEvent = true, bool onlyGenIfNotExistInEventStack = false)
        [HarmonyPatch(typeof(BattleManager), "CheckDeadPawn", typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void CheckDeadPawn(Pawn attacker, Pawn mainTarget, bool isGenDeadEvent, bool onlyGenIfNotExistInEventStack)
        {
            if (!noDeadConfirm.Value)
            {
                return;
            }
            logger.LogWarning($"CheckDeadPawn ; {attacker.name} ; {mainTarget?.name} ;{mainTarget?.Stat.HP} ; {isGenDeadEvent} ; {onlyGenIfNotExistInEventStack} ");
            if (mainTarget?.Faction == FactionType.Hero)
            {
                mainTarget.Stat.FillMaxHP();
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "AddCurrency", typeof(CurrencyType), typeof(int), typeof(bool))]
        [HarmonyPostfix]
        public static void AddCurrency(CurrencyType type, int count, bool updateUI = true)
        {
            logger.LogWarning($"AddCurrency ; {type} ; {count}");
        }
        /*
        //public static bool IsCurrencyEnough(CurrencyType type, int price)
        //[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.IsCurrencyEnough),typeof(CurrencyType),typeof(int))]//, MethodType.StaticConstructor
        [HarmonyPatch(typeof(InventoryManager), "IsCurrencyEnough", typeof(CurrencyType),typeof(int))]//, MethodType.StaticConstructor
        [HarmonyPostfix]
        public static void IsCurrencyEnough(CurrencyType type, int price, ref bool __result)
        {
            if (!isCurrencyEnough.Value)
            {
                return;
            }
            logger.LogWarning($"{nameof(InventoryManager.IsCurrencyEnough)} ; {type} ; {price} ");
            switch (type)
            {
                case CurrencyType.SilverCoin:
                case CurrencyType.PharaohCoin:
                    InventoryManager.AddCurrency(type, price);                   
                    __result = true;
                    break;
                case CurrencyType.Orb:
                    break;
                default:
                    break;
            }

        }
        */
        /*
        [HarmonyPatch(typeof(Pawn), MethodType.Constructor)]
        [HarmonyPrefix]
        public static void PawnC(Pawn __instance)
        {
            logger.LogWarning($"PawnC ;{__instance.name} ; {__instance.Faction}");
            if (__instance.Faction == FactionType.Hero)
            {
                pawns.Add(__instance);
            }

        }
        */

        [HarmonyPatch(typeof(Pawn), "DeadConfirm", typeof(bool))]
        [HarmonyPrefix]
        public static void DeadConfirm(Pawn __instance, bool force)
        {
            if (!noDeadConfirm.Value)
            {
                return;
            }
            logger.LogWarning($"DeadConfirm ; {GameConfig.IsPlayerControlledPawn(__instance)} ; {__instance.Stat.HP} ; {__instance.Stat.MaxHP} ; {force}");
            if (__instance.Stat.HP <= 0 && __instance.Stat.MaxHP > 0 && GameConfig.IsPlayerControlledPawn(__instance) && !force)
            {
                __instance.Stat.FillMaxHP();
            }            
        }
        
        [HarmonyPatch(typeof(Pawn), "ReadFromTransitData")]
        [HarmonyPrefix]
        public static void ReadFromTransitData(PawnTransitData data)
        {
            if (!noDeadConfirm.Value)
            {
                return;
            }
            var __instance = data;
            logger.LogWarning($"DeadConfirm ; {__instance.Stat.HP} ; {__instance.Stat.MaxHP} ; ");
            //if (__instance.Stat.HP <= 0 && __instance.Stat.MaxHP > 0 && GameConfig.IsPlayerControlledPawn(__instance) && !force)
            //{
            //    __instance.Stat.FillMaxHP();
            //}            
        }


        
        [HarmonyPatch(typeof(PlayerPawnInfoBoard), "Start")]
        [HarmonyPrefix]
        public static void PlayerPawnInfoBoardStart(PlayerPawnInfoBoard __instance)
        {

            logger.LogWarning($"PlayerPawnInfoBoardStart ;");
        }
        
        [HarmonyPatch(typeof(PlayerPawnInfoBoard), "setBaseStat")]
        [HarmonyPrefix]
        public static void setBaseStat(Pawn p, PawnBaseConfig config)
        {

            logger.LogWarning($"setBaseStat ;");
        }
        
        //List<Pawn> pawns = new List<Pawn>();

        /*
        [HarmonyPatch(typeof(AEnemy), "DamageMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eDamageMult.Value;
        }
        */
            // =========================================================
            #endregion
    }
}
