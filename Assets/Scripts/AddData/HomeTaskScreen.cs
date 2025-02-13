using System;
using System.Collections.Generic;
using AddTask;
using Bitsplash.DatePicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class HomeTask
    {
        public string Name;
        public string SubjectName;
        public DateTime DateTime;
        public string Priority;
        public bool IsExpanded { get; set; }
    }

    public class HomeTaskScreen : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _subjectInput;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Button _dateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private TimeSelector _timeSelector;
        [SerializeField] private Button _priorityButton;
        [SerializeField] private GameObject _priorityPlane;
        [SerializeField] private Button _timeButton;
        [SerializeField] private GameObject _timeScroll;
        [SerializeField] private TMP_Text _priorityText;
        [SerializeField] private FilterButton[] _filterButtons;
        [SerializeField] private Button _saveButton;

        #endregion

        #region Private Fields

        private string _name;
        private string _subject;
        private DateTime _date;
        private string _time;
        private string _priority;
        private bool _componentsValidated;
        private bool _isInitialized;
        private HomeTask _taskToEdit;

        #endregion

        #region Events

        public event Action<HomeTask> OnHomeTaskCreated;
        public event Action<HomeTask, HomeTask> OnHomeTaskEdited;

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

        private void Start()
        {
            if (_componentsValidated)
            {
                InitializeDefaultDateTime();
            }
        }

        private void OnEnable()
        {
            ValidateComponents();
            if (_componentsValidated)
            {
                SubscribeToEvents();
                InitializeDefaultDateTime();
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

        private void OnPriorityButtonClick()
        {
            ToggleFilterPanel(true);
        }

        private void OnSaveButtonClick()
        {
            HomeTask homeTask = GetHomeTask();
            if (homeTask != null)
            {
                if (_taskToEdit != null)
                {
                    Debug.Log($"Edited task: {homeTask.Name}");
                    OnHomeTaskEdited?.Invoke(_taskToEdit, homeTask);
                    _taskToEdit = null;
                }
                else
                {
                    Debug.Log($"Created new task: {homeTask.Name}");
                    OnHomeTaskCreated?.Invoke(homeTask);
                }
                ResetUI();
            }
            else
            {
                Debug.LogError("Failed to create/edit HomeTask - validation failed");
            }
        }

        #endregion

        #region Initialization
        
        public void SetHomeTaskForEdit(HomeTask task)
        {
            _taskToEdit = task;
            _name = task.Name;
            _subject = task.SubjectName;
            _date = task.DateTime;
            _priority = task.Priority;

            // Update UI
            _nameInput.text = _name;
            _subjectInput.text = _subject;
            UpdateDateTimeDisplay();
            _priorityText.text = _priority;

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

            // Set priority button state
            if (_filterButtons != null)
            {
                foreach (var button in _filterButtons)
                {
                    if (button != null)
                    {
                        button.SetStatus(button.name == _priority);
                    }
                }
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

            if (_subjectInput == null)
            {
                Debug.LogError("SubjectInput is not assigned", this);
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

            if (_priorityButton == null)
            {
                Debug.LogError("PriorityButton is not assigned", this);
                _componentsValidated = false;
            }

            if (_priorityPlane == null)
            {
                Debug.LogError("PriorityPlane is not assigned", this);
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

            if (_priorityText == null)
            {
                Debug.LogError("PriorityText is not assigned", this);
                _componentsValidated = false;
            }

            if (_filterButtons == null || _filterButtons.Length == 0)
            {
                Debug.LogError("FilterButtons array is empty or not assigned", this);
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
            _nameInput.onValueChanged.AddListener(OnNameChanged);
            _subjectInput.onValueChanged.AddListener(OnSubjectChanged);
        }

        private void InitializeDefaultDateTime()
        {
            _date = DateTime.Now;
            UpdateDateText();

            int hour = _date.Hour % 12;
            if (hour == 0) hour = 12;
            string ampm = _date.Hour >= 12 ? "PM" : "AM";
            
            _timeText.text = $"{_date.Hour}:{_date.Minute} {ampm}";
        }

        private void SubscribeToEvents()
        {
            if (!_isInitialized) return;

            // Remove existing listeners first
            UnsubscribeFromEvents();

            // Button click events
            _timeButton?.onClick.AddListener(OnTimeButtonClick);
            _dateButton?.onClick.AddListener(OnDateButtonClick);
            _priorityButton?.onClick.AddListener(OnPriorityButtonClick);
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

            // Filter buttons events
            if (_filterButtons != null)
            {
                foreach (var button in _filterButtons)
                {
                    if (button != null)
                    {
                        button.FilterClicked += ApplyPriority;
                    }
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            // Button click events
            if (_timeButton != null)
            {
                _timeButton.onClick.RemoveAllListeners();
            }
            
            if (_dateButton != null)
            {
                _dateButton.onClick.RemoveAllListeners();
            }
            
            if (_priorityButton != null)
            {
                _priorityButton.onClick.RemoveAllListeners();
            }
            
            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveAllListeners();
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
                _datePickerSettings.Content.OnSelectionChanged.RemoveAllListeners();
            }

            // Filter buttons events
            if (_filterButtons != null)
            {
                foreach (var button in _filterButtons)
                {
                    if (button != null)
                    {
                        button.FilterClicked -= ApplyPriority;
                    }
                }
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
                    DateTime timeToSet = _taskToEdit != null ? _taskToEdit.DateTime : DateTime.Now;
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
                if (status)
                {
                    _priorityPlane?.SetActive(false);
                    _timeScroll?.SetActive(false);
                }
            }
        }

        private void ToggleFilterPanel(bool status)
        {
            if (_priorityPlane != null)
            {
                _priorityPlane.SetActive(status);
                if (status)
                {
                    _timeScroll?.SetActive(false);
                    _datePickerSettings?.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyPriority(FilterButton button, string filterName)
        {
            if (_filterButtons == null || button == null) return;

            foreach (var toggle in _filterButtons)
            {
                if (toggle != null)
                {
                    toggle.SetStatus(toggle == button);
                }
            }

            ToggleFilterPanel(false);

            if (_priorityText != null)
            {
                _priorityText.text = filterName;
            }

            _priority = filterName;
            ValidateInputs();
        }

        #endregion

        #region Input Handlers

        private void OnNameChanged(string value)
        {
            _name = value?.Trim();
            ValidateInputs();
        }

        private void OnSubjectChanged(string value)
        {
            _subject = value?.Trim();
            ValidateInputs();
        }

        private void OnDateSelected()
        {
            if (_datePickerSettings?.Content != null)
            {
                _date = _datePickerSettings.Content.Selection.GetItem(0);
                UpdateDateText();
                ToggleDatePicker(false);
                ValidateInputs();
            }
        }

        private void UpdateDateText()
        {
            if (_dateText != null)
            {
                _dateText.text = _date.ToString("MMM dd, yyyy");
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
        
        private void UpdateDateTimeDisplay()
        {
            _dateText.text = _date.ToString("MMM dd, yyyy");
    
            string ampm = _date.Hour >= 12 ? "PM" : "AM";
            int hour = _date.Hour % 12;
            if (hour == 0) hour = 12;
            _time = $"{hour}:{_date.Minute:D2} {ampm}";
            _timeText.text = _time;
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

            bool nameValid = !string.IsNullOrEmpty(_name?.Trim());
            bool subjectValid = !string.IsNullOrEmpty(_subject?.Trim());
            bool timeValid = !string.IsNullOrEmpty(_time);
            bool priorityValid = !string.IsNullOrEmpty(_priority);
            bool dateValid = _date != default(DateTime);
            bool timeFormatValid = IsValidTime();

            bool isValid = nameValid && subjectValid && timeValid &&
                           priorityValid && dateValid && timeFormatValid;

            _saveButton.interactable = isValid;
        }

        private bool IsValidTime()
        {
            if (_timeSelector == null) return false;

            if (!int.TryParse(_timeSelector.Hour, out int hour) ||
                !int.TryParse(_timeSelector.Minute, out int minute))
            {
                return false;
            }

            bool validHour = hour >= 1 && hour <= 12;
            bool validMinute = minute >= 0 && minute <= 59;
            bool validAmPm = _timeSelector.AmPm.ToUpper() == "AM" ||
                             _timeSelector.AmPm.ToUpper() == "PM";

            return validHour && validMinute && validAmPm;
        }

        private void ResetUI()
        {
            if (_nameInput != null)
                _nameInput.text = string.Empty;

            if (_subjectInput != null)
                _subjectInput.text = string.Empty;

            if (_timeText != null)
                _timeText.text = string.Empty;

            if (_dateText != null)
                _dateText.text = string.Empty;

            if (_priorityText != null)
                _priorityText.text = string.Empty;

            _time = string.Empty;
            _name = string.Empty;
            _subject = string.Empty;
            _priority = string.Empty;
            _date = default(DateTime);

            if (_filterButtons != null)
            {
                foreach (var toggle in _filterButtons)
                {
                    if (toggle != null)
                    {
                        toggle.SetStatus(false);
                    }
                }
            }

            ToggleFilterPanel(false);
            ToggleTimePlane(false);
            ToggleDatePicker(false);

            InitializeDefaultDateTime();
            ValidateInputs();
        }

        public HomeTask GetHomeTask()
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

                return new HomeTask
                {
                    Name = _name.Trim(),
                    SubjectName = _subject.Trim(),
                    DateTime = dateTime,
                    Priority = _priority
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating HomeTask: {e.Message}", this);
                return null;
            }
        }

        #endregion
    }
}