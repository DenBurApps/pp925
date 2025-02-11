using System;
using System.Collections.Generic;
using System.Linq;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenProject
{
    public class OpenProjectScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _projectName;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private GameObject _confirmDeletePlane;
        [SerializeField] private GameObject _editDeleteProjectPlane;

        [SerializeField] private Button _editButton;
        [SerializeField] private Button _firstDeleteButton;
        [SerializeField] private Button _confirmDeleteButton;
        [SerializeField] private List<OpenProjectTask> _tasks;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _openEditDeletePlaneButton;

        [SerializeField] private MainScreenController _mainScreenController;
        [SerializeField] private ProjectsScreen _projectsScreen;

        [SerializeField] private DataManager _dataManager;

        public event Action<Project> OnProjectUpdated;

        public Project Project { get; private set; }

        private void Awake()
        {
            _editButton.onClick.AddListener(OnEditButtonClicked);
            _firstDeleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _confirmDeleteButton.onClick.AddListener(ConfirmDelete);
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _openEditDeletePlaneButton.onClick.AddListener(OnEditDeleteButtonClicked);
        }

        private void OnDestroy()
        {
            _editButton.onClick.RemoveListener(OnEditButtonClicked);
            _firstDeleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            _confirmDeleteButton.onClick.RemoveListener(ConfirmDelete);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _openEditDeletePlaneButton.onClick.RemoveListener(OnEditDeleteButtonClicked);
        }

        public void OpenScreen(Project project)
        {
            Project = project;
            _projectName.text = Project.Name;
            _dateText.text = Project.Date.ToString("MMM dd, yyyy");
    
            RefreshTaskList();
        }
        
        private void RefreshTaskList()
        {
            // Deactivate all tasks first
            _tasks.ForEach(task => task.gameObject.SetActive(false));

            // Initialize tasks and subscribe to their events
            for (int i = 0; i < Project.TaskDatas.Count; i++)
            {
                if (i >= _tasks.Count)
                {
                    Debug.LogWarning("Not enough task objects in the pool!");
                    break;
                }

                var taskUI = _tasks[i];
                taskUI.gameObject.SetActive(true);
                taskUI.Initialize(Project.TaskDatas[i]);
                taskUI.OnTaskCompleted += HandleTaskCompleted;
            }
        }
        
        public void OnProjectScreenClosed()
        {
            RefreshTaskList();
            gameObject.SetActive(true);
        }
        
        private void HandleTaskCompleted(TaskData task)
        {
            // Notify listeners that project has been updated
            OnProjectUpdated?.Invoke(Project);
        }

        private void OnEditButtonClicked()
        {
            _editDeleteProjectPlane.SetActive(false);

            if (_projectsScreen != null)
            {
                gameObject.SetActive(false);
                _projectsScreen.gameObject.SetActive(true);
                _projectsScreen.SetProjectForEdit(Project);

                // Subscribe to the edit events temporarily
                _projectsScreen.OnProjectEdited += HandleProjectEdited;
            }
        }

        private void HandleProjectEdited(Project oldProject, Project newProject)
        {
            // Update the current project with the edited version
            Project = newProject;

            // Update the UI
            _projectName.text = Project.Name;
            _dateText.text = Project.Date.ToString("MMM dd, yyyy");

            // Refresh tasks
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (i < Project.TaskDatas.Count)
                {
                    _tasks[i].gameObject.SetActive(true);
                    _tasks[i].Initialize(Project.TaskDatas[i]);
                }
                else
                {
                    _tasks[i].gameObject.SetActive(false);
                }
            }

            // Unsubscribe from the event to prevent memory leaks
            _projectsScreen.OnProjectEdited -= HandleProjectEdited;

            // Show this screen again
            gameObject.SetActive(true);
            _projectsScreen.gameObject.SetActive(false);
        }

        public void OnBackFromEdit()
        {
            // Unsubscribe from the event if we're going back without saving
            _projectsScreen.OnProjectEdited -= HandleProjectEdited;

            gameObject.SetActive(true);
            _projectsScreen.gameObject.SetActive(false);
        }

        private void OnDeleteButtonClicked()
        {
            _editDeleteProjectPlane.SetActive(false);
            _confirmDeletePlane.SetActive(true);
        }

        private void OnBackButtonClicked()
        {
            gameObject.SetActive(false);
            // You might want to raise an event or call a method to inform the parent system
        }

        private void OnEditDeleteButtonClicked()
        {
            _editDeleteProjectPlane.SetActive(!_editDeleteProjectPlane.activeSelf);
            _confirmDeletePlane.SetActive(false);
        }

        public void ConfirmDelete()
        {
            _dataManager.RemoveProject(Project);
            _confirmDeletePlane.SetActive(false);
            gameObject.SetActive(false);
            _mainScreenController.LoadTodayData(); // Refresh main screen data
            _mainScreenController.gameObject.SetActive(true);
        }

        public void CancelDelete()
        {
            _confirmDeletePlane.SetActive(false);
        }

        private void OnDisable()
        {
            foreach (var task in _tasks)
            {
                if (task != null)
                {
                    task.OnTaskCompleted -= HandleTaskCompleted;
                }
            }

            if (_projectsScreen != null)
            {
                _projectsScreen.OnProjectEdited -= HandleProjectEdited;
            }
        }
    }
}