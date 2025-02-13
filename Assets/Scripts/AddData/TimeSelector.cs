using System;
using DanielLochner.Assets.SimpleScrollSnap;
using TMPro;
using UnityEngine;

namespace AddTask
{
    public class TimeSelector : MonoBehaviour
    {
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;

        [SerializeField] private SimpleScrollSnap _hourScrollSnap;
        [SerializeField] private SimpleScrollSnap _minuteScrollSnap;
        [SerializeField] private SimpleScrollSnap _ampmScrollSnap;
        [SerializeField] private TMP_Text[] _hourText;
        [SerializeField] private TMP_Text[] _minuteText;
        [SerializeField] private TMP_Text[] _ampmText;

        private string _hour;
        private string _minute;
        private string _ampm;

        public event Action<string> HourInputed;
        public event Action<string> MinuteInputed;
        public event Action<string> AmPmInputed;

        public string Hour => _hour;
        public string Minute => _minute;
        public string AmPm => _ampm;

        private void OnEnable()
        {
            _hourScrollSnap.OnPanelCentered.AddListener(SetHour);
            _minuteScrollSnap.OnPanelCentered.AddListener(SetMinute);
            _ampmScrollSnap.OnPanelCentered.AddListener(SetAmPm);
        }

        private void OnDisable()
        {
            _hourScrollSnap.OnPanelCentered.RemoveListener(SetHour);
            _minuteScrollSnap.OnPanelCentered.RemoveListener(SetMinute);
            _ampmScrollSnap.OnPanelCentered.RemoveListener(SetAmPm);
        }

        private void Start()
        {
            InitializeTimeFields();
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            Reset();
            gameObject.SetActive(false);
        }

        private void SetHour(int start, int end)
        {
            _hour = _hourText[start].text;
            SetColorForSelected(_hourText, start);
            HourInputed?.Invoke(_hour);
        }

        private void SetMinute(int start, int end)
        {
            _minute = _minuteText[start].text;
            SetColorForSelected(_minuteText, start);
            MinuteInputed?.Invoke(_minute);
        }

        private void SetAmPm(int start, int end)
        {
            _ampm = _ampmText[start].text;
            SetColorForSelected(_ampmText, start);
            AmPmInputed?.Invoke(_ampm);
        }

        private void InitializeTimeFields()
        {
            PopulateHours();
            PopulateMinutes();
            PopulateAmPm();
            SetColorForSelected(_hourText, 0);
            SetColorForSelected(_minuteText, 0);
            SetColorForSelected(_ampmText, 0);
        }

        private void PopulateHours()
        {
            for (int i = 0; i < _hourText.Length; i++)
            {
                int hour = i % 12 + 1;
                _hourText[i].text = i < 12 ? hour.ToString("00") : "";
            }
        }
        
        public void SetTimeWithScroll(string hour, string minute, string ampm)
        {
            // Find and set the correct indices
            int hourIndex = FindHourIndex(hour);
            int minuteIndex = FindMinuteIndex(minute);
            int ampmIndex = ampm.ToUpper() == "PM" ? 1 : 0;

            // Set the scroll positions
            if (hourIndex >= 0)
                _hourScrollSnap.GoToPanel(hourIndex);
    
            if (minuteIndex >= 0)
                _minuteScrollSnap.GoToPanel(minuteIndex);
    
            _ampmScrollSnap.GoToPanel(ampmIndex);

            // Set the values
            _hour = hour;
            _minute = minute;
            _ampm = ampm;
        }
        
        private int FindHourIndex(string hour)
        {
            if (int.TryParse(hour, out int hourValue))
            {
                for (int i = 0; i < _hourText.Length; i++)
                {
                    if (_hourText[i].text == hour)
                        return i;
                }
            }
            return -1;
        }

        private int FindMinuteIndex(string minute)
        {
            if (int.TryParse(minute, out int minuteValue))
            {
                return minuteValue;
            }
            return -1;
        }
        
        public void SetTime(string hour, string minute, string ampm)
        {
            // These fields should already exist in your TimeSelector class
            _hour = hour;
            _minute = minute;
            _ampm = ampm;
        
            // Update any UI elements that display the time
            // This depends on your specific implementation
        }

        private void PopulateMinutes()
        {
            for (int i = 0; i < _minuteText.Length; i++)
            {
                _minuteText[i].text = i < 60 ? i.ToString("00") : "";
            }
        }

        private void PopulateAmPm()
        {
            if (_ampmText.Length >= 2)
            {
                _ampmText[0].text = "AM";
                _ampmText[1].text = "PM";
            }
        }

        private void SetColorForSelected(TMP_Text[] texts, int selectedIndex)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].color = i == selectedIndex ? _selectedColor : _unselectedColor;
            }
        }

        private void Reset()
        {
            _hourScrollSnap.GoToPanel(0);
            _minuteScrollSnap.GoToPanel(0);
            _ampmScrollSnap.GoToPanel(0);

            _hour = string.Empty;
            _minute = string.Empty;
            _ampm = string.Empty;
        }

        public string GetTime24Hour()
        {
            if (string.IsNullOrEmpty(_hour) || string.IsNullOrEmpty(_minute) || string.IsNullOrEmpty(_ampm))
                return string.Empty;

            int hour = int.Parse(_hour);
            if (_ampm == "PM" && hour != 12)
                hour += 12;
            else if (_ampm == "AM" && hour == 12)
                hour = 0;

            return $"{hour:00}:{_minute}";
        }
    }
}