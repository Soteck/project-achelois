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
        private int maxLines = 100;

        private const int ConsoleMaxLength = 5000;
        private const int ConsoleChunkToDelete = 2500;
        private const int ConsoleOverflow = ConsoleMaxLength + ConsoleChunkToDelete;

        private readonly List<LogLine> _lines = new List<LogLine>();
        void Awake()
        {
            if (debugAreaText == null)
            {
                debugAreaText = GetComponent<TextMeshProUGUI>();
            }
            debugAreaText.text = string.Empty;
            AddText($"<color=\"white\">{DateTime.Now.ToString("HH:mm:ss.fff")} {this.GetType().Name} enabled</color>");
        }

        private void OnEnable()
        {
            if (debugAreaText == null)
            {
                debugAreaText = GetComponent<TextMeshProUGUI>();
            }
            DrawLog();
        }

        private void AddText(string text) {
            _lines.Add(new LogLine() {
                drawn = false,
                data = text
            });
            
            if (_lines.Count > maxLines) {
                _lines.RemoveAt(0);
            }
            DrawLog();
        }

        private void DrawLog() {
            if (debugAreaText.text.Length > ConsoleOverflow ) {
                debugAreaText.text = debugAreaText.text.Substring(ConsoleMaxLength, debugAreaText.text.Length - ConsoleMaxLength);
            }
            
            foreach (LogLine line in _lines) {
                if (line.drawn) continue;
                debugAreaText.text += line.data + '\n';
                line.drawn = true;
            }
        }

        private void LogInfo(string message) {
            Debug.Log(message);
            AddText($"<color=\"green\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
            AddText($"<color=\"red\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
            AddText($"<color=\"yellow\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
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
        
        
        private class LogLine {
            public bool drawn;
            public string data;
        }
    }
}