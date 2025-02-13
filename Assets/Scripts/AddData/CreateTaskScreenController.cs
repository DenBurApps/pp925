using System;
using System.Collections.Generic;
using AddTask;
using Bitsplash.DatePicker;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    public class CreateTaskScreenController : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _lessonsButton;
        [SerializeField] private Button _homeTasksButton;
        [SerializeField] private Button _projectsButton;

        [Header("Screen GameObjects")]
        [SerializeField] private HomeTaskScreen _homeTaskScreen;
        [SerializeField] private ProjectsScreen _projectScreen;
        [SerializeField] private LessonsScreen _lessonScreen;
        [SerializeField] private MainScreenController _mainScreenController;

        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup _homeTaskCanvasGroup;
        [SerializeField] private CanvasGroup _projectCanvasGroup;
        [SerializeField] private CanvasGroup _lessonCanvasGroup;

        [Header("Button Colors")]
        [SerializeField] private Color _activeButtonColor;
        [SerializeField] private Color _inactiveButtonColor;

        private Dictionary<Button, CanvasGroup> _screenMapping;
        private Button _currentActiveButton;

        public event Action<HomeTask> OnHomeTaskCreated;
        public event Action<Project> OnProjectCreated;
        public event Action<Lesson> OnLessonCreated;
        public event Action OnBackPressed;

        #region Unity Lifecycle

        private void Awake()
        {
            if (!ValidateComponents())
            {
                Debug.LogError("Required components are not assigned in the inspector", this);
                enabled = false;
                return;
            }

            InitializeScreenMapping();
        }

        private void OnEnable()
        {
            _homeTasksButton.image.color = _inactiveButtonColor;
            _lessonsButton.image.color = _inactiveButtonColor;
            _projectsButton.image.color = _inactiveButtonColor;

            SubscribeToEvents();
            ShowHomeTaskScreen();
            _homeTasksButton.image.color = _activeButtonColor;
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization
        
        private bool ValidateComponents()
        {
            return _backButton != null && 
                   _lessonsButton != null && 
                   _homeTasksButton != null &&
                   _projectsButton != null && 
                   _homeTaskScreen != null && 
                   _projectScreen != null &&
                   _lessonScreen != null &&
                   _homeTaskCanvasGroup != null &&
                   _projectCanvasGroup != null &&
                   _lessonCanvasGroup != null;
        }

        private void InitializeScreenMapping()
        {
            _screenMapping = new Dictionary<Button, CanvasGroup>
            {
                { _homeTasksButton, _homeTaskCanvasGroup },
                { _projectsButton, _projectCanvasGroup },
                { _lessonsButton, _lessonCanvasGroup }
            };

            // Initially disable all screens
            foreach (var screen in _screenMapping.Values)
            {
                SetCanvasGroupState(screen, false);
            }
        }

        private void SubscribeToEvents()
        {
            _backButton.onClick.AddListener(OnBackButtonClick);
            _lessonsButton.onClick.AddListener(ShowLessonsScreen);
            _homeTasksButton.onClick.AddListener(ShowHomeTaskScreen);
            _projectsButton.onClick.AddListener(ShowProjectsScreen);

            // Subscribe to screen events
            _homeTaskScreen.OnHomeTaskCreated += HandleHomeTaskCreated;
            _projectScreen.OnProjectCreated += HandleProjectCreated;
            _lessonScreen.OnLessonCreated += HandleLessonCreated;
        }

        private void UnsubscribeFromEvents()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClick);
            _lessonsButton.onClick.RemoveListener(ShowLessonsScreen);
            _homeTasksButton.onClick.RemoveListener(ShowHomeTaskScreen);
            _projectsButton.onClick.RemoveListener(ShowProjectsScreen);

            // Unsubscribe from screen events
            _homeTaskScreen.OnHomeTaskCreated -= HandleHomeTaskCreated;
            _projectScreen.OnProjectCreated -= HandleProjectCreated;
            _lessonScreen.OnLessonCreated -= HandleLessonCreated;
        }

        #endregion

        #region Navigation

        private void OnBackButtonClick()
        {
            OnBackPressed?.Invoke();
            Hide();
        }

        private void ShowHomeTaskScreen()
        {
            SwitchScreen(_homeTasksButton);
        }

        private void ShowProjectsScreen()
        {
            SwitchScreen(_projectsButton);
        }

        private void ShowLessonsScreen()
        {
            SwitchScreen(_lessonsButton);
        }

        private void SwitchScreen(Button selectedButton)
        {
            if (_currentActiveButton == selectedButton) return;

            // Deactivate current screen and button
            if (_currentActiveButton != null)
            {
                _currentActiveButton.image.color = _inactiveButtonColor;
                SetCanvasGroupState(_screenMapping[_currentActiveButton], false);
            }

            // Activate new screen and button
            selectedButton.GetComponent<Image>().color = _activeButtonColor;
            SetCanvasGroupState(_screenMapping[selectedButton], true);

            _currentActiveButton = selectedButton;
        }

        private void SetCanvasGroupState(CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.gameObject.SetActive(state);
        }

        #endregion

        #region Event Handlers

        private void HandleHomeTaskCreated(HomeTask homeTask)
        {
            OnHomeTaskCreated?.Invoke(homeTask);
            _mainScreenController.gameObject.SetActive(true);
            Hide();
        }

        private void HandleProjectCreated(Project project)
        {
            OnProjectCreated?.Invoke(project);
            _mainScreenController.gameObject.SetActive(true);
            _projectScreen.PrepareForNewProject();
            Hide();
        }

        private void HandleLessonCreated(Lesson lesson)
        {
            OnLessonCreated?.Invoke(lesson);
            _mainScreenController.gameObject.SetActive(true);
            Hide();
        }

        #endregion

        #region Public Methods

        public void Show()
        {
            gameObject.SetActive(true);
            ShowHomeTaskScreen();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}