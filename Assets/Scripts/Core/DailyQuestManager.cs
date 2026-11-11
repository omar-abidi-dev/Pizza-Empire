using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShopIdle.Core
{
    public enum DailyQuestType
    {
        ServeCustomers = 0,
        CollectMoney = 1,
        CleanTables = 2,
        CompleteRestaurant = 3,
        ServeVIP = 4
    }

    /// <summary>
    /// Manages 3 random daily quests that reset at midnight local time.
    /// Quests track progress and award gems on completion.
    /// </summary>
    public class DailyQuestManager : MonoBehaviour
    {
        private static DailyQuestManager _instance;
        public static DailyQuestManager Instance => _instance;

        private List<Save.DailyQuestData> _quests;
        private string _questDate;

        // UI
        private GameObject _panelObj;
        private bool _panelOpen;
        private readonly List<QuestRowUI> _rows = new();

        private struct QuestRowUI
        {
            public Image FillBar;
            public TextMeshProUGUI ProgressText;
            public Button ClaimButton;
            public Image ClaimBG;
        }

        private static readonly string[] QuestTemplates =
        {
            "Serve {0} customers",
            "Collect ${0}",
            "Clean {0} tables",
            "Complete a restaurant",
            "Serve {0} VIP customers"
        };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            LoadOrGenerate();
            CreateQuestButton();
        }

        private void LoadOrGenerate()
        {
            var sm = Save.SaveManager.Instance;
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            if (sm != null && sm.Data != null && sm.Data.dailyQuestDate == today
                && sm.Data.dailyQuests != null && sm.Data.dailyQuests.Count == 3)
            {
                _quests = sm.Data.dailyQuests;
                _questDate = today;
            }
            else
            {
                GenerateNewQuests(today);
                if (sm != null && sm.Data != null)
                {
                    sm.Data.todayCustomersServed = 0;
                    sm.Data.todayMoneyCollected = 0;
                    sm.Data.todayTablesCleaned = 0;
                }
            }
        }

        private void GenerateNewQuests(string date)
        {
            _questDate = date;
            _quests = new List<Save.DailyQuestData>();

            // Always include serve customers and collect money
            _quests.Add(MakeQuest(DailyQuestType.ServeCustomers,
                UnityEngine.Random.Range(3, 8) * 10, UnityEngine.Random.Range(3, 6)));

            _quests.Add(MakeQuest(DailyQuestType.CollectMoney,
                UnityEngine.Random.Range(5, 20) * 100, UnityEngine.Random.Range(3, 6)));

            // Third quest is random
            var pool = new[] { DailyQuestType.CleanTables, DailyQuestType.ServeVIP };
            var type = pool[UnityEngine.Random.Range(0, pool.Length)];
            int target = type == DailyQuestType.CleanTables
                ? UnityEngine.Random.Range(3, 12)
                : UnityEngine.Random.Range(2, 6);
            _quests.Add(MakeQuest(type, target, UnityEngine.Random.Range(4, 8)));

            SaveQuests();
        }

        private Save.DailyQuestData MakeQuest(DailyQuestType type, int target, int gems)
        {
            return new Save.DailyQuestData
            {
                questType = (int)type,
                target = target,
                progress = 0,
                gemReward = gems,
                claimed = false
            };
        }

        // ── Progress Tracking ──

        public void TrackServeCustomer()
        {
            UpdateQuestProgress(DailyQuestType.ServeCustomers, 1);
        }

        public void TrackMoneyCollected(int amount)
        {
            UpdateQuestProgress(DailyQuestType.CollectMoney, amount);
        }

        public void TrackTableCleaned()
        {
            UpdateQuestProgress(DailyQuestType.CleanTables, 1);
        }

        public void TrackVIPServed()
        {
            UpdateQuestProgress(DailyQuestType.ServeVIP, 1);
        }

        private void UpdateQuestProgress(DailyQuestType type, int amount)
        {
            if (_quests == null) return;
            for (int i = 0; i < _quests.Count; i++)
            {
                if (_quests[i].questType == (int)type && !_quests[i].claimed)
                {
                    _quests[i].progress = Mathf.Min(
                        _quests[i].progress + amount, _quests[i].target);
                    break;
                }
            }
            SaveQuests();
            if (_panelOpen) RefreshPanel();
        }

        private void SaveQuests()
        {
            var sm = Save.SaveManager.Instance;
            if (sm == null || sm.Data == null) return;
            sm.Data.dailyQuestDate = _questDate;
            sm.Data.dailyQuests = _quests;
        }

        // ── UI ──

        private void CreateQuestButton()
        {
            var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var btnObj = new GameObject("QuestButton");
            btnObj.transform.SetParent(canvas.transform, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, -10f);
            rect.sizeDelta = new Vector2(70f, 70f);

            var bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.1f, 0.35f, 0.8f);

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(TogglePanel);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Q";
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.85f, 0.3f);
            tmp.fontStyle = FontStyles.Bold;
        }

        private void TogglePanel()
        {
            if (_panelOpen)
            {
                if (_panelObj != null) Destroy(_panelObj);
                _panelOpen = false;
            }
            else
            {
                BuildPanel();
                _panelOpen = true;
            }
        }

        private void BuildPanel()
        {
            var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _rows.Clear();

            _panelObj = new GameObject("QuestPanel");
            _panelObj.transform.SetParent(canvas.transform, false);
            var panelRect = _panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.3f);
            panelRect.anchorMax = new Vector2(0.95f, 0.7f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelBg = _panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.06f, 0.18f, 0.95f);

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.85f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "DAILY QUESTS";
            titleTmp.fontSize = 32;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.85f, 0.3f);
            titleTmp.fontStyle = FontStyles.Bold;

            // Close button
            var closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(_panelObj.transform, false);
            var closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.88f, 0.88f);
            closeRect.anchorMax = new Vector2(0.98f, 0.98f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            var closeBg = closeObj.AddComponent<Image>();
            closeBg.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);
            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(TogglePanel);
            var closeLbl = new GameObject("X");
            closeLbl.transform.SetParent(closeObj.transform, false);
            var closeLblRect = closeLbl.AddComponent<RectTransform>();
            closeLblRect.anchorMin = Vector2.zero;
            closeLblRect.anchorMax = Vector2.one;
            closeLblRect.offsetMin = Vector2.zero;
            closeLblRect.offsetMax = Vector2.zero;
            var closeTmp = closeLbl.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            closeTmp.fontSize = 24;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;

            // Quest rows
            for (int i = 0; i < _quests.Count; i++)
                CreateQuestRow(i);

            RefreshPanel();
        }

        private void CreateQuestRow(int index)
        {
            var q = _quests[index];
            float yMax = 0.80f - index * 0.27f;
            float yMin = yMax - 0.23f;

            var rowObj = new GameObject($"Quest{index}");
            rowObj.transform.SetParent(_panelObj.transform, false);
            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.05f, yMin);
            rowRect.anchorMax = new Vector2(0.95f, yMax);
            rowRect.offsetMin = Vector2.zero;
            rowRect.offsetMax = Vector2.zero;
            var rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.15f, 0.12f, 0.28f, 0.8f);

            // Quest description
            string desc = FormatQuest(q);
            var descObj = new GameObject("Desc");
            descObj.transform.SetParent(rowObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.03f, 0.5f);
            descRect.anchorMax = new Vector2(0.7f, 0.95f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            var descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = desc;
            descTmp.fontSize = 20;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            descTmp.color = Color.white;

            // Progress bar background
            var barBgObj = new GameObject("BarBG");
            barBgObj.transform.SetParent(rowObj.transform, false);
            var barBgRect = barBgObj.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.03f, 0.1f);
            barBgRect.anchorMax = new Vector2(0.7f, 0.4f);
            barBgRect.offsetMin = Vector2.zero;
            barBgRect.offsetMax = Vector2.zero;
            var barBgImg = barBgObj.AddComponent<Image>();
            barBgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Progress bar fill
            var fillObj = new GameObject("BarFill");
            fillObj.transform.SetParent(barBgObj.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.5f, 0.3f, 0.9f, 0.9f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            // Progress text
            var progObj = new GameObject("ProgText");
            progObj.transform.SetParent(barBgObj.transform, false);
            var progRect = progObj.AddComponent<RectTransform>();
            progRect.anchorMin = Vector2.zero;
            progRect.anchorMax = Vector2.one;
            progRect.offsetMin = Vector2.zero;
            progRect.offsetMax = Vector2.zero;
            var progTmp = progObj.AddComponent<TextMeshProUGUI>();
            progTmp.fontSize = 16;
            progTmp.alignment = TextAlignmentOptions.Center;
            progTmp.color = Color.white;

            // Gem reward + claim button
            var claimObj = new GameObject("ClaimBtn");
            claimObj.transform.SetParent(rowObj.transform, false);
            var claimRect = claimObj.AddComponent<RectTransform>();
            claimRect.anchorMin = new Vector2(0.73f, 0.15f);
            claimRect.anchorMax = new Vector2(0.97f, 0.85f);
            claimRect.offsetMin = Vector2.zero;
            claimRect.offsetMax = Vector2.zero;
            var claimBg = claimObj.AddComponent<Image>();
            claimBg.color = new Color(0.3f, 0.7f, 0.3f, 0.9f);
            var claimBtn = claimObj.AddComponent<Button>();
            claimBtn.targetGraphic = claimBg;
            int idx = index;
            claimBtn.onClick.AddListener(() => ClaimQuest(idx));

            var claimLabel = new GameObject("Label");
            claimLabel.transform.SetParent(claimObj.transform, false);
            var claimLabelRect = claimLabel.AddComponent<RectTransform>();
            claimLabelRect.anchorMin = new Vector2(0.42f, 0f);
            claimLabelRect.anchorMax = Vector2.one;
            claimLabelRect.offsetMin = Vector2.zero;
            claimLabelRect.offsetMax = Vector2.zero;
            var claimTmp = claimLabel.AddComponent<TextMeshProUGUI>();
            claimTmp.text = $"{q.gemReward}";
            claimTmp.fontSize = 20;
            claimTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var claimIcon = new GameObject("GemIcon");
            claimIcon.transform.SetParent(claimObj.transform, false);
            var claimIconRect = claimIcon.AddComponent<RectTransform>();
            claimIconRect.anchorMin = new Vector2(0.08f, 0.15f);
            claimIconRect.anchorMax = new Vector2(0.38f, 0.85f);
            claimIconRect.offsetMin = Vector2.zero;
            claimIconRect.offsetMax = Vector2.zero;
            ShopIdle.UI.GemSprite.AddTo(claimIcon);
            claimTmp.color = Color.white;
            claimTmp.fontStyle = FontStyles.Bold;

            _rows.Add(new QuestRowUI
            {
                FillBar = fillImg,
                ProgressText = progTmp,
                ClaimButton = claimBtn,
                ClaimBG = claimBg
            });
        }

        private void RefreshPanel()
        {
            for (int i = 0; i < _rows.Count && i < _quests.Count; i++)
            {
                var q = _quests[i];
                var row = _rows[i];
                float fill = q.target > 0 ? (float)q.progress / q.target : 0f;
                row.FillBar.fillAmount = fill;
                row.ProgressText.text = $"{q.progress}/{q.target}";

                bool complete = q.progress >= q.target;
                row.ClaimButton.interactable = complete && !q.claimed;
                row.ClaimBG.color = q.claimed
                    ? new Color(0.3f, 0.3f, 0.3f, 0.5f)
                    : complete
                        ? new Color(0.3f, 0.7f, 0.3f, 0.9f)
                        : new Color(0.3f, 0.3f, 0.4f, 0.6f);
            }
        }

        private void ClaimQuest(int index)
        {
            if (index < 0 || index >= _quests.Count) return;
            var q = _quests[index];
            if (q.claimed || q.progress < q.target) return;

            q.claimed = true;
            GemManager.Instance?.AddGems(q.gemReward);
            AudioManager.Instance?.Play(SoundType.ChaChing);
            SaveQuests();
            RefreshPanel();
        }

        private string FormatQuest(Save.DailyQuestData q)
        {
            var type = (DailyQuestType)q.questType;
            return type switch
            {
                DailyQuestType.ServeCustomers => string.Format(QuestTemplates[0], q.target),
                DailyQuestType.CollectMoney => string.Format(QuestTemplates[1], q.target),
                DailyQuestType.CleanTables => string.Format(QuestTemplates[2], q.target),
                DailyQuestType.CompleteRestaurant => QuestTemplates[3],
                DailyQuestType.ServeVIP => string.Format(QuestTemplates[4], q.target),
                _ => "Unknown quest"
            };
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
