using System;
using System.Collections;
using System.Collections.Generic;
using AddTask;
using Bitsplash.DatePicker;
using DanielLochner.Assets.SimpleScrollSnap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateTaskScreenController : MonoBehaviour
{
    public enum ScreenType
    {
        Lessons,
        HomeTasks,
        Projects
    }
    
    [Header("Navigation")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _lessonsButton;
    [SerializeField] private Button _homeTasksButton;
    [SerializeField] private Button _projectsButton;
    
    [Header("Screen GameObjects")]
    [SerializeField] private GameObject _homeTaskScreen;
    [SerializeField] private GameObject _projectScreen;
    [SerializeField] private GameObject _lessonScreen;
    
    [Header("HomeTask Fields")]
    [SerializeField] private TMP_InputField _homeTaskNameInput;
    [SerializeField] private TMP_InputField _subjectInput;
    [SerializeField] private Button _dateButton;
    [SerializeField] private TMP_Text _dateText;
    [SerializeField] private Button _timeButton;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Button _priorityButton;
    [SerializeField] private GameObject _priorityMenu;
    [SerializeField] private Button _saveHomeTaskButton;
    
    [Header("Project Fields")]
    [SerializeField] private TMP_InputField _projectNameInput;
    [SerializeField] private Button _projectDateButton;
    [SerializeField] private TMP_Text _projectDateText;
    [SerializeField] private Button _addTaskButton;
    [SerializeField] private Transform _tasksContainer;
    [SerializeField] private GameObject _taskItemPrefab;
    [SerializeField] private Button _saveProjectButton;
    
    [Header("Button Colors")]
    [SerializeField] private Color _activeButtonColor;
    [SerializeField] private Color _inactiveButtonColor;
    
    [SerializeField] private DatePickerSettings _datePicker;
    [SerializeField] private TimeSelector _timeSelector;
    private string _selectedPriority = "";
    
    private void Start()
    {
        InitializeUI();
        SetupListeners();
    }
    
    private void InitializeUI()
    {
        _saveHomeTaskButton.interactable = false;
        _saveProjectButton.interactable = false;

        _dateText.text = DateTime.Now.ToString("MMM dd, yyyy");
        _projectDateText.text = DateTime.Now.ToString("MMM dd, yyyy");
    }

    private void SetupListeners()
    {
        _backButton.onClick.AddListener(OnBackButtonClick);
        _lessonsButton.onClick.AddListener(() => ShowScreen(ScreenType.Lessons));
        _homeTasksButton.onClick.AddListener(() => ShowScreen(ScreenType.HomeTasks));
        _projectsButton.onClick.AddListener(() => ShowScreen(ScreenType.Projects));
        
        _dateButton.onClick.AddListener(() => ShowDatePicker(_dateText));
        _timeButton.onClick.AddListener(ShowTimePicker);
        _priorityButton.onClick.AddListener(TogglePriorityMenu);
        
        _projectDateButton.onClick.AddListener(() => ShowDatePicker(_projectDateText));
        _addTaskButton.onClick.AddListener(ShowAddTaskDialog);
        
        _homeTaskNameInput.onValueChanged.AddListener(_ => ValidateHomeTaskInputs());
        _subjectInput.onValueChanged.AddListener(_ => ValidateHomeTaskInputs());
        _projectNameInput.onValueChanged.AddListener(_ => ValidateProjectInputs());
    }

    private void ShowScreen(ScreenType screenType)
    {
        _homeTaskScreen.SetActive(screenType == ScreenType.HomeTasks);
        _projectScreen.SetActive(screenType == ScreenType.Projects);
        _lessonScreen.SetActive(screenType == ScreenType.Lessons);
        
        _homeTasksButton.GetComponent<Image>().color = screenType == ScreenType.HomeTasks ? _activeButtonColor : _inactiveButtonColor;
        _projectsButton.GetComponent<Image>().color = screenType == ScreenType.Projects ? _activeButtonColor : _inactiveButtonColor;
        _lessonsButton.GetComponent<Image>().color = screenType == ScreenType.Lessons ? _activeButtonColor : _inactiveButtonColor;
    }

    private void ShowDatePicker(TMP_Text targetText)
    {
        _datePicker.Show((date) =>
        {
            targetText.text = date.ToString("MMM dd, yyyy");
            ValidateAllInputs();
        });
    }
    
    private void ShowTimePicker()
    {
        _timeSelector.gameObject.SetActive(true);
        _timeSelector.OnSelectionChanged.AddListener((index) =>
        {
            _timeText.text = _timeSelector.GetCurrentPanel().GetComponent<TimeOption>().TimeValue;
            ValidateAllInputs();
        });
    }
    
    private void TogglePriorityMenu()
    {
        _priorityMenu.SetActive(!_priorityMenu.activeSelf);
    }
    
    public void SetPriority(string priority)
    {
        _selectedPriority = priority;
        _priorityButton.GetComponentInChildren<TMP_Text>().text = priority;
        _priorityMenu.SetActive(false);
        ValidateAllInputs();
    }
    
    private void ValidateHomeTaskInputs()
    {
        bool isValid = !string.IsNullOrEmpty(_homeTaskNameInput.text) &&
                      !string.IsNullOrEmpty(_subjectInput.text) &&
                      !string.IsNullOrEmpty(_timeText.text) &&
                      !string.IsNullOrEmpty(_selectedPriority);
        
        _saveHomeTaskButton.interactable = isValid;
    }
    
    private void ValidateProjectInputs()
    {
        bool isValid = !string.IsNullOrEmpty(_projectNameInput.text) &&
                      _tasksContainer.childCount > 0;
        
        _saveProjectButton.interactable = isValid;
    }
    
    private void ValidateAllInputs()
    {
        ValidateHomeTaskInputs();
        ValidateProjectInputs();
    }
    
    private void ShowAddTaskDialog()
    {
        GameObject taskItem = Instantiate(_taskItemPrefab, _tasksContainer);
        TaskItemUI taskItemUI = taskItem.GetComponent<TaskItemUI>();
        taskItemUI.Initialize(() => ValidateProjectInputs());
    }
    
    private void OnBackButtonClick()
    {
        gameObject.SetActive(false);
    }
}
