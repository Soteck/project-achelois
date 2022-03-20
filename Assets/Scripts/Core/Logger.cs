using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Core {
    public class Logger : Singleton<Logger>
    {
        [SerializeField]
        private TextMeshProUGUI debugAreaText = null;

        [SerializeField]
        private int maxLines = 15;

        private List<string> lines = new List<string>();
        void Awake()
        {
            if (debugAreaText == null)
            {
                debugAreaText = GetComponent<TextMeshProUGUI>();
            }
            debugAreaText.text = string.Empty;
            AddText($"<color=\"white\">{DateTime.Now.ToString("HH:mm:ss.fff")} {this.GetType().Name} enabled</color>");
        }

        void OnEnable()
        {
            if (debugAreaText == null)
            {
                debugAreaText = GetComponent<TextMeshProUGUI>();
            }
            DrawLog();
        }

        private void AddText(string text) {
            lines.Add(text);
            if (lines.Count > maxLines) {
                lines.RemoveAt(0);
            }
        }

        public void DrawLog() {
            debugAreaText.text = string.Empty;
            foreach (string line in lines) {
                debugAreaText.text += line + '\n';
            }
        }
        
        public void LogInfo(string message)
        {
            AddText($"<color=\"green\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>");
        }

        public void LogError(string message)
        {
            AddText($"<color=\"red\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>");
        }

        public void LogWarning(string message)
        {
            AddText($"<color=\"yellow\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>");
        }

        public static void Info(string message) {
            Instance.LogInfo(message);
        }

        public static void Error(string message) {
            Instance.LogError(message);
        }

        public static void Warning(string message) {
            Instance.LogWarning(message);
        }

        public static void Draw() {
            Instance.DrawLog();
        }
    }
}