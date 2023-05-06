using RFUniverse.Attributes;
using RFUniverse.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RFUniverse
{
    public class PlayerMain : RFUniverseMain
    {
        public static PlayerMain Instance = null;
        [SerializeField]
        private PlayerMainUI playerMainUI;


        public static Version pythonVersion = new Version();
        Queue<string> logList = new Queue<string>();
        [SerializeField]
        float fixedDeltaTime = 0.02f;

        public float FixedDeltaTime
        {
            get
            {
                return fixedDeltaTime;
            }
            set
            {
                fixedDeltaTime = value;
                Time.fixedDeltaTime = fixedDeltaTime;
            }
        }
        [SerializeField]
        float timeScale = 1;
        public float TimeScale
        {
            get
            {
                return timeScale;
            }
            set
            {
                timeScale = value;
                Time.timeScale = timeScale;
            }
        }

        public void Pend()
        {
            playerMainUI.ShowPend();
        }
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            FixedDeltaTime = fixedDeltaTime;
            TimeScale = timeScale;

            BaseAttr[] sceneAttrs = FindObjectsOfType<BaseAttr>(true);
            List<BaseAttr> noParentAttr = new List<BaseAttr>(sceneAttrs);
            foreach (var item in sceneAttrs)
            {
                foreach (var child in item.childs)
                {
                    noParentAttr.Remove(child);
                }
            }
            foreach (var item in noParentAttr)
            {
                if (item != null)
                    item.Instance();
            }

            BaseAgent agent = BaseAgent.Instance;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_IDColor", Color.black);
            foreach (var render in Ground.GetComponentsInChildren<Renderer>())
            {
                render.SetPropertyBlock(mpb);
            }

            playerMainUI.Init(pythonVersion,
                () => AssetManager.Instance.SendPendDoneMsg()
                ); ;
        }

        public void AddLog<T>(T log)
        {
            logList.Enqueue(log.ToString());
            if (logList.Count > 50)
                logList.Dequeue();
            playerMainUI.RefreshLogList(logList.ToArray());
        }
        void OnValidate()
        {
            Instance = this;
        }
    }
}