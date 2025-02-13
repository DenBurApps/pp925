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
        #region Serialized Fields
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Button _dateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private TimeSelector _timeSelector;
        [SerializeField] private Button _timeButton;
        [SerializeField] private GameObject _timeScroll;
        [SerializeField] private Button _saveButton;
        #endregion

        #region Private Fields
        private Lesson _lessonToEdit;
        private string _name;
        private DateTime _date;
        private string _time;
        private bool _componentsValidated;
        private bool _isInitialized;
        #endregion

        #region Events
        public event Action<Lesson> OnLessonCreated;
        public event Action<Lesson, Lesson> OnLessonEdited;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            ValidateComponents();
            if (_componentsValidated)
            {
                InitializeComponents();
                _isInitialized = true;
            }
        }

        private void OnEnable()
        {
            if (_componentsValidated)
            {
                SubscribeToEvents();
                if (_lessonToEdit == null)
                {
                    ResetUI();
                    _date = DateTime.Now;
                    UpdateDateTimeDisplay();
                }
            }
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

        #region Button Click Handlers
        private void OnTimeButtonClick()
        {
            ToggleTimePlane(true);
        }

        private void OnDateButtonClick()
        {
            ToggleDatePicker(true);
        }

        private void OnSaveButtonClick()
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
        #endregion

        #region Initialization
        public void SetLessonForEdit(Lesson lesson)
        {
            _lessonToEdit = lesson;
            _name = lesson.Name;
            _date = lesson.DateTime;

            // Update UI
            _nameInput.text = _name;
            UpdateDateTimeDisplay();

            // Initialize TimeSelector with existing time
            if (_timeSelector != null)
            {
                int hour = _date.Hour % 12;
                if (hour == 0) hour = 12;
                string ampm = _date.Hour >= 12 ? "PM" : "AM";

                _timeSelector.SetTimeWithScroll(
                    hour.ToString("00"),
                    _date.Minute.ToString("00"),
                    ampm
                );
            }

            ValidateInputs();
        }

        private void ValidateComponents()
        {
            _componentsValidated = true;

            if (_saveButton == null)
            {
                Debug.LogError("SaveButton is not assigned", this);
                _componentsValidated = false;
            }

            if (_nameInput == null)
            {
                Debug.LogError("NameInput is not assigned", this);
                _componentsValidated = false;
            }

            if (_timeSelector == null)
            {
                Debug.LogError("TimeSelector is not assigned", this);
                _componentsValidated = false;
            }

            if (_datePickerSettings == null)
            {
                Debug.LogError("DatePickerSettings is not assigned", this);
                _componentsValidated = false;
            }

            if (_timeText == null)
            {
                Debug.LogError("TimeText is not assigned", this);
                _componentsValidated = false;
            }

            if (_dateText == null)
            {
                Debug.LogError("DateText is not assigned", this);
                _componentsValidated = false;
            }

            if (_timeButton == null)
            {
                Debug.LogError("TimeButton is not assigned", this);
                _componentsValidated = false;
            }

            if (_timeScroll == null)
            {
                Debug.LogError("TimeScroll is not assigned", this);
                _componentsValidated = false;
            }

            if (_dateButton == null)
            {
                Debug.LogError("DateButton is not assigned", this);
                _componentsValidated = false;
            }

            if (!_componentsValidated)
            {
                enabled = false;
            }
        }

        private void InitializeComponents()
        {
            _saveButton.interactable = false;
            ResetUI();
        }

        private void SubscribeToEvents()
        {
            Debug.Log("Subscribing to events");
            
            if (!_isInitialized) 
            {
                Debug.LogWarning("Not initialized, skipping event subscription");
                return;
            }

            // Remove existing listeners first
            UnsubscribeFromEvents();

            // Input field events
            if (_nameInput != null)
            {
                Debug.Log("Adding name input listener");
                _nameInput.onValueChanged.AddListener(OnNameChanged);
            }
            else
            {
                Debug.LogError("_nameInput is null during subscription");
            }

            // Button click events
            _timeButton?.onClick.AddListener(OnTimeButtonClick);
            _dateButton?.onClick.AddListener(OnDateButtonClick);
            _saveButton?.onClick.AddListener(OnSaveButtonClick);

            // TimeSelector events
            if (_timeSelector != null)
            {
                _timeSelector.HourInputed += SetHour;
                _timeSelector.MinuteInputed += SetMinute;
                _timeSelector.AmPmInputed += SetAmPm;
            }

            // DatePicker events
            if (_datePickerSettings?.Content != null)
            {
                _datePickerSettings.Content.OnSelectionChanged.AddListener(OnDateSelected);
            }
        }

        private void UnsubscribeFromEvents()
        {
            Debug.Log("Unsubscribing from events");
            
            // Input field events
            if (_nameInput != null)
            {
                Debug.Log("Removing name input listener");
                _nameInput.onValueChanged.RemoveListener(OnNameChanged);
            }

            // Button click events
            if (_timeButton != null)
            {
                _timeButton.onClick.RemoveListener(OnTimeButtonClick);
            }

            if (_dateButton != null)
            {
                _dateButton.onClick.RemoveListener(OnDateButtonClick);
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveListener(OnSaveButtonClick);
            }

            // TimeSelector events
            if (_timeSelector != null)
            {
                _timeSelector.HourInputed -= SetHour;
                _timeSelector.MinuteInputed -= SetMinute;
                _timeSelector.AmPmInputed -= SetAmPm;
            }

            // DatePicker events
            if (_datePickerSettings?.Content != null)
            {
                _datePickerSettings.Content.OnSelectionChanged.RemoveListener(OnDateSelected);
            }
        }
        #endregion

        #region UI Event Handlers
        private void ToggleTimePlane(bool status)
        {
            if (_timeScroll != null)
            {
                // If open and trying to open - close
                if (_timeScroll.activeSelf && status)
                {
                    _timeScroll.SetActive(false);
                    ValidateInputs();
                    return;
                }

                _timeScroll.SetActive(status);

                // Initialize time when opening
                if (status)
                {
                    DateTime timeToSet = _lessonToEdit != null ? _lessonToEdit.DateTime : DateTime.Now;
                    int hour = timeToSet.Hour % 12;
                    if (hour == 0) hour = 12;
                    string ampm = timeToSet.Hour >= 12 ? "PM" : "AM";

                    _timeSelector.SetTimeWithScroll(
                        hour.ToString("00"),
                        timeToSet.Minute.ToString("00"),
                        ampm
                    );
                }

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
            Debug.Log($"Name changed to: {value}");
            _name = value?.Trim();
            Debug.Log($"Trimmed name: {_name}");
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

        private void UpdateDateTimeDisplay()
        {
            _dateText.text = _date.ToString("MMM dd, yyyy");

            string ampm = _date.Hour >= 12 ? "PM" : "AM";
            int hour = _date.Hour % 12;
            if (hour == 0) hour = 12;
            _time = $"{hour}:{_date.Minute:D2} {ampm}";
            _timeText.text = _time;
        }
        #endregion

        #region Helper Methods
        private void ValidateInputs()
        {
            if (_saveButton == null) return;

            bool nameValid = !string.IsNullOrEmpty(_name?.Trim());
            bool timeValid = !string.IsNullOrEmpty(_time);
            bool dateValid = _date != default(DateTime);
            bool timeFormatValid = IsValidTime();

            bool isValid = nameValid && timeValid && dateValid && timeFormatValid;

            Debug.Log($"Validation: Name:{nameValid}, Time:{timeValid}, Date:{dateValid}, TimeFormat:{timeFormatValid}");

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