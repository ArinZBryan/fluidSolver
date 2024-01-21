using UnityEngine;

public class SettingsDispatcher : MonoBehaviour
{
    public Canvas UICanvas;
    public GameObject IntSettingPrefab;
    public GameObject FloatSettingPrefab;
    public GameObject BoolSettingPrefab;
    public GameObject StringSettingPrefab;
    public GameObject EnumSettingPrefab;


    Setting[] simulationSettings = new Setting[]
    {
        new Setting.Int     ("Simulation Size",         64,     8,      256,    1),
        new Setting.Int     ("Simulation Tick Rate",    60,     1,      120,    1),
        new Setting.Int     ("Simulation Scale",        5,      5,      15,     2),
        new Setting.Float   ("Mouse Force",             100f,   0.5f,   1000f,  null),
        new Setting.Float   ("Mouse Draw Value",        100f,   0.5f,   1000f,  null),
        new Setting.Float   ("Mouse Draw Size",         1f,     1f,     10f,    0.5f),
    };
    Setting[] solverSettings = new Setting[]
    {
        new Setting.Float   ("Fluid Viscosity",         0f,     0f,     1f,     null),
        new Setting.Float   ("Fluid Diffusion Rate",    0f,     0f,     1f,     null),
    };
    Setting[] outputSettings = new Setting[]
    {
        new Setting.Enum   ("File Output Format",       Destinations.FileFormat.NONE, typeof(Destinations.FileFormat)),
        new Setting.String ("File Output Path",         "", (x) => System.IO.Directory.Exists(x) ? x : null),
        new Setting.String ("File Output Name",         "", (x) => x),
        new Setting.Int    ("Recording Time",           0,      0,      1_000_000, 1),
    };

    GameObject createSettingUI(Setting s)
    {
        GameObject settingUI;
        switch (s)
        {
            case Setting.Int:
                settingUI = Instantiate(IntSettingPrefab, UICanvas.transform);
                Setting.Int intSetting = (Setting.Int)s;
                settingUI.GetComponent<GenericIntSetting>().setup(intSetting.maximumValue, intSetting.minimumValue, (int)intSetting.defaultValue, intSetting.stepSize, intSetting.name);
                break;
            case Setting.Float:
                settingUI = Instantiate(FloatSettingPrefab, UICanvas.transform);
                Setting.Float floatSetting = (Setting.Float)s;
                settingUI.GetComponent<GenericFloatSetting>().setup(floatSetting.maximumValue, floatSetting.minimumValue, (float)floatSetting.defaultValue, floatSetting.stepSize, floatSetting.name);
                break;
            case Setting.Bool:
                settingUI = Instantiate(BoolSettingPrefab, UICanvas.transform);
                Setting.Bool boolSetting = (Setting.Bool)s;
                settingUI.GetComponent<GenericBooleanSetting>().setup((bool)boolSetting.defaultValue, boolSetting.name);
                break;
            case Setting.String:
                settingUI = Instantiate(StringSettingPrefab, UICanvas.transform);
                Setting.String stringSetting = (Setting.String)s;
                settingUI.GetComponent<GenericStringSetting>().setup((string)stringSetting.defaultValue, stringSetting.verifyInput, stringSetting.name);
                break;
            case Setting.Enum:
                settingUI = Instantiate(EnumSettingPrefab, UICanvas.transform);
                Setting.Enum enumSetting = (Setting.Enum)s;
                settingUI.GetComponent<GenericEnumSetting>().setup(enumSetting.settingEnum, (System.Enum)enumSetting.defaultValue, enumSetting.name);
                break;
            default:
                settingUI = new GameObject();
                settingUI.transform.name = "Broken Setting";
                break;
        }
        return settingUI;
    }
    void makeSettings(Setting[] settings)
    {
        for (int i = 0; i < settings.Length; i++)
        {
            GameObject settingUI = createSettingUI(settings[i]);
            settingUI.transform.localPosition = new Vector3(0, -i * 50, 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        makeSettings(simulationSettings);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class Setting
{
    public string name;
    public object value;
    public object defaultValue;

    Setting(string name, object defaultValue)
    {
        this.name = name;
        this.defaultValue = defaultValue;
        this.value = defaultValue;
    }

    public class Int : Setting
    {
        public int minimumValue;
        public int maximumValue;
        public int stepSize;
        public Int(string name, int defaultValue, int miniumValue, int maximumValue, int stepSize) : base(name, defaultValue)
        {
            this.value = defaultValue;
            this.defaultValue = defaultValue;
            this.minimumValue = miniumValue;
            this.maximumValue = maximumValue;
            this.stepSize = stepSize;
        }
    }
    public class Float : Setting
    {
        public float minimumValue;
        public float maximumValue;
        public float? stepSize;
        public Float(string name, float defaultValue, float miniumValue, float maximumValue, float? stepSize) : base(name, defaultValue)
        {
            this.value = defaultValue;
            this.defaultValue = defaultValue;
            this.minimumValue = miniumValue;
            this.maximumValue = maximumValue;
            this.stepSize = stepSize;
        }
    }
    public class Bool : Setting
    {
        public Bool(string name, bool defaultValue) : base(name, defaultValue)
        {
            this.value = defaultValue;
            this.defaultValue = defaultValue;
        }
    }
    public class String : Setting
    {
        public GenericStringSetting.verificationFunction verifyInput;
        public String(string name, string defaultValue, GenericStringSetting.verificationFunction f) : base(name, defaultValue)
        {
            this.value = defaultValue;
            this.defaultValue = defaultValue;
            this.verifyInput = f;
        }
    }
    public class Enum : Setting
    {
        public System.Type settingEnum;
        public Enum(string name,  System.Enum defaultValue, System.Type settingEnum) : base(name, defaultValue)
        {
            this.value = defaultValue;
            this.defaultValue = defaultValue;
            this.settingEnum = settingEnum;
        }
    }
}
