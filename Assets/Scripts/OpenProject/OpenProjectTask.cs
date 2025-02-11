using System;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenProject
{
    public class OpenProjectTask : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private GameObject _toggleImage;
        [SerializeField] private Button _toggleButton;

        public TaskData Data { get; private set; }
        public event Action<TaskData> OnTaskCompleted; // New event

        private void Awake()
        {
            _toggleButton.onClick.AddListener(OnToggleClicked);
        }

        private void OnDestroy()
        {
            _toggleButton.onClick.RemoveListener(OnToggleClicked);
        }

        public void Initialize(TaskData taskData)
        {
            Data = taskData;
            _nameText.text = Data.Name;
            _timeText.text = Data.DateTime.ToString($"{Data.DateTime:HH:mm}");
            _toggleImage.SetActive(Data.IsCompleted);
            gameObject.SetActive(true);
        }

        private void OnToggleClicked()
        {
            Data.IsCompleted = !Data.IsCompleted;
            _toggleImage.SetActive(Data.IsCompleted);
            OnTaskCompleted?.Invoke(Data); // Notify listeners when task completion status changes
        }

        private string FormatTime(float timeInSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
            if (time.Hours > 0) return $"{time.Hours}h {time.Minutes}m";
            if (time.Minutes > 0) return $"{time.Minutes}m {time.Seconds}s";
            return $"{time.Seconds}s";
        }
    }
}