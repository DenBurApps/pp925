using System;
using AddTask;
using Bitsplash.DatePicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class AddTaskScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Button _dateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private TimeSelector _timeSelector;
        [SerializeField] private Button _timeButton;
        [SerializeField] private GameObject _timeScroll;
        [SerializeField] private TMP_Text _priorityText;
        [SerializeField] private GameObject _priorityPlane;
        [SerializeField] private FilterButton[] _filterButtons;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _priorityButton;
        [SerializeField] public ProjectsScreen _projectsScreen;

        private string _name;
        private DateTime _date;
        private string _time;
        private string _priority;
        private TaskData _existingTask; // Store reference to existing task if in edit mode
        private bool _isEditMode; // Flag to track if we're editing an existing task

        public event Action<TaskData> OnTaskCreated;
        public event Action<TaskData> OnTaskDeleted; // New event for deletion
        public event Action OnBackPressed;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_saveButton == null || _nameInput == null || _timeSelector == null ||
                _datePickerSettings == null || _backButton == null || _deleteButton == null)
            {
                Debug.LogError("Required components are not assigned in the inspector", this);
                enabled = false;
                return;
            }

            _saveButton.interactable = false;
            _nameInput.onValueChanged.AddListener(OnNameChanged);
        }

        private void Start()
        {
            ToggleDatePicker(false);
            ToggleTimePlane(false);
        }

        private void OnEnable()
        {
            // Set default date and time
            _date = DateTime.Now;
            _dateText.text = _date.ToString("MMM dd, yyyy");
            
            string ampm = DateTime.Now.Hour >= 12 ? "PM" : "AM";
            int hour = DateTime.Now.Hour % 12;
            if (hour == 0) hour = 12;
            _timeText.text = $"{hour}:{DateTime.Now.Minute:D2} {ampm}";

            _dateButton.onClick.AddListener(() => ToggleDatePicker(true));
            _timeSelector.HourInputed += SetHour;
            _timeSelector.MinuteInputed += SetMinute;
            _timeSelector.AmPmInputed += SetAmPm;
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _timeButton.onClick.AddListener(() => ToggleTimePlane(true));
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _priorityButton.onClick.RemoveAllListeners();
            _priorityButton.onClick.AddListener(() => TogglePriority(true));

            _datePickerSettings.Content.OnSelectionChanged.AddListener(OnDateSelected);
            
            foreach (var toggle in _filterButtons)
            {
                toggle.FilterClicked += ApplyPriority;
            }
        }

        private void OnDisable()
        {
            _dateButton.onClick.RemoveAllListeners();
            _timeSelector.HourInputed -= SetHour;
            _timeSelector.MinuteInputed -= SetMinute;
            _timeSelector.AmPmInputed -= SetAmPm;
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            _timeButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);

            _datePickerSettings.Content.OnSelectionChanged.RemoveListener(OnDateSelected);

            foreach (var toggle in _filterButtons)
            {
                toggle.FilterClicked -= ApplyPriority;
            }
        }

        private void OnDestroy()
        {
            _nameInput.onValueChanged.RemoveListener(OnNameChanged);
        }

        #endregion

        #region Public Methods

        public void Initialize(TaskData existingTask = null)
        {
            _existingTask = existingTask;
            _isEditMode = existingTask != null;

            if (_isEditMode)
            {
                Debug.Log(_isEditMode);
                PopulateExistingTaskData(existingTask);
                _saveButton.gameObject.SetActive(false); // Enable save button in edit mode
                _deleteButton.gameObject.SetActive(true);
            }
            else
            {
                ClearInputs();
                _saveButton.interactable = false;
                _deleteButton.gameObject.SetActive(false);
            }
        }

        public void TogglePriorityPlane(bool status)
        {
            _priorityPlane.SetActive(status);
        }

        #endregion

        #region UI Event Handlers

        private void OnSaveButtonClicked()
        {
            TaskData task = GetTask();
            if (task != null)
            {
                /*OnTaskCreated?.Invoke(task);*/
                _projectsScreen.AddTask(task);
                ClearInputs();
                _projectsScreen.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        private void OnDeleteButtonClicked()
        {
            if (_existingTask != null)
            {
                _projectsScreen.RemoveTask(_existingTask);
                OnTaskDeleted?.Invoke(_existingTask);
                ClearInputs();
                _projectsScreen.EnableScreen();
                gameObject.SetActive(false);
            }
        }

        private void OnBackButtonClicked()
        {
            OnBackPressed?.Invoke();
            // Don't set ProjectsScreen active here since it might be coming from OpenProjectScreen
            gameObject.SetActive(false);
        }

        private void ToggleDatePicker(bool status)
        {
            _datePickerSettings.gameObject.SetActive(status);
        }

        private void TogglePriority(bool status)
        {
            // Reset the status based on current state
            status = !_priorityPlane.activeSelf;
            _priorityPlane.SetActive(status);
        }

        private void ToggleTimePlane(bool status)
        {
            if (_timeScroll != null)
            {
                // Если открыто и пытаемся открыть - закрываем
                if (_timeScroll.activeSelf && status)
                {
                    _timeScroll.SetActive(false);
                    ValidateInputs();
                    return;
                }
        
                _timeScroll.SetActive(status);
        
                // Инициализируем время при открытии
                if (status)
                {
                    DateTime timeToSet = _existingTask != null ? _existingTask.DateTime : DateTime.Now;
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

        private void ApplyPriority(FilterButton button, string filterName)
        {
            foreach (var toggle in _filterButtons)
            {
                toggle.SetStatus(toggle == button);
            }

            _priorityPlane.SetActive(false);
            _priorityText.text = filterName;
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

        private void OnDateSelected()
        {
            _date = _datePickerSettings.Content.Selection.GetItem(0);
            _dateText.text = _date.ToString("MMM dd, yyyy");
            ValidateInputs();
            ToggleDatePicker(false);
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
            bool isValid = !string.IsNullOrEmpty(_name?.Trim()) &&
                           !string.IsNullOrEmpty(_time) &&
                           !string.IsNullOrEmpty(_priority) &&
                           _date != default(DateTime) &&
                           IsValidTime();

            _saveButton.interactable = !_isEditMode && isValid;
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
        
        private void PopulateExistingTaskData(TaskData task)
        {
            _nameInput.text = task.Name;
            _name = task.Name;

            _date = task.DateTime;
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

            _priority = task.Priority;
            _priorityText.text = task.Priority;

            foreach (var button in _filterButtons)
            {
                if (button.name.Equals(task.Priority, StringComparison.OrdinalIgnoreCase))
                {
                    button.SetStatus(true);
                    break;
                }
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

        private void ClearInputs()
        {
            _nameInput.text = string.Empty;
            _timeText.text = string.Empty;
            _dateText.text = string.Empty;
            _priorityText.text = string.Empty;
            _time = string.Empty;
            _name = string.Empty;
            _priority = string.Empty;
            _date = default(DateTime);
            _existingTask = null;
            _isEditMode = false;

            foreach (var toggle in _filterButtons)
            {
                toggle.SetStatus(false);
            }

            _priorityPlane.SetActive(false);
            _priorityButton.interactable = true;
            _timeScroll.SetActive(false);
            _datePickerSettings.gameObject.SetActive(false);

            ValidateInputs();
        }

        public TaskData GetTask()
        {
            try
            {
                if (!_saveButton.interactable)
                    return null;

                DateTime dateTime = _date;

                int hour = int.Parse(_timeSelector.Hour);
                int minute = int.Parse(_timeSelector.Minute);

                if (_timeSelector.AmPm.ToUpper() == "PM" && hour != 12)
                    hour += 12;
                else if (_timeSelector.AmPm.ToUpper() == "AM" && hour == 12)
                    hour = 0;

                dateTime = dateTime.Date + new TimeSpan(hour, minute, 0);

                return new TaskData
                {
                    Name = _name.Trim(),
                    DateTime = dateTime,
                    Priority = _priority
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating Task: {e.Message}", this);
                return null;
            }
        }

        #endregion
    }
}