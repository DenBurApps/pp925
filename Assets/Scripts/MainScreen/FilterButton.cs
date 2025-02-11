using System;
using UnityEngine;
using UnityEngine.UI;

public class FilterButton : MonoBehaviour
{
    [SerializeField] private Button _selectButton;
    [SerializeField] private Image _checkImage;
    [SerializeField] private string _filterName;
    [SerializeField] private bool _isSelected;

    public event Action<FilterButton, string> FilterClicked;

    private void OnEnable()
    {
        _selectButton.onClick.AddListener(OnButtonClicked);
        ToggleCheckImage();
    }

    private void OnDisable()
    {
        _selectButton.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        FilterClicked?.Invoke(this,_filterName);
        ToggleCheckImage();
    }

    private void ToggleCheckImage()
    {
        _checkImage.gameObject.SetActive(_isSelected);
    }

    public void SetStatus(bool status)
    {
        _isSelected = status;
        ToggleCheckImage();
    }
}
