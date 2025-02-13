using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AddData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text tasksText;
    [SerializeField] private Image progressFill;
    [SerializeField] private Button _editButton;

    public Project Project { get; private set; }
    public event Action<Project> ProjectEdit;

    private void Awake()
    {
        if (_editButton != null)
        {
            _editButton.onClick.AddListener(OnEditClicked);
        }
    }

    public void Initialize(Project project)
    {
        Project = project;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Project != null)
        {
            nameText.text = Project.Name;
            dateText.text = Project.Date.ToString("MMM dd, yyyy");

            int totalTasks = Project.TaskDatas.Count;
            int completedTasks = Project.TaskDatas.Count(task => task.IsCompleted);
            tasksText.text = $"{completedTasks}/{totalTasks} tasks";

            float fillAmount = totalTasks > 0 ? (float)completedTasks / totalTasks : 0f;
            progressFill.fillAmount = fillAmount;
        }
    }

    private void OnEditClicked()
    {
        ProjectEdit?.Invoke(Project);
    }
}