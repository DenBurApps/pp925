using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class MainScreenController : MonoBehaviour
{
    [Header("Filter")] [SerializeField] private Button _filterButton;
    [SerializeField] private GameObject _filterPanel;
    [SerializeField] private List<FilterButton> _filterToggles;
    [SerializeField] private TMP_Text _filterText;

    [Header("Projects")] [SerializeField] private GameObject _projectPlane;
    [SerializeField] private Transform _projectsContainer;
    [SerializeField] private GameObject _projectPrefab;

    [Header("Lessons")] [SerializeField] private GameObject _lessonsPlane;
    [SerializeField] private Transform _lessonsContainer;
    [SerializeField] private GameObject _lessonPrefab;
    [SerializeField] private Button _lessonsWrapButton;
    [SerializeField] private GameObject _lessonsScrollView;

    [Header("HomeTasks")] [SerializeField] private GameObject _hometaskPlane;
    [SerializeField] private Transform _homeTasksContainer;
    [SerializeField] private GameObject _homeTaskPrefab;
    [SerializeField] private Button _homeTasksWrapButton;
    [SerializeField] private GameObject _homeTasksScrollView;

    [Header("Empty State")] [SerializeField]
    private GameObject _emptyStateObject;

    [SerializeField] private DataManager _dataManager;

    [SerializeField] private GameObject createTaskScreen;

    [SerializeField] private Button _addTaskButton;

    private void Start()
    {
        LoadTodayData();
        ToggleFilterPanel();
    }

    private void OnEnable()
    {
        _filterButton.onClick.AddListener(ToggleFilterPanel);
        _lessonsWrapButton.onClick.AddListener(() => ToggleScrollView(_lessonsScrollView));
        _homeTasksWrapButton.onClick.AddListener(() => ToggleScrollView(_homeTasksScrollView));
        _addTaskButton.onClick.AddListener(() => createTaskScreen.SetActive(true));

        foreach (var toggle in _filterToggles)
        {
            toggle.FilterClicked += ApplyFilter;
        }
    }

    private void OnDisable()
    {
        _filterButton.onClick.RemoveListener(ToggleFilterPanel);
        _lessonsWrapButton.onClick.RemoveListener(() => ToggleScrollView(_lessonsScrollView));
        _homeTasksWrapButton.onClick.RemoveListener(() => ToggleScrollView(_homeTasksScrollView));
        _addTaskButton.onClick.RemoveListener(() => createTaskScreen.SetActive(true));
        
        foreach (var toggle in _filterToggles)
        {
            toggle.FilterClicked -= ApplyFilter;
        }
    }

    private void LoadTodayData()
    {
        var todayData = _dataManager.GetTodayData();
        bool hasData = false;

        // Load Projects
        if (todayData.projects.Any())
        {
            hasData = true;
            foreach (var project in todayData.projects)
            {
                CreateProjectUI(project);
            }
        }

        // Load Lessons
        if (todayData.lessons.Any())
        {
            hasData = true;
            foreach (var lesson in todayData.lessons)
            {
                CreateLessonUI(lesson);
            }
        }

        // Load HomeTasks
        if (todayData.homeTasks.Any())
        {
            hasData = true;
            foreach (var homeTask in todayData.homeTasks)
            {
                CreateHomeTaskUI(homeTask);
            }
        }

        _emptyStateObject.SetActive(!hasData);
        _hometaskPlane.SetActive(hasData);
        _lessonsPlane.SetActive(hasData);
        _projectPlane.SetActive(hasData);
    }

    private void CreateProjectUI(Project project)
    {
        var projectGO = Instantiate(_projectPrefab, _projectsContainer);
        var projectUI = projectGO.GetComponent<ProjectUI>();
        projectUI.Initialize(project);
    }

    private void CreateLessonUI(Lesson lesson)
    {
        var lessonGO = Instantiate(_lessonPrefab, _lessonsContainer);
        var lessonUI = lessonGO.GetComponent<LessonUI>();
        lessonUI.Initialize(lesson);
    }

    private void CreateHomeTaskUI(HomeTask homeTask)
    {
        var homeTaskGO = Instantiate(_homeTaskPrefab, _homeTasksContainer);
        var homeTaskUI = homeTaskGO.GetComponent<HomeTaskUI>();
        homeTaskUI.Initialize(homeTask);
    }

    private void ToggleFilterPanel()
    {
        _filterPanel.SetActive(!_filterPanel.activeSelf);
    }

    private void ToggleScrollView(GameObject scrollView)
    {
        scrollView.SetActive(!scrollView.activeSelf);
    }

    private void ApplyFilter(FilterButton button, string filterName)
    {
        foreach (var toggle in _filterToggles)
        {
            toggle.SetStatus(false);
        }

        button.SetStatus(true);
        ToggleFilterPanel();

        switch (filterName)
        {
            case "NoFilters":
                _filterText.text = "No filters";
                ShowAllSections();
                break;
            case "OnlyProjects":
                _filterText.text = "Only projects";
                ShowOnlyProjects();
                break;
            case "OnlyLessons":
                ShowOnlyLessons();
                _filterText.text = "Only lessons";
                break;
            case "OnlyHomeTasks":
                _filterText.text = "Only home tasks";
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
}

[Serializable]
public class Project
{
    public string name;
    public DateTime creationDate;
    public List<Task> tasks;
    public int completedTasks => tasks.Count(t => t.isCompleted);
}

[Serializable]
public class Task
{
    public string name;
    public bool isCompleted;
}

[Serializable]
public class Lesson
{
    public string name;
    public TimeSpan duration;
    public DateTime date;
}

[Serializable]
public class HomeTask
{
    public string name;
    public string subjectName;
    public DateTime time;
}