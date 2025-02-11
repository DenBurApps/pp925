using System;
using AddTask;
using Bitsplash.DatePicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class Lesson
    {
        public string Name;
        public DateTime DateTime;
        public bool IsExpanded { get; set; } 
    }
    
    public class LessonsScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Button _dateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private TimeSelector _timeSelector;
        [SerializeField] private Button _timeButton;
        [SerializeField] private GameObject _timeScroll;
        [SerializeField] private Button _saveButton;

        private Lesson _lessonToEdit;
        
        private string _name;
        private DateTime _date;
        private string _time;

        public event Action<Lesson> OnLessonCreated;
        public event Action<Lesson, Lesson> OnLessonEdited; 

        #region Unity Lifecycle
        private void Awake()
        {
            ValidateComponents();
            InitializeComponents();
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
            if (_lessonToEdit == null)
            {
                ResetUI();
                _date = DateTime.Now;
                UpdateDateTimeDisplay();
            }
            
        }
        
        public void SetLessonForEdit(Lesson lesson)
        {
            _lessonToEdit = lesson;
            _name = lesson.Name;
            _date = lesson.DateTime;
            
            // Update UI
            _nameInput.text = _name;
            UpdateDateTimeDisplay();
            
            ValidateInputs();
        }
        
        private void UpdateDateTimeDisplay()
        {
            _dateText.text = _date.ToString("MMM dd, yyyy");
            
            string ampm = _date.Hour >= 12 ? "PM" : "AM";
            int hour = _date.Hour % 12;
            if (hour == 0) hour = 12;
            _time = $"{hour}:{_date.Minute:D2} {ampm}";
            _timeText.text = _time;
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Initialization
        private void ValidateComponents()
        {
            if (_saveButton == null || _nameInput == null || _timeSelector == null || 
                _datePickerSettings == null || _timeText == null || _dateText == null ||
                _timeButton == null || _timeScroll == null || _dateButton == null)
            {
                Debug.LogError("Required components are not assigned in the inspector", this);
                enabled = false;
                return;
            }
        }

        private void InitializeComponents()
        {
            _saveButton.interactable = false;
            _nameInput.onValueChanged.AddListener(OnNameChanged);
            ResetUI();
        }

        private void SubscribeToEvents()
        {
            _dateButton.onClick.AddListener(() => ToggleDatePicker(true));
            _timeSelector.HourInputed += SetHour;
            _timeSelector.MinuteInputed += SetMinute;
            _timeSelector.AmPmInputed += SetAmPm;
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _timeButton.onClick.AddListener(() => ToggleTimePlane(true));
            _datePickerSettings.Content.OnSelectionChanged.AddListener(OnDateSelected);
        }

        private void UnsubscribeFromEvents()
        {
            if (_dateButton != null)
                _dateButton.onClick.RemoveListener(() => ToggleDatePicker(true));
            
            if (_timeSelector != null)
            {
                _timeSelector.HourInputed -= SetHour;
                _timeSelector.MinuteInputed -= SetMinute;
                _timeSelector.AmPmInputed -= SetAmPm;
            }
            
            if (_saveButton != null)
                _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            
            if (_timeButton != null)
                _timeButton.onClick.RemoveListener(() => ToggleTimePlane(true));
            
            if (_datePickerSettings?.Content != null)
                _datePickerSettings.Content.OnSelectionChanged.RemoveListener(OnDateSelected);
            
            if (_nameInput != null)
                _nameInput.onValueChanged.RemoveListener(OnNameChanged);
        }
        #endregion

        #region UI Event Handlers
        private void OnSaveButtonClicked()
        {
            Lesson lesson = GetLesson();
            if (lesson != null)
            {
                if (_lessonToEdit != null)
                {
                    OnLessonEdited?.Invoke(_lessonToEdit, lesson);
                    _lessonToEdit = null;
                }
                else
                {
                    OnLessonCreated?.Invoke(lesson);
                }
                ResetUI();
            }
        }

        private void ToggleTimePlane(bool status)
        {
            if (_timeScroll != null)
            {
                if (_timeScroll.activeSelf)
                {
                    status = false;
                }
                
                _timeScroll.SetActive(status);
                if (!status)
                {
                    ValidateInputs();
                }
            }
        }

        private void ToggleDatePicker(bool status)
        {
            if (_datePickerSettings != null)
            {
                _datePickerSettings.gameObject.SetActive(status);
                if (!status)
                {
                    ValidateInputs();
                }
            }
        }
        #endregion

        #region Input Handlers
        private void OnNameChanged(string value)
        {
            _name = value?.Trim();
            ValidateInputs();
        }

        private void OnDateSelected()
        {
            if (_datePickerSettings?.Content != null)
            {
                _date = _datePickerSettings.Content.Selection.GetItem(0);
                _dateText.text = _date.ToString("MMM dd, yyyy");
                ToggleDatePicker(false);
                ValidateInputs();
            }
        }

        private void SetHour(string hour)
        {
            UpdateTimeText();
        }

        private void SetMinute(string minute)
        {
            UpdateTimeText();
        }

        private void SetAmPm(string ampm)
        {
            UpdateTimeText();
        }

        private void UpdateTimeText()
        {
            if (_timeSelector != null && _timeText != null)
            {
                _time = $"{_timeSelector.Hour}:{_timeSelector.Minute} {_timeSelector.AmPm}";
                _timeText.text = _time;
                ValidateInputs();
            }
        }
        #endregion

        #region Helper Methods
        private void ValidateInputs()
        {
            if (_saveButton == null) return;

            bool isValid = !string.IsNullOrEmpty(_name?.Trim()) &&
                          !string.IsNullOrEmpty(_time) &&
                          _date != default(DateTime) &&
                          IsValidTime();

            _saveButton.interactable = isValid;
        }

        private bool IsValidTime()
        {
            if (_timeSelector == null) return false;
            
            if (!int.TryParse(_timeSelector.Hour, out int hour) ||
                !int.TryParse(_timeSelector.Minute, out int minute))
                return false;

            return hour >= 1 && hour <= 12 &&
                   minute >= 0 && minute <= 59 &&
                   (_timeSelector.AmPm.ToUpper() == "AM" || _timeSelector.AmPm.ToUpper() == "PM");
        }

        private void ResetUI()
        {
            if (_nameInput != null)
                _nameInput.text = string.Empty;
            
            if (_timeText != null)
                _timeText.text = string.Empty;
            
            if (_dateText != null)
                _dateText.text = string.Empty;

            _time = string.Empty;
            _name = string.Empty;
            _date = default(DateTime);

            ToggleTimePlane(false);
            ToggleDatePicker(false);

            ValidateInputs();
        }

        public Lesson GetLesson()
        {
            try
            {
                if (!_saveButton.interactable || _timeSelector == null)
                    return null;

                if (!int.TryParse(_timeSelector.Hour, out int hour) ||
                    !int.TryParse(_timeSelector.Minute, out int minute))
                {
                    Debug.LogError("Invalid time format", this);
                    return null;
                }

                if (hour < 1 || hour > 12 || minute < 0 || minute > 59)
                {
                    Debug.LogError("Time values out of range", this);
                    return null;
                }

                string ampm = _timeSelector.AmPm.ToUpper();
                if (ampm != "AM" && ampm != "PM")
                {
                    Debug.LogError("Invalid AM/PM value", this);
                    return null;
                }

                if (ampm == "PM" && hour != 12)
                    hour += 12;
                else if (ampm == "AM" && hour == 12)
                    hour = 0;

                DateTime dateTime = _date.Date + new TimeSpan(hour, minute, 0);

                return new Lesson
                {
                    Name = _name.Trim(),
                    DateTime = dateTime
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating Lesson: {e.Message}", this);
                return null;
            }
        }
        #endregion
    }
}