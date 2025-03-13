using UnityEngine;
using UnityEngine.UI;

public class ResolutionDropdown : MonoBehaviour
{
    public Dropdown resolutionDropdown;

    private Resolution[] resolutions;
    private bool isFullscreen = true;

    void Start()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("Dropdown не назначен!");
            return;
        }

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();

        foreach (var resolution in resolutions)
        {
            float aspectRatio = (float)resolution.width / resolution.height;
            if (Mathf.Approximately(aspectRatio, 16f / 9f))
            {
                string option = $"{resolution.width}x{resolution.height}";
                options.Add(option);
            }
        }

        resolutionDropdown.AddOptions(options);

        SetCurrentResolution();

        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
    }

    void SetCurrentResolution()
    {
        Resolution currentResolution = Screen.currentResolution;
        string currentOption = $"{currentResolution.width}x{currentResolution.height}";


        for (int i = 0; i < resolutions.Length; i++)
        {
            if ($"{resolutions[i].width}x{resolutions[i].height}" == currentOption)
            {
                resolutionDropdown.value = i;
                resolutionDropdown.RefreshShownValue();
                break;
            }
        }
    }

    void ChangeResolution(int index)
    {
        if (index >= 0 && index < resolutions.Length)
        {
            var validResolutions = new System.Collections.Generic.List<Resolution>();
            foreach (var res in resolutions)
            {
                float aspectRatio = (float)res.width / res.height;
                if (Mathf.Approximately(aspectRatio, 16f / 9f))
                {
                    validResolutions.Add(res);
                }
            }

            if (index >= 0 && index < validResolutions.Count)
            {
                Resolution selectedResolution = validResolutions[index];
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, isFullscreen);
                Debug.Log($"Установлено разрешение: {selectedResolution.width}x{selectedResolution.height}");
            }
            else
            {
                Debug.LogError("Недопустимый индекс разрешения!");
            }
        }
        else
        {
            Debug.LogError("Индекс выходит за пределы допустимых значений!");
        }
    }
    public void ToggleFullscreen(bool fullscreen)
    {
        isFullscreen = fullscreen;
        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, isFullscreen);
        Debug.Log($"Полноэкранный режим: {isFullscreen}");
    }
}