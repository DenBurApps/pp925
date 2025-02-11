using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class TaskPlane : MonoBehaviour
    {
        [SerializeField] private Button _detailButton;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _timeText;

        private TaskData _currentData;

        public event Action<TaskData> OnTaskSelected;

        private void Awake()
        {
            if (_detailButton == null || _nameText == null || _timeText == null)
            {
                Debug.LogError("Required components are not assigned in the inspector", this);
                enabled = false;
                return;
            }

            _detailButton.onClick.AddListener(OnDetailButtonClicked);
        }

        private void OnDestroy()
        {
            _detailButton.onClick.RemoveListener(OnDetailButtonClicked);
        }

        private void OnDetailButtonClicked()
        {
            OnTaskSelected?.Invoke(_currentData);
        }

        public void SetData(TaskData data)
        {
            _currentData = data;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_currentData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            _nameText.text = _currentData.Name;
            _timeText.text = _currentData.DateTime.ToString("HH:mm");
        }
    }
}