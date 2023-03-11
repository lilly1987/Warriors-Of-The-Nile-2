using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {

        public void Awake()
        {
            Warriors_Of_The_Nile_2.Setup(Logger, Config);
        }

        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            if (Warriors_Of_The_Nile_2.Instance)
            {
                Logger.LogWarning("SetActive");
                Warriors_Of_The_Nile_2.Instance.gameObject.SetActive(true);
            }
        }

        public void Start()
        {
            Logger.LogWarning("Start");
            
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");

        }
    }
}
