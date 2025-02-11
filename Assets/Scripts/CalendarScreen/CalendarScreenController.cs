using System;
using System.Collections.Generic;
using System.Linq;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CalendarScreen
{
    public class CalendarScreenController : MonoBehaviour
    {
        [Header("Filter")] [SerializeField] private Button _filterButton;
        [SerializeField] private GameObject _filterPanel;
        [SerializeField] private List<FilterButton> _filterToggles;
        [SerializeField] private TMP_Text _filterText;

        [Header("Containers")] [SerializeField]
        private Transform _projectsContainer;

        [SerializeField] private Transform _lessonsContainer;
        [SerializeField] private Transform _homeTasksContainer;

        [SerializeField] private GameObject _lessonsScrollView;
        [SerializeField] private GameObject _homeTasksScrollView;

        [SerializeField] private Button _homeTasksWrapButton;
        [SerializeField] private Button _lessonsWrapButton;

        [Header("UI Elements")] [SerializeField]
        private List<ProjectUI> _projectUis;

        [SerializeField] private List<LessonUI> _lessonUis;
        [SerializeField] private List<HomeTaskUI> _homeTaskUis;
        [SerializeField] private Calendar _calendar;
        [SerializeField] private Button _addDataButton;
        [SerializeField] private GameObject _noDataPlane;
        [SerializeField] private Button _mainScreenButton;
        [SerializeField] private Button _settingsButton;

        [Header("References")] [SerializeField]
        private DataManager _dataManager;

        [SerializeField] private MainScreenController _mainScreenController;
        [SerializeField] private CreateTaskScreenController _addTaskScreen;

        private bool _hasProjects;
        private bool _hasLessons;
        private bool _hasHomeTasks;
        private DateTime _currentSelectedDate;

        private void Awake()
        {
            SetupListeners();
            _currentSelectedDate = DateTime.Today;
        }

        private void OnDestroy()
        {
            CleanupListeners();
        }

        private void SetupListeners()
        {
            if (_calendar != null)
            {
                _calendar.DateSelected += OnDateSelected;
            }

            if (_filterButton != null)
            {
                _filterButton.onClick.AddListener(() => ToggleFilterPanel());
            }

            if (_dataManager != null)
            {
                _dataManager.DataChanged += OnDataChanged;
            }

            foreach (var filterButton in _filterToggles)
            {
                filterButton.FilterClicked += (button, filterName) => ApplyFilter(button, filterName);
            }

            if (_homeTasksWrapButton != null)
            {
                _homeTasksWrapButton.onClick.AddListener(() => ToggleWrapContent(_homeTasksScrollView));
            }

            if (_lessonsWrapButton != null)
            {
                _lessonsWrapButton.onClick.AddListener(() => ToggleWrapContent(_lessonsScrollView));
            }

            _addDataButton.onClick.AddListener(() => _addTaskScreen.gameObject.SetActive(true));
            _mainScreenButton.onClick.AddListener((() => _mainScreenController.gameObject.SetActive(true)));
        }

        private void CleanupListeners()
        {
            if (_calendar != null)
            {
                _calendar.DateSelected -= OnDateSelected;
            }

            if (_filterButton != null)
            {
                _filterButton.onClick.RemoveAllListeners();
            }

            if (_dataManager != null)
            {
                _dataManager.DataChanged -= OnDataChanged;
            }

            if (_homeTasksWrapButton != null)
            {
                _homeTasksWrapButton.onClick.RemoveAllListeners();
            }

            if (_lessonsWrapButton != null)
            {
                _lessonsWrapButton.onClick.RemoveAllListeners();
            }

            _mainScreenButton.onClick.RemoveListener((() => _mainScreenController.gameObject.SetActive(true)));
            _addDataButton.onClick.RemoveListener(() => _addTaskScreen.gameObject.SetActive(true));
        }

        private void Start()
        {
            LoadDateData(DateTime.Today);
        }

        private void OnDateSelected(DateTime selectedDate)
        {
            _currentSelectedDate = selectedDate;
            ClearCurrentData();
            LoadDateData(selectedDate);
        }

        private void OnDataChanged()
        {
            // Reload current date data when data changes
            LoadDateData(_currentSelectedDate);
        }

        private void ClearCurrentData()
        {
            // Deactivate all project UIs
            foreach (var projectUI in _projectUis)
            {
                if (projectUI != null && projectUI.gameObject != null)
                {
                    projectUI.gameObject.SetActive(false);
                }
            }

            // Deactivate all lesson UIs
            foreach (var lessonUI in _lessonUis)
            {
                if (lessonUI != null && lessonUI.gameObject != null)
                {
                    lessonUI.gameObject.SetActive(false);
                }
            }

            // Deactivate all homeTask UIs
            foreach (var homeTaskUI in _homeTaskUis)
            {
                if (homeTaskUI != null && homeTaskUI.gameObject != null)
                {
                    homeTaskUI.gameObject.SetActive(false);
                }
            }

            _hasProjects = false;
            _hasLessons = false;
            _hasHomeTasks = false;
        }

        private void LoadDateData(DateTime date)
        {
            if (_dataManager == null) return;

            Debug.Log(date);

            var dateData = _dataManager.GetDateData(date);

            // Handle Projects
            _hasProjects = dateData.projects.Any();
            for (int i = 0; i < _projectUis.Count; i++)
            {
                if (i < dateData.projects.Count)
                {
                    _projectUis[i].gameObject.SetActive(true);
                    _projectUis[i].Initialize(dateData.projects[i]);
                }
                else
                {
                    _projectUis[i].gameObject.SetActive(false);
                }
            }

            // Handle Lessons
            _hasLessons = dateData.lessons.Any();
            Debug.Log(_hasLessons);
            for (int i = 0; i < _lessonUis.Count; i++)
            {
                if (i < dateData.lessons.Count)
                {
                    Debug.Log(dateData.lessons.Count);
                    _lessonUis[i].gameObject.SetActive(true);
                    _lessonUis[i].Initialize(dateData.lessons[i], _dataManager);
                }
                else
                {
                    _lessonUis[i].gameObject.SetActive(false);
                }
            }

            // Handle HomeTasks
            _hasHomeTasks = dateData.homeTasks.Any();
            for (int i = 0; i < _homeTaskUis.Count; i++)
            {
                if (i < dateData.homeTasks.Count)
                {
                    _homeTaskUis[i].gameObject.SetActive(true);
                    _homeTaskUis[i].Initialize(dateData.homeTasks[i], _dataManager);
                }
                else
                {
                    _homeTaskUis[i].gameObject.SetActive(false);
                }
            }

            UpdateUIVisibility();
        }

        private void ToggleWrapContent(GameObject scrollView)
        {
            if (scrollView == null) return;
            scrollView.SetActive(!scrollView.activeSelf);
        }

        private void ToggleFilterPanel(bool? forcedState = null)
        {
            bool newState = forcedState ?? !_filterPanel.activeSelf;
            _filterPanel.SetActive(newState);
        }

        private void ApplyFilter(FilterButton button, string filterName)
        {
            foreach (var toggle in _filterToggles)
            {
                toggle.SetStatus(toggle == button);
            }

            ToggleFilterPanel(false);
            UpdateFilterText(filterName);
            UpdateVisibleSections(filterName);
        }

        private void UpdateFilterText(string filterName)
        {
            _filterText.text = filterName switch
            {
                "NoFilters" => "No filters",
                "OnlyProjects" => "Only projects",
                "OnlyLessons" => "Only lessons",
                "OnlyHomeTasks" => "Only home tasks",
                _ => _filterText.text
            };
        }

        private void UpdateVisibleSections(string filterName)
        {
            switch (filterName)
            {
                case "NoFilters":
                    _projectsContainer.gameObject.SetActive(_hasProjects);
                    _lessonsContainer.gameObject.SetActive(_hasLessons);
                    _homeTasksContainer.gameObject.SetActive(_hasHomeTasks);
                    break;
                case "OnlyProjects":
                    _projectsContainer.gameObject.SetActive(_hasProjects);
                    _lessonsContainer.gameObject.SetActive(false);
                    _homeTasksContainer.gameObject.SetActive(false);
                    break;
                case "OnlyLessons":
                    _projectsContainer.gameObject.SetActive(false);
                    _lessonsContainer.gameObject.SetActive(_hasLessons);
                    _homeTasksContainer.gameObject.SetActive(false);
                    break;
                case "OnlyHomeTasks":
                    _projectsContainer.gameObject.SetActive(false);
                    _lessonsContainer.gameObject.SetActive(false);
                    _homeTasksContainer.gameObject.SetActive(_hasHomeTasks);
                    break;
            }
        }

        private void ShowAllSections()
        {
            _projectsContainer.gameObject.SetActive(_hasProjects);
            _lessonsContainer.gameObject.SetActive(_hasLessons);
            _homeTasksContainer.gameObject.SetActive(_hasHomeTasks);
        }

        private void ShowOnlyProjects()
        {
            _projectsContainer.gameObject.SetActive(_hasProjects);
            _lessonsContainer.gameObject.SetActive(false);
            _homeTasksContainer.gameObject.SetActive(false);
        }

        private void ShowOnlyLessons()
        {
            _projectsContainer.gameObject.SetActive(false);
            _lessonsContainer.gameObject.SetActive(_hasLessons);
            _homeTasksContainer.gameObject.SetActive(false);
        }

        private void ShowOnlyHomeTasks()
        {
            _projectsContainer.gameObject.SetActive(false);
            _lessonsContainer.gameObject.SetActive(false);
            _homeTasksContainer.gameObject.SetActive(_hasHomeTasks);
        }

        private void UpdateUIVisibility()
        {
            bool hasAnyData = _hasProjects || _hasLessons || _hasHomeTasks;

            // First determine which sections should be visible based on current filter
            UpdateVisibleSections(_filterText.text);
            
            _projectsContainer.gameObject.SetActive(_hasProjects);
            _lessonsContainer.gameObject.SetActive(_hasLessons);
            _homeTasksContainer.gameObject.SetActive(_hasHomeTasks);
            _noDataPlane.SetActive(!hasAnyData);
        }
    }
}