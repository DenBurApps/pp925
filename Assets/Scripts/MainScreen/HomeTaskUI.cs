using System;
using System.Collections;
using System.Collections.Generic;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTaskUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button _toggleButton;
    [SerializeField] private GameObject _toggleObject;
    [SerializeField] private DataManager _dataManager;
    
    
    public event Action<HomeTask> HomeTaskEdit;
    public event Action<HomeTask> HomeTaskDelete;
    
    public HomeTask HomeTask { get; private set; }
    
    public void Initialize(HomeTask homeTask, DataManager dataManager)
    {
        HomeTask = homeTask;
        
        nameText.text = homeTask.Name;
        subjectText.text = homeTask.SubjectName;
        timeText.text = homeTask.DateTime.ToString("HH:mm");
        
        editButton.onClick.AddListener(() => EditHomeTask(homeTask));
        deleteButton.onClick.AddListener(() => DeleteHomeTask(homeTask));
        _toggleButton.onClick.AddListener(ToggleObject);

        _dataManager = dataManager;
        
        if (_toggleObject != null)
        {
            _toggleObject.SetActive(homeTask.IsExpanded);
        }
    }
    
    private void EditHomeTask(HomeTask homeTask)
    {
        HomeTaskEdit?.Invoke(homeTask);
    }
    
    private void DeleteHomeTask(HomeTask homeTask)
    {
        HomeTaskDelete?.Invoke(homeTask);
    }
    
    private void OnDestroy()
    {
        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        HomeTaskEdit = null;
        HomeTaskDelete = null;
        _toggleButton.onClick.RemoveAllListeners();
    }
    
    private void ToggleObject()
    {
        if (_toggleObject != null)
        {
            HomeTask.IsExpanded = !_toggleObject.activeSelf;
            _toggleObject.SetActive(HomeTask.IsExpanded);
            
            // Обновляем состояние в DataManager
            if (_dataManager != null)
            {
                _dataManager.EditHomeTask(HomeTask, HomeTask);
            }
        }
    }
}
