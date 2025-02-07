using System.Collections;
using System.Collections.Generic;
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
    
    public void Initialize(HomeTask homeTask)
    {
        nameText.text = homeTask.name;
        subjectText.text = homeTask.subjectName;
        timeText.text = homeTask.time.ToString("HH:mm");
        
        editButton.onClick.AddListener(() => EditHomeTask(homeTask));
        deleteButton.onClick.AddListener(() => DeleteHomeTask(homeTask));
    }
    
    private void EditHomeTask(HomeTask homeTask)
    {
        // Implement edit functionality
    }
    
    private void DeleteHomeTask(HomeTask homeTask)
    {
        // Implement delete functionality
    }
}
