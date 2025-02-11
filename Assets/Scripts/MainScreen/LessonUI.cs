using System;
using System.Collections;
using System.Collections.Generic;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LessonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button _toggleButton;
    [SerializeField] private GameObject _toggleObject;
    [SerializeField] private DataManager _dataManager;

    public event Action<Lesson> LessonEdit;
    public event Action<Lesson> LessonDelete;
    
    public Lesson Lesson { get; private set; }
    
    public void Initialize(Lesson lesson, DataManager dataManager)
    {
        Lesson = lesson;
        
        nameText.text = lesson.Name;
        durationText.text = $"{lesson.DateTime.Hour:D2}:{lesson.DateTime.Minute:D2}";
        
        editButton.onClick.AddListener(() => EditLesson(lesson));
        deleteButton.onClick.AddListener(() => DeleteLesson(lesson));
        _toggleButton.onClick.AddListener(ToggleObject);
        _dataManager = dataManager;
        
        if (_toggleObject != null)
        {
            _toggleObject.SetActive(lesson.IsExpanded);
        }
    }
    
    private void EditLesson(Lesson lesson)
    {
        LessonEdit?.Invoke(lesson);
    }
    
    private void DeleteLesson(Lesson lesson)
    {
        LessonDelete?.Invoke(lesson);
    }
    
    private void ToggleObject()
    {
        if (_toggleObject != null)
        {
            Lesson.IsExpanded = !_toggleObject.activeSelf;
            _toggleObject.SetActive(Lesson.IsExpanded);
            
            // Обновляем состояние в DataManager
            if (_dataManager != null)
            {
                _dataManager.EditLesson(Lesson, Lesson);
            }
        }
    }
    
    private void OnDestroy()
    {
        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        _toggleButton.onClick.RemoveAllListeners();
        LessonEdit = null;
        LessonDelete = null;
    }
}