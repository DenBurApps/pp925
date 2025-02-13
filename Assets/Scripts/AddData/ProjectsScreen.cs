using System;
using System.Collections.Generic;
using System.Linq;
using Bitsplash.DatePicker;
using OpenProject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class TaskData
    {
        public string Name;
        public DateTime DateTime;
        public string Priority;
        public bool IsCompleted;
    }

    public class Project
    {
        public string Name;
        public DateTime Date;
        public List<TaskData> TaskDatas;

        public Project()
        {
            TaskDatas = new List<TaskData>();
        }
    }

    public class ProjectsScreen : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Button _dateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private GameObject _addtaskEmpty;
        [SerializeField] private GameObject _addtaskfilled;
        [SerializeField] private Button _addTaskButton;
        [SerializeField] private TMP_Text _addedTaskCountText;
        [SerializeField] private List<TaskPlane> _taskPlanes;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private AddTaskScreen _addTaskScreen;
        [SerializeField] private GameObject _mainContent;

        #endregion

        #region Private Fields

        private string _name;
        private DateTime _date;
        private List<TaskData> _tasks;
        private Project _projectToEdit;

        #endregion

        #region Events

        public event Action<Project> OnProjectCreated;
        public event Action<Project, Project> OnProjectEdited;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_saveButton == null || _nameInput == null || _datePickerSettings == null ||
                _addTaskScreen == null || _mainContent == null)
            {
                Debug.LogError("Required components are not assigned in the inspector", this);
                enabled = false;
                return;
            }

            _tasks = new List<TaskData>();
            _saveButton.interactable = false;
            _nameInput.onValueChanged.AddListener(OnNameChanged);
        
            _addTaskScreen.gameObject.SetActive(false);
            _mainContent.SetActive(true);
        
            // Clear any existing data when the component awakens
            ClearInputs();
            UpdateTasksUI();
        }

        private void Start()
        {
            ToggleDatePicker(false);
            if (_projectToEdit == null) // If not in edit mode
            {
                _date = DateTime.Now;
                _dateText.text = _date.ToString("MMM dd, yyyy");
            }
        }

        private void OnEnable()
        {
            _dateButton.onClick.AddListener(() => ToggleDatePicker(true));
            _addTaskButton.onClick.AddListener(OnAddTaskClicked);
            _saveButton.onClick.AddListener(OnSaveButtonClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClicked);

            _datePickerSettings.Content.OnSelectionChanged.AddListener(OnDateSelected);

            _addTaskScreen.OnTaskCreated += HandleNewTask;
            _addTaskScreen.OnTaskDeleted += HandleTaskDeleted;
            _addTaskScreen.OnBackPressed += HandleBackFromAddTask;

            foreach (var taskPlane in _taskPlanes)
            {
                taskPlane.OnTaskSelected += OnTaskSelected;
            }

            _dateText.text = _date.ToString("MMM dd, yyyy");
            
            if (_projectToEdit == null && _date == default(DateTime))
            {
                _date = DateTime.Now;
                _dateText.text = _date.ToString("MMM dd, yyyy");
            }

            SetupAddTaskScreen();
        }

        private void OnDisable()
        {
            _dateButton.onClick.RemoveListener(() => ToggleDatePicker(true));
            _addTaskButton.onClick.RemoveListener(OnAddTaskClicked);
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);

            if (_backButton != null)
                _backButton.onClick.RemoveListener(OnBackButtonClicked);
            
            _datePickerSettings.Content.OnSelectionChanged.RemoveListener(OnDateSelected);

            _addTaskScreen.OnTaskCreated -= HandleNewTask;
            _addTaskScreen.OnTaskDeleted -= HandleTaskDeleted;
            _addTaskScreen.OnBackPressed -= HandleBackFromAddTask;

            foreach (var taskPlane in _taskPlanes)
            {
                taskPlane.OnTaskSelected -= OnTaskSelected;
            }
        }

        private void OnDestroy()
        {
            _nameInput.onValueChanged.RemoveListener(OnNameChanged);
        }

        #endregion

        #region Setup Methods

        private void SetupAddTaskScreen()
        {
            if (_addTaskScreen != null)
            {
                _addTaskScreen._projectsScreen = this;
            }
        }

        #endregion

        #region Event Handlers

        public void SetProjectForEdit(Project project)
        {
            _projectToEdit = project;
            _name = project.Name;
            _date = project.Date;
            _tasks = new List<TaskData>(project.TaskDatas); // Clone the tasks list

            // Update UI
            _nameInput.text = _name;
            _dateText.text = _date.ToString("MMM dd, yyyy");
            UpdateTasksUI();
            ValidateInputs();
        }

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

        private void OnAddTaskClicked()
        {
            SetupAddTaskScreen(); // Ensure reference is set before showing screen
            _addTaskScreen.Initialize(null); // Initialize with no existing task
            _addTaskScreen.gameObject.SetActive(true);
        }

        private void OnBackButtonClicked()
        {
            if (_projectToEdit != null)
            {
                var openProjectScreen = FindObjectOfType<OpenProjectScreen>();
                if (openProjectScreen != null)
                {
                    openProjectScreen.OnBackFromEdit();
                }
            }

            gameObject.SetActive(false);
        }

        public void HandleNewTask(TaskData task)
        {
            AddTask(task);
            _mainContent.SetActive(true);
            _addTaskScreen.gameObject.SetActive(false);
        }

        private void HandleTaskDeleted(TaskData task)
        {
            RemoveTask(task);
            _mainContent.SetActive(true);
            _addTaskScreen.gameObject.SetActive(false);
        }

        private void HandleBackFromAddTask()
        {
            if (_projectToEdit != null)
            {
                // We're in edit mode, return to OpenProjectScreen
                var openProjectScreen = FindObjectOfType<OpenProjectScreen>();
                if (openProjectScreen != null)
                {
                    openProjectScreen.OnProjectScreenClosed();
                }
            }

            _mainContent.SetActive(true);
            _addTaskScreen.gameObject.SetActive(false);
        }

        private void OnTaskSelected(TaskData task)
        {
            SetupAddTaskScreen();
            _addTaskScreen.Initialize(task);
            _addTaskScreen.gameObject.SetActive(true);
            _mainContent.SetActive(false);
        }

        private void OnSaveButtonClicked()
        {
            Project project = GetProject();
            if (project != null)
            {
                if (_projectToEdit != null)
                {
                    OnProjectEdited?.Invoke(_projectToEdit, project);
                    _projectToEdit = null;
                }
                else
                {
                    OnProjectCreated?.Invoke(project);
                }

                ClearInputs(); // Clear data after successful save
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Methods

        public void EnableScreen()
        {
            if (_projectToEdit == null)
            {
                ClearInputs();
            }
            _mainContent.SetActive(true);
            UpdateTasksUI();
        }

        public void AddTask(TaskData task)
        {
            if (task == null) return;

            _tasks.Add(task);
            UpdateTasksUI();
            ValidateInputs();
        }

        public void RemoveTask(TaskData task)
        {
            if (task == null) return;

            _tasks.Remove(task);
            UpdateTasksUI();
            ValidateInputs();
        }
        
        public void PrepareForNewProject()
        {
            ClearInputs();
            _date = DateTime.Now;
            _dateText.text = _date.ToString("MMM dd, yyyy");
            _mainContent.SetActive(true);
            UpdateTasksUI();
            ValidateInputs();
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            if (_projectToEdit == null) // Если не в режиме редактирования
            {
                PrepareForNewProject();
            }
        }

        #endregion

        #region Helper Methods

        private void ToggleDatePicker(bool status)
        {
            _datePickerSettings.gameObject.SetActive(status);
        }

        private void UpdateTasksUI()
        {
            _addedTaskCountText.text = _tasks.Count.ToString() + " tasks";
            _addtaskEmpty.SetActive(_tasks.Count == 0);
            _addtaskfilled.SetActive(_tasks.Count > 0);

            // Update task planes
            for (int i = 0; i < _taskPlanes.Count; i++)
            {
                if (i < _tasks.Count)
                {
                    _taskPlanes[i].gameObject.SetActive(true);
                    _taskPlanes[i].SetData(_tasks[i]);
                }
                else
                {
                    _taskPlanes[i].gameObject.SetActive(false);
                    _taskPlanes[i].SetData(null);
                }
            }
        }

        private void ValidateInputs()
        {
            if (_projectToEdit != null)
            {
                _saveButton.interactable = true;
                return;
            }

            bool isValid = !string.IsNullOrEmpty(_name?.Trim()) &&
                           _date != default(DateTime) &&
                           _tasks.Count > 0;

            _saveButton.interactable = isValid;
        }

        private void ClearInputs()
        {
            _nameInput.text = string.Empty;
            _dateText.text = string.Empty;
            _name = string.Empty;
            _date = default(DateTime);
            _tasks.Clear();
            _projectToEdit = null; // Also clear the edit reference

            _datePickerSettings.gameObject.SetActive(false);
            _addTaskScreen.gameObject.SetActive(false);

            UpdateTasksUI();
            ValidateInputs();
        }

        private Project GetProject()
        {
            try
            {
                if (!_saveButton.interactable)
                    return null;

                return new Project
                {
                    Name = _name.Trim(),
                    Date = _date,
                    TaskDatas = new List<TaskData>(_tasks)
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating Project: {e.Message}", this);
                return null;
            }
        }

        #endregion
    }
}