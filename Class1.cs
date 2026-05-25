using BepInEx;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StreamerPlus
{
    [BepInPlugin("com.FUTUREAA.gorillatag.StreamerPlusPlus", "StreamerPlus", "1.0.0")]
    public class StreamPlugin : BaseUnityPlugin
    {
        public const string HIDDEN = "HIDDEN";
        public static StreamPlugin Instance;

        private bool enabledMode = false;
        private bool uiVisible = true;
        private float scanTimer = 0f;
        private const float SCAN_INTERVAL = 1f;

        // Track originals so we can restore when disabled
        private readonly Dictionary<TMP_Text, string> tmpOriginals = new Dictionary<TMP_Text, string>();
        private readonly Dictionary<Text, string> uiOriginals = new Dictionary<Text, string>();
        private readonly Dictionary<TextMesh, string> meshOriginals = new Dictionary<TextMesh, string>();

        // Status UI (OnGUI)
        private Texture2D shadowTex;
        private Texture2D panelTex;
        private Texture2D panelHighlightTex;
        private Texture2D dividerTex;
        private Texture2D accentEnabledTex;
        private Texture2D accentDisabledTex;
        private Texture2D borderTex;
        private Texture2D chipTex;
        private GUIStyle titleStyle;
        private GUIStyle valueStyle;
        private GUIStyle hintStyle;
        private GUIStyle hintKeyStyle;
        private GUIStyle brandStyle;

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo("StreamerPlus loaded. Press F8 to toggle streamer mode.");
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f8Key.wasPressedThisFrame)
            {
                enabledMode = !enabledMode;
                if (!enabledMode) RestoreAll();
            }

            if (kb != null && kb.f5Key.wasPressedThisFrame)
            {
                uiVisible = !uiVisible;
            }

            scanTimer += Time.deltaTime;
            if (scanTimer >= SCAN_INTERVAL)
            {
                scanTimer = 0f;
                if (enabledMode) ScanAndHide();
            }
        }

        private string CurrentRoomName()
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
                return PhotonNetwork.CurrentRoom.Name;
            return null;
        }

        private void ScanAndHide()
        {
            string room = CurrentRoomName();
            if (string.IsNullOrEmpty(room)) return;

            foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (t == null) continue;
                string original;
                if (!tmpOriginals.TryGetValue(t, out original))
                {
                    original = t.text;
                    tmpOriginals[t] = original;
                }
                if (!string.IsNullOrEmpty(t.text) && t.text.Contains(room))
                {
                    string replaced = t.text.Replace(room, HIDDEN);
                    if (t.text != replaced) t.text = replaced;
                }
            }

            foreach (var t in Resources.FindObjectsOfTypeAll<Text>())
            {
                if (t == null) continue;
                string original;
                if (!uiOriginals.TryGetValue(t, out original))
                {
                    original = t.text;
                    uiOriginals[t] = original;
                }
                if (!string.IsNullOrEmpty(t.text) && t.text.Contains(room))
                {
                    string replaced = t.text.Replace(room, HIDDEN);
                    if (t.text != replaced) t.text = replaced;
                }
            }

            foreach (var t in Resources.FindObjectsOfTypeAll<TextMesh>())
            {
                if (t == null) continue;
                string original;
                if (!meshOriginals.TryGetValue(t, out original))
                {
                    original = t.text;
                    meshOriginals[t] = original;
                }
                if (!string.IsNullOrEmpty(t.text) && t.text.Contains(room))
                {
                    string replaced = t.text.Replace(room, HIDDEN);
                    if (t.text != replaced) t.text = replaced;
                }
            }
        }

        private void RestoreAll()
        {
            foreach (var kv in tmpOriginals)
                if (kv.Key != null) kv.Key.text = kv.Value;
            tmpOriginals.Clear();

            foreach (var kv in uiOriginals)
                if (kv.Key != null) kv.Key.text = kv.Value;
            uiOriginals.Clear();

            foreach (var kv in meshOriginals)
                if (kv.Key != null) kv.Key.text = kv.Value;
            meshOriginals.Clear();
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.wrapMode = TextureWrapMode.Clamp;
            t.Apply();
            return t;
        }

        private void EnsureGUIResources()
        {
            if (shadowTex == null) shadowTex = MakeTex(new Color(0f, 0f, 0f, 0.45f));
            if (panelTex == null) panelTex = MakeTex(new Color(0.06f, 0.07f, 0.10f, 0.94f));
            if (panelHighlightTex == null) panelHighlightTex = MakeTex(new Color(1f, 1f, 1f, 0.04f));
            if (dividerTex == null) dividerTex = MakeTex(new Color(1f, 1f, 1f, 0.08f));
            if (accentEnabledTex == null) accentEnabledTex = MakeTex(new Color(0.35f, 0.95f, 0.60f, 1f));
            if (accentDisabledTex == null) accentDisabledTex = MakeTex(new Color(0.98f, 0.35f, 0.42f, 1f));
            if (borderTex == null) borderTex = MakeTex(new Color(1f, 1f, 1f, 0.14f));
            if (chipTex == null) chipTex = MakeTex(new Color(1f, 1f, 1f, 0.10f));

            if (brandStyle == null)
            {
                brandStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                };
                brandStyle.normal.textColor = new Color(0.65f, 0.70f, 0.80f, 1f);
            }
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                };
                titleStyle.normal.textColor = new Color(0.78f, 0.82f, 0.90f, 1f);
            }
            if (valueStyle == null)
            {
                valueStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                };
            }
            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                };
                hintStyle.normal.textColor = new Color(0.78f, 0.82f, 0.90f, 1f);
            }
            if (hintKeyStyle == null)
            {
                hintKeyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                };
                hintKeyStyle.normal.textColor = Color.white;
            }
        }

        private void DrawBorder(Rect r, float t, Texture2D tex)
        {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), tex);
            GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), tex);
            GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), tex);
            GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), tex);
        }

        private void DrawKeyChip(Rect r, string key)
        {
            GUI.DrawTexture(r, chipTex);
            DrawBorder(r, 1f, borderTex);
            GUI.Label(r, key, hintKeyStyle);
        }

        private void OnGUI()
        {
            if (!uiVisible) return;
            EnsureGUIResources();

            // Layout: [F5 hint] | [STATUS] | [F8 hint] inside one HUD bar
            const float h = 46f;
            const float padX = 14f;

            const float hintLeftW = 130f;   // "F5  Toggle UI"
            const float statusW   = 240f;   // "● STREAMER MODE  ENABLED"
            const float hintRightW = 170f;  // "F8  Press to Enable"

            float innerW = hintLeftW + statusW + hintRightW;
            float totalW = innerW + padX * 2f + 8f; // divider spacing

            float x = (Screen.width - totalW) * 0.5f;
            float y = 14f;
            var bar = new Rect(x, y, totalW, h);

            // Shadow
            GUI.DrawTexture(new Rect(bar.x + 0, bar.y + 6f, bar.width, bar.height), shadowTex);
            GUI.DrawTexture(new Rect(bar.x + 2, bar.y + 4f, bar.width, bar.height), shadowTex);
            // Panel
            GUI.DrawTexture(bar, panelTex);
            // Subtle top highlight
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width, 1f), panelHighlightTex);
            // Border
            DrawBorder(bar, 1f, borderTex);

            // Top accent strip
            var accent = enabledMode ? accentEnabledTex : accentDisabledTex;
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width, 2f), accent);

            float cx = bar.x + padX;

            // --- Left hint (F5 Toggle UI) ---
            var chipL = new Rect(cx, bar.y + (h - 20f) * 0.5f, 30f, 20f);
            DrawKeyChip(chipL, "F5");
            var hintLText = new Rect(chipL.xMax + 8f, bar.y, hintLeftW - 30f - 8f, h);
            GUI.Label(hintLText, "Toggle UI", hintStyle);

            cx += hintLeftW;

            // Divider
            GUI.DrawTexture(new Rect(cx, bar.y + 8f, 1f, h - 16f), dividerTex);
            cx += 9f;

            // --- Status block ---
            // Status dot
            var dotRect = new Rect(cx, bar.y + (h - 10f) * 0.5f, 10f, 10f);
            GUI.DrawTexture(dotRect, accent);
            // Outer glow ring
            DrawBorder(new Rect(dotRect.x - 2, dotRect.y - 2, dotRect.width + 4, dotRect.height + 4), 1f, accent);

            var titleRect = new Rect(dotRect.xMax + 8f, bar.y, 120f, h);
            GUI.Label(titleRect, "STREAMER MODE", titleStyle);

            var valueRect = new Rect(titleRect.xMax + 4f, bar.y, statusW - (titleRect.xMax + 4f - cx), h);
            var prevColor = valueStyle.normal.textColor;
            valueStyle.normal.textColor = enabledMode ? new Color(0.40f, 0.98f, 0.65f) : new Color(0.98f, 0.50f, 0.55f);
            GUI.Label(valueRect, enabledMode ? "ENABLED" : "DISABLED", valueStyle);
            valueStyle.normal.textColor = prevColor;

            cx += statusW;

            // Divider
            GUI.DrawTexture(new Rect(cx, bar.y + 8f, 1f, h - 16f), dividerTex);
            cx += 9f;

            // --- Right hint (F8 Press to Enable/Disable) ---
            var chipR = new Rect(cx, bar.y + (h - 20f) * 0.5f, 30f, 20f);
            DrawKeyChip(chipR, "F8");
            var hintRText = new Rect(chipR.xMax + 8f, bar.y, hintRightW - 30f - 8f, h);
            GUI.Label(hintRText, enabledMode ? "Press to Disable" : "Press to Enable", hintStyle);
        }
    }
}
    

