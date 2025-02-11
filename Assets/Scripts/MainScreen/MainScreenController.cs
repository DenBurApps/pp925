using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using AddData;
using CalendarScreen;
using OpenProject;
using TMPro;

public class MainScreenController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Filter")] [SerializeField] private Button _filterButton;
    [SerializeField] private GameObject _filterPanel;
    [SerializeField] private List<FilterButton> _filterToggles;
    [SerializeField] private TMP_Text _filterText;

    [Header("Projects")] [SerializeField] private GameObject _projectPlane;
    [SerializeField] private Transform _projectsContainer;
    [SerializeField] private ProjectUI _projectPrefab;

    [Header("Lessons")] [SerializeField] private GameObject _lessonsPlane;
    [SerializeField] private Transform _lessonsContainer;
    [SerializeField] private LessonUI _lessonPrefab;
    [SerializeField] private Button _lessonsWrapButton;
    [SerializeField] private GameObject _lessonsScrollView;

    [Header("HomeTasks")] [SerializeField] private GameObject _hometaskPlane;
    [SerializeField] private Transform _homeTasksContainer;
    [SerializeField] private HomeTaskUI _homeTaskPrefab;
    [SerializeField] private Button _homeTasksWrapButton;
    [SerializeField] private GameObject _homeTasksScrollView;

    [Header("Other UI")] [SerializeField] private GameObject _emptyStateObject;
    [SerializeField] private Button _addTaskButton;
    [SerializeField] private CreateTaskScreenController _addTaskScreen;

    [Header("Data")] [SerializeField] private DataManager _dataManager;

    [SerializeField] private CalendarScreenController _calendarScreenController;

    [SerializeField] private Button _calendarButton;
    [SerializeField] private Button _settingsButton;

    [Header("Edit Screens")] [SerializeField]
    private LessonsScreen _editLessonsScreen;

    [SerializeField] private ProjectsScreen _editProjectScreen;
    [SerializeField] private HomeTaskScreen _editHomeTaskScreen;

    [SerializeField] private OpenProjectScreen _openProjectScreen;

    #endregion

    #region Private Fields

    private bool _hasProjects;
    private bool _hasLessons;
    private bool _hasHomeTasks;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        LoadTodayData();
        ToggleFilterPanel(false);
        _addTaskScreen.gameObject.SetActive(false);

        if (_editLessonsScreen != null) _editLessonsScreen.gameObject.SetActive(false);
        if (_editProjectScreen != null) _editProjectScreen.gameObject.SetActive(false);
        if (_editHomeTaskScreen != null) _editHomeTaskScreen.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        LoadTodayData();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        if (_openProjectScreen != null)
        {
            _openProjectScreen.OnProjectUpdated -= HandleProjectUpdated;
        }
    }

    #endregion

    #region Event Subscription

    private void SubscribeToEvents()
    {
        _filterButton.onClick.AddListener((() => ToggleFilterPanel()));
        _lessonsWrapButton.onClick.AddListener(() => ToggleScrollView(_lessonsScrollView));
        _homeTasksWrapButton.onClick.AddListener(() => ToggleScrollView(_homeTasksScrollView));
        _addTaskButton.onClick.AddListener(() => _addTaskScreen.gameObject.SetActive(true));
        _calendarButton.onClick.AddListener(() => _calendarScreenController.gameObject.SetActive(true));

        if (_addTaskScreen != null)
        {
            _addTaskScreen.OnProjectCreated += HandleProjectCreated;
            _addTaskScreen.OnLessonCreated += HandleLessonCreated;
            _addTaskScreen.OnHomeTaskCreated += HandleHomeTaskCreated;
        }

        if (_editLessonsScreen != null)
        {
            _editLessonsScreen.OnLessonEdited += HandleLessonEdited;
        }

        if (_editProjectScreen != null)
        {
            _editProjectScreen.OnProjectEdited += HandleProjectEdited;
        }

        if (_editHomeTaskScreen != null)
        {
            _editHomeTaskScreen.OnHomeTaskEdited += HandleHomeTaskEdited;
        }

        foreach (var toggle in _filterToggles)
        {
            toggle.FilterClicked += ApplyFilter;
        }
    }

    private void UnsubscribeFromEvents()
    {
        _filterButton.onClick.RemoveListener((() => ToggleFilterPanel()));
        _lessonsWrapButton.onClick.RemoveListener(() => ToggleScrollView(_lessonsScrollView));
        _homeTasksWrapButton.onClick.RemoveListener(() => ToggleScrollView(_homeTasksScrollView));
        _addTaskButton.onClick.RemoveListener(() => _addTaskScreen.gameObject.SetActive(true));
        _calendarButton.onClick.RemoveListener(() => _calendarScreenController.gameObject.SetActive(true));


        if (_addTaskScreen != null)
        {
            _addTaskScreen.OnProjectCreated -= HandleProjectCreated;
            _addTaskScreen.OnLessonCreated -= HandleLessonCreated;
            _addTaskScreen.OnHomeTaskCreated -= HandleHomeTaskCreated;
        }

        if (_editLessonsScreen != null)
        {
            _editLessonsScreen.OnLessonEdited -= HandleLessonEdited;
        }

        if (_editProjectScreen != null)
        {
            _editProjectScreen.OnProjectEdited -= HandleProjectEdited;
        }

        if (_editHomeTaskScreen != null)
        {
            _editHomeTaskScreen.OnHomeTaskEdited -= HandleHomeTaskEdited;
        }

        foreach (var toggle in _filterToggles)
        {
            toggle.FilterClicked -= ApplyFilter;
        }
        
        foreach (Transform child in _lessonsContainer)
        {
            LessonUI lessonUI = child.GetComponent<LessonUI>();
            if (lessonUI != null)
            {
                lessonUI.LessonEdit -= OpenLessonEditScreen;
                lessonUI.LessonDelete -= HandleLessonDeleted;
            }
        }

        foreach (Transform child in _homeTasksContainer)
        {
            HomeTaskUI taskUI = child.GetComponent<HomeTaskUI>();
            if (taskUI != null)
            {
                taskUI.HomeTaskEdit -= OpenHomeTaskEditScreen;
                taskUI.HomeTaskDelete -= HandleHomeTaskDeleted;
            }
        }
    }

    #endregion

    #region Task Creation Handlers

    public void HandleProjectCreated(Project project)
    {
        ProjectUI projectUI = Instantiate(_projectPrefab, _projectsContainer);
        projectUI.Initialize(project);

        _projectPlane.gameObject.SetActive(true);
        _hasProjects = true;
        UpdateEmptyState();
    }

    public void HandleLessonCreated(Lesson lesson)
    {
        LessonUI lessonUI = Instantiate(_lessonPrefab, _lessonsContainer);
        lessonUI.Initialize(lesson,_dataManager);
        lessonUI.LessonEdit += OpenLessonEditScreen;

        _lessonsPlane.SetActive(true);
        _hasLessons = true;
        UpdateEmptyState();
    }

    public void HandleHomeTaskCreated(HomeTask homeTask)
    {
        HomeTaskUI homeTaskUI = Instantiate(_homeTaskPrefab, _homeTasksContainer);
        homeTaskUI.Initialize(homeTask, _dataManager);
        homeTaskUI.HomeTaskEdit += OpenHomeTaskEditScreen;

        _hometaskPlane.SetActive(true);
        _hasHomeTasks = true;
        UpdateEmptyState();
    }
    
    private void HandleLessonDeleted(Lesson lesson)
    {
        _dataManager.RemoveLesson(lesson);
        foreach (Transform child in _lessonsContainer)
        {
            LessonUI lessonUI = child.GetComponent<LessonUI>();
            if (lessonUI != null && lessonUI.Lesson == lesson)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    
        _hasLessons = _lessonsContainer.childCount > 0;
        UpdateEmptyState();
    }

    private void HandleHomeTaskDeleted(HomeTask homeTask)
    {
        _dataManager.RemoveHomeTask(homeTask);
        foreach (Transform child in _homeTasksContainer)
        {
            HomeTaskUI taskUI = child.GetComponent<HomeTaskUI>();
            if (taskUI != null && taskUI.HomeTask == homeTask)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    
        _hasHomeTasks = _homeTasksContainer.childCount > 0;
        UpdateEmptyState();
    }

    #endregion

    #region Data Loading

    public void LoadTodayData()
    {
        foreach (Transform child in _projectsContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in _lessonsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _homeTasksContainer)
        {
            Destroy(child.gameObject);
        }
        
        var todayData = _dataManager.GetTodayData();
        
        // Load Projects
        _hasProjects = todayData.projects.Any();
        if (_hasProjects)
        {
            foreach (var project in todayData.projects)
            {
                CreateProjectUI(project);
            }
        }

        // Load Lessons
        _hasLessons = todayData.lessons.Any();
        if (_hasLessons)
        {
            foreach (var lesson in todayData.lessons)
            {
                CreateLessonUI(lesson);
            }
        }

        // Load HomeTasks
        _hasHomeTasks = todayData.homeTasks.Any();
        if (_hasHomeTasks)
        {
            foreach (var homeTask in todayData.homeTasks)
            {
                CreateHomeTaskUI(homeTask);
            }
        }

        UpdateUIVisibility();
    }

    private void CreateProjectUI(Project project)
    {
        ProjectUI projectUI = Instantiate(_projectPrefab, _projectsContainer);
        projectUI.Initialize(project);
        projectUI.ProjectEdit += OpenProjectEditScreen;
        projectUI.ProjectSelected += HandleProjectSelected;
    }

    private void HandleProjectSelected(Project project)
    {
        if (_openProjectScreen != null)
        {
            _openProjectScreen.gameObject.SetActive(true);
            _openProjectScreen.OpenScreen(project);
            _openProjectScreen.OnProjectUpdated += HandleProjectUpdated; // Subscribe to project updates
        }
    }
    
    private void HandleProjectUpdated(Project project)
    {
        // Find and update the corresponding ProjectUI
        foreach (Transform child in _projectsContainer)
        {
            ProjectUI projectUI = child.GetComponent<ProjectUI>();
            if (projectUI != null && projectUI.Project == project)
            {
                projectUI.UpdateUI();
                break;
            }
        }
    }

    private void CreateLessonUI(Lesson lesson)
    {
        LessonUI lessonUI = Instantiate(_lessonPrefab, _lessonsContainer);
        lessonUI.Initialize(lesson, _dataManager);
        lessonUI.LessonEdit += OpenLessonEditScreen;
        lessonUI.LessonDelete += HandleLessonDeleted; // Add this line
    }

    private void CreateHomeTaskUI(HomeTask homeTask)
    {
        HomeTaskUI homeTaskUI = Instantiate(_homeTaskPrefab, _homeTasksContainer);
        homeTaskUI.Initialize(homeTask, _dataManager);
        homeTaskUI.HomeTaskEdit += OpenHomeTaskEditScreen;
        homeTaskUI.HomeTaskDelete += HandleHomeTaskDeleted; // Add this line
    }

    #endregion

    private void OpenLessonEditScreen(Lesson lesson)
    {
        if (_editLessonsScreen != null)
        {
            _editLessonsScreen.gameObject.SetActive(true);
            _editLessonsScreen.SetLessonForEdit(lesson);
        }
    }

    private void OpenProjectEditScreen(Project project)
    {
        if (_editProjectScreen != null)
        {
            _editProjectScreen.gameObject.SetActive(true);
            _editProjectScreen.SetProjectForEdit(project);
        }
    }

    private void OpenHomeTaskEditScreen(HomeTask homeTask)
    {
        if (_editHomeTaskScreen != null)
        {
            _editHomeTaskScreen.gameObject.SetActive(true);
            _editHomeTaskScreen.SetHomeTaskForEdit(homeTask);
        }
    }

    private void HandleProjectEdited(Project oldProject, Project newProject)
    {
        foreach (Transform child in _projectsContainer)
        {
            ProjectUI projectUI = child.GetComponent<ProjectUI>();
            if (projectUI != null && projectUI.Project == oldProject)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        CreateProjectUI(newProject);
        _hasProjects = _projectsContainer.childCount > 0;
        UpdateEmptyState();

        if (_editProjectScreen != null)
        {
            _editProjectScreen.gameObject.SetActive(false);
        }
    }

    private void HandleLessonEdited(Lesson oldLesson, Lesson newLesson)
    {
        foreach (Transform child in _lessonsContainer)
        {
            LessonUI lessonUI = child.GetComponent<LessonUI>();
            if (lessonUI != null && lessonUI.Lesson == oldLesson)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        CreateLessonUI(newLesson);
        _hasLessons = _lessonsContainer.childCount > 0;
        UpdateEmptyState();

        if (_editLessonsScreen != null)
        {
            _editLessonsScreen.gameObject.SetActive(false);
        }
    }

    private void HandleHomeTaskEdited(HomeTask oldTask, HomeTask newTask)
    {
        foreach (Transform child in _homeTasksContainer)
        {
            HomeTaskUI taskUI = child.GetComponent<HomeTaskUI>();
            if (taskUI != null && taskUI.HomeTask == oldTask)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        CreateHomeTaskUI(newTask);
        _hasHomeTasks = _homeTasksContainer.childCount > 0;
        UpdateEmptyState();

        if (_editHomeTaskScreen != null)
        {
            _editHomeTaskScreen.gameObject.SetActive(false);
        }
    }

    #region UI State Management

    private void UpdateUIVisibility()
    {
        bool hasAnyData = _hasProjects || _hasLessons || _hasHomeTasks;
        _emptyStateObject.SetActive(!hasAnyData);
        _hometaskPlane.SetActive(_hasHomeTasks);
        _lessonsPlane.SetActive(_hasLessons);
        _projectPlane.SetActive(_hasProjects);
    }

    private void UpdateEmptyState()
    {
        bool hasAnyData = _hasProjects || _hasLessons || _hasHomeTasks;
        _emptyStateObject.SetActive(!hasAnyData);
    }

    private void ToggleFilterPanel(bool? forcedState = null)
    {
        bool newState = forcedState ?? !_filterPanel.activeSelf;
        Debug.Log($"Toggle Filter Panel - Current state: {_filterPanel.activeSelf}, New state: {newState}");
        _filterPanel.SetActive(newState);
    }
    private void ToggleScrollView(GameObject scrollView)
    {
        scrollView.SetActive(!scrollView.activeSelf);
    }

    #endregion

    #region Filtering

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
                ShowAllSections();
                break;
            case "OnlyProjects":
                ShowOnlyProjects();
                break;
            case "OnlyLessons":
                ShowOnlyLessons();
                break;
            case "OnlyHomeTasks":
                ShowOnlyHomeTasks();
                break;
        }
    }

    private void ShowAllSections()
    {
        _projectsContainer.gameObject.SetActive(true);
        _lessonsScrollView.SetActive(true);
        _homeTasksScrollView.SetActive(true);
    }

    private void ShowOnlyProjects()
    {
        _projectsContainer.gameObject.SetActive(true);
        _lessonsScrollView.SetActive(false);
        _homeTasksScrollView.SetActive(false);
    }

    private void ShowOnlyLessons()
    {
        _projectsContainer.gameObject.SetActive(false);
        _lessonsScrollView.SetActive(true);
        _homeTasksScrollView.SetActive(false);
    }

    private void ShowOnlyHomeTasks()
    {
        _projectsContainer.gameObject.SetActive(false);
        _lessonsScrollView.SetActive(false);
        _homeTasksScrollView.SetActive(true);
    }

    #endregion
}