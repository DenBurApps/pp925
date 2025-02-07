using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text tasksText;
    [SerializeField] private Image progressFill;
    
    public void Initialize(Project project)
    {
        nameText.text = project.name;
        dateText.text = project.creationDate.ToString("MMM dd, yyyy");
        tasksText.text = $"{project.completedTasks}/{project.tasks.Count} tasks";
        progressFill.fillAmount = project.tasks.Count > 0 ? 
            (float)project.completedTasks / project.tasks.Count : 0f;
    }
}
