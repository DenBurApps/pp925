using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskItemUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Button _detailButton;
    [SerializeField] private Button _deleteButton;
    
    private Action _onValidationRequired;
    
    public void Initialize(Action validationCallback)
    {
        _onValidationRequired = validationCallback;
        
        _nameInput.onValueChanged.AddListener(_ => _onValidationRequired?.Invoke());
        _deleteButton.onClick.AddListener(() => 
        {
            Destroy(gameObject);
            _onValidationRequired?.Invoke();
        });
    }
}
