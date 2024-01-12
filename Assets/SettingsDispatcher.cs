using UnityEngine;

public class SettingsDispatcher : MonoBehaviour
{
    Canvas UICanvas;
    GameObject IntSettingPrefab;
    GameObject FloatSettingPrefab;
    GameObject BoolSettingPrefab;
    GameObject StringSettingPrefab;
    GameObject EnumSettingPrefab;


    Setting[] simulationSettings = new Setting[]
    {
        new Setting("Simulation Size", Setting.Type.Int, 64),
        new Setting("Simulation Tick Rate", Setting.Type.Int, 60),
        new Setting("Simulation Scale", Setting.Type.Int, 5),
        new Setting("Mouse Force", Setting.Type.Float, 100f),
        new Setting("Mouse Draw Value", Setting.Type.Float, 100f),
        new Setting("Mouse Draw Size", Setting.Type.Float, 1f),
    };
    Setting[] solverSettings = new Setting[]
    {
        new Setting("Fluid Viscosity", Setting.Type.Float, 0f),
        new Setting("Fluid Diffusion Rate", Setting.Type.Float, 0f),
    };
    Setting[] outputSettings = new Setting[]
    {
        new Setting("File Output Format", Setting.Type.Enum, Destinations.FileFormat.NONE),
        new Setting("File Output Path", Setting.Type.String, ""),
        new Setting("File Output Name", Setting.Type.String, ""),
        new Setting("Recording Time", Setting.Type.Int, 0),
    };

    void createSettingUI(Setting s)
    {
        GameObject settingUI;
        switch (s.type)
        {
            case Setting.Type.Int:
                settingUI = Instantiate(IntSettingPrefab, UICanvas.transform);
                break;
            case Setting.Type.Float:
                settingUI = Instantiate(FloatSettingPrefab, UICanvas.transform);
                break;
            case Setting.Type.Bool:
                settingUI = Instantiate(BoolSettingPrefab, UICanvas.transform);
                break;
            case Setting.Type.String:
                settingUI = Instantiate(StringSettingPrefab, UICanvas.transform);
                break;
            case Setting.Type.Enum:
                settingUI = Instantiate(EnumSettingPrefab, UICanvas.transform);
                break;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class Setting
{
    public enum Type
    {
        Float,
        Int,
        Bool,
        String,
        Enum
    }

    public string name;
    public Type type;
    public object value;
    public object defaultValue;

    public Setting(string name, Type type, object defaultValue)
    {
        this.name = name;
        this.type = type;
        this.defaultValue = defaultValue;
        this.value = defaultValue;
    }
}