using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject _settingsCanvas;
    [SerializeField] private GameObject _privacyCanvas;
    [SerializeField] private GameObject _termsCanvas;
    [SerializeField] private GameObject _contactCanvas;
    [SerializeField] private GameObject _versionCanvas;
    [SerializeField] private TMP_Text _versionText;
    [SerializeField] private MainScreenController _mainScreenController;
    [SerializeField] private CalendarScreen.CalendarScreenController _calendarScreenController;
    [SerializeField] private Button _calendarButton;
    [SerializeField] private Button _mainScreenButton;
    private string _version = "Application version:\n";

    private void Awake()
    {
        _settingsCanvas.SetActive(false);
        _privacyCanvas.SetActive(false);
        _termsCanvas.SetActive(false);
        _contactCanvas.SetActive(false);
        _versionCanvas.SetActive(false);
        SetVersion();
    }

    private void OnEnable()
    {
        _mainScreenButton.onClick.AddListener(OpenMainScreen);
        _calendarButton.onClick.AddListener(OpenCalendar);
    }

    private void OnDisable()
    {
        _mainScreenButton.onClick.RemoveListener(OpenMainScreen);
        _calendarButton.onClick.RemoveListener(OpenCalendar);
    }

    private void SetVersion()
    {
        _versionText.text = _version + Application.version;
    }

    public void ShowSettings()
    {
        _settingsCanvas.SetActive(true);
    }

    private void OpenMainScreen()
    {
        _mainScreenController.EnableScreen();
        _settingsCanvas.SetActive(false);
    }

    private void OpenCalendar()
    {
        _calendarScreenController.EnableScreen();
        _settingsCanvas.SetActive(false);
    }

    public void RateUs()
    {
#if UNITY_IOS
        Device.RequestStoreReview();
#endif
    }
}
