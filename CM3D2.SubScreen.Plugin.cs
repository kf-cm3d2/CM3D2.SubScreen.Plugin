using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.SubScreen.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 OffScreen"),
    PluginVersion("0.2.1.0")]
    public class SubScreen : PluginBase
    {
        public const string Version = "0.2.1.0";

        const string DebugLogHeader = "OffScreen : ";

        public readonly string WinFileName = Directory.GetCurrentDirectory() + @"\UnityInjector\Config\SubScreen.png";

        const int SubScreenLayer = 11;

        const string PKeyEnable = "ENABLE";
        const string PKeyLookAtMaid = "LOOK_AT_MAID";

        const string PKeyAlwaysLookAtMaid = "ALWAYS_LOOK_AT_MAID";

        const string PKeyAlwaysLookAtFace = "ALWAYS_LOOK_AT_FACE";
        const string PPropLookAtFaceUp = "LOOK_AT_FACE.up";
        const string PPropLookAtFaceLeft = "LOOK_AT_FACE.left";
        const string PPropLookAtFaceFront = "LOOK_AT_FACE.front";

        const string PKeyAlwaysLookAtXxx = "ALWAYS_LOOK_AT_XXX";
        const string PPropLookAtXxxUp = "LOOK_AT_XXX.up";
        const string PPropLookAtXxxLeft = "LOOK_AT_XXX.left";
        const string PPropLookAtXxxFront = "LOOK_AT_XXX.front";

        const string PKeyResetCameraPos = "RESET_CAMERA_POS";
        const string PKeyMoveToBack = "MOVE_TO_BACK";

        const string PKeySubCamera = "SUB_CAMERA";
        const string PPropSubCameraPosX = "SUB_CAMERA.xMin";
        const string PPropSubCameraPosY = "SUB_CAMERA.yMin";
        const string PPropSubCameraWidth = "SUB_CAMERA.width";

        const string PKeyMainLight = "MAIN_LIGHT";
        const string PPropMainLightLuminance = "MAIN_LIGHT.l";
        const string PPropMainLightColorRed = "MAIN_LIGHT.r";
        const string PPropMainLightColorGreen = "MAIN_LIGHT.g";
        const string PPropMainLightColorBlue = "MAIN_LIGHT.b";

        const string PKeySubLight = "SUB_LIGHT";
        const string PPropSubLightRange = "SUB_LIGHT.range";
        const string PPropSubLightLuminance = "SUB_LIGHT.l";
        const string PPropSubLightColorRed = "SUB_LIGHT.r";
        const string PPropSubLightColorGreen = "SUB_LIGHT.g";
        const string PPropSubLightColorBlue = "SUB_LIGHT.b";

        const string PKeyCameraColor = "CAMERA_COLOR";
        const string PPropCameraColorRed = "CAMERA_COLOR.r";
        const string PPropCameraColorGreen = "CAMERA_COLOR.g";
        const string PPropCameraColorBlue = "CAMERA_COLOR.b";
        const string PPropCameraColorAlpha = "CAMERA_COLOR.a";

        const string PKeyScreenFilter = "SCREEN_FILTER";
        const string PPropScreenFilterLuminance = "SCREEN_FILTER.l";
        const string PPropScreenFilterRed = "SCREEN_FILTER.r";
        const string PPropScreenFilterGreen = "SCREEN_FILTER.g";
        const string PPropScreenFilterBlue = "SCREEN_FILTER.b";
        const string PPropScreenFilterAlpha = "SCREEN_FILTER.a";

        const string PKeyBsPos = "BS_POS";
        const string PPropBSPosX = "BS_POS.x";
        const string PPropBSPosY = "BS_POS.y";
        const string PPropBSPosZ = "BS_POS.z";

        const string PKeyBssize = "BS_SIZE";
        const string PPropBsSize = "BS_SIZE";

        const string PKeyBsAngle = "BS_ANGLE";
        const string PPropBsAngleX = "BS_ANGLE.x";
        const string PPropBsAngleY = "BS_ANGLE.y";
        const string PPropBsAngleZ = "BS_ANGLE.z";

        const string PKeyBsColor = "BS_COLOR";
        const string PPropScreenLIghtLuminance = "SCREEN_LIGHT.l";
        const string PPropBsColorLuminance = "BS_COLOR.l";
        const string PPropBsColorRed = "BS_COLOR.r";
        const string PPropBsColorGreen = "BS_COLOR.g";
        const string PPropBsColorBlue = "BS_COLOR.b";
        const string PPropBsColorAlpha = "BS_COLOR.a";

        private enum TargetLevel
        {
            //ダンス:ドキドキ☆Fallin' Love
            SceneDance_DDFL = 4,

            // エディット
            SceneEdit = 5,

            // 夜伽
            SceneYotogi = 14,

            // ADVパート
            SceneADV = 15,

            // ダンス:entrance to you
            SceneDance_ETYL = 20
        }

        const float LowSpeed = 1f;

        const float HighSpeed = 6f;

        const int RenderTextureScale = 1;

        private GameObject goSubCam;

        private GameObject goSubLight;

        private GameObject goSubScreen;

        private GameObject goSsLight;

        private GameObject goSsFrontFilter;

        private RenderTexture rTex;

        private Maid maid;

        private SubScreenParam ssParam;

        private PixelValues pv;

        private Rect winRect;

        private Vector2 lastScreenSize;

        private Vector2 scrollViewVector = Vector2.zero;

        private float guiWidth = 0.25f;

        private bool xmlLoaded;

        private bool guiVisible = false;

        private bool bsEnable = false;

        Dictionary<string, UIButton[]> uiOnOffButton = new Dictionary<string, UIButton[]>();
        Dictionary<string, UIButton> uiCommandButton = new Dictionary<string, UIButton>();
        Dictionary<string, Dictionary<string, UILabel>> uiValueLable = new Dictionary<string, Dictionary<string, UILabel>>();

        Dictionary<string, float> currentValues = new Dictionary<string, float>();

        private class SubScreenParam
        {
            public readonly string DefMatchPattern = @"([-+]?[0-9]*\.?[0-9]+)";
            public readonly string XmlFileName = Directory.GetCurrentDirectory() + @"\UnityInjector\Config\SubScreenParam.xml";

            public string XmlFormat;
            public List<string> sKey = new List<string>();

            public Dictionary<string, bool> bEnabled = new Dictionary<string, bool>();
            public Dictionary<string, string> sDescription = new Dictionary<string, string>();
            public Dictionary<string, string> sType = new Dictionary<string, string>();
            public Dictionary<string, bool> bVisible = new Dictionary<string, bool>();

            public Dictionary<string, string[]> sPropName = new Dictionary<string, string[]>();
            public Dictionary<string, Dictionary<string, float>> fValue = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, float>> fVmin = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, float>> fVmax = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, float>> fVdef = new Dictionary<string, Dictionary<string, float>>();
            public Dictionary<string, Dictionary<string, string>> sVType = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, string>> sLabel = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, string>> sMatchPattern = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, bool>> bVVisible = new Dictionary<string, Dictionary<string, bool>>();

            public int KeyCount { get { return sKey.Count; } }
            public int ValCount(string key) { return sPropName[key].Length; }

            //--------

            public SubScreenParam()
            {
                Init();
            }

            public bool Init()
            {
                if (!loadXML())
                {
                    Debug.LogError(DebugLogHeader + "loadXML() failed.");
                    return false;
                }

                return true;
            }

            public bool IsToggle(string key)
            {
                return (sType[key] == "toggle") ? true : false;
            }

            public bool IsButton(string key)
            {
                return (sType[key] == "button") ? true : false;
            }

            public bool IsCheck(string key)
            {
                return (sType[key] == "check") ? true : false;
            }

            //--------

            private bool loadXML()
            {
                if (!File.Exists(XmlFileName))
                {
                    Debug.LogError(DebugLogHeader + "\"" + XmlFileName + "\" does not exist.");
                    return false;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(XmlFileName);

                XmlNode mods = doc.DocumentElement;
                XmlFormat = ((XmlElement)mods).GetAttribute("format");
                if (XmlFormat != "1.0")
                {
                    Debug.LogError(DebugLogHeader + SubScreen.Version + " requires fomart=\"1.0\" of SubScreenParam.xml.");
                    return false;
                }

                XmlNodeList modNodeS = mods.SelectNodes("/mods/mod");
                if (!(modNodeS.Count > 0))
                {
                    Debug.LogError(DebugLogHeader + "\"" + XmlFileName + "\" has no <mod>elements.");
                    return false;
                }

                sKey.Clear();

                foreach (XmlNode modNode in modNodeS)
                {
                    // mod属性
                    string key = ((XmlElement)modNode).GetAttribute("id");
                    if (key != "" && !sKey.Contains(key)) sKey.Add(key);
                    else continue;

                    bool b = false;
                    bEnabled[key] = false;
                    sDescription[key] = ((XmlElement)modNode).GetAttribute("description");
                    bVisible[key] = (Boolean.TryParse(((XmlElement)modNode).GetAttribute("visible"), out b)) ? b : true;

                    sType[key] = ((XmlElement)modNode).GetAttribute("type");

                    if (sType[key] == "") sType[key] = "slider";
                    if (IsCheck(key)) continue;
                    if (IsButton(key)) continue;

                    XmlNodeList valueNodeS = ((XmlElement)modNode).GetElementsByTagName("value");
                    if (!(valueNodeS.Count > 0)) continue;

                    sPropName[key] = new string[valueNodeS.Count];
                    fValue[key] = new Dictionary<string, float>();
                    fVmin[key] = new Dictionary<string, float>();
                    fVmax[key] = new Dictionary<string, float>();
                    fVdef[key] = new Dictionary<string, float>();
                    sVType[key] = new Dictionary<string, string>();
                    sLabel[key] = new Dictionary<string, string>();
                    sMatchPattern[key] = new Dictionary<string, string>();
                    bVVisible[key] = new Dictionary<string, bool>();

                    // value属性
                    int j = 0;
                    foreach (XmlNode valueNode in valueNodeS)
                    {
                        float x = 0f;

                        string prop = ((XmlElement)valueNode).GetAttribute("prop_name");
                        if (prop != "" && Array.IndexOf(sPropName[key], prop) < 0)
                        {
                            sPropName[key][j] = prop;
                        }
                        else
                        {
                            sKey.Remove(key);
                            break;
                        }

                        sVType[key][prop] = ((XmlElement)valueNode).GetAttribute("type");
                        switch (sVType[key][prop])
                        {
                            case "num": break;
                            case "scale": break;
                            case "int": break;
                            case "button": break;
                            default: sVType[key][prop] = "num"; break;
                        }

                        fVmin[key][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("min"), out x) ? x : 0f;
                        fVmax[key][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("max"), out x) ? x : 0f;
                        fVdef[key][prop] = Single.TryParse(((XmlElement)valueNode).GetAttribute("default"), out x) ? x : (sVType[key][prop] == "scale") ? 1f : 0f;
                        fValue[key][prop] = fVdef[key][prop];

                        sLabel[key][prop] = ((XmlElement)valueNode).GetAttribute("label");
                        sMatchPattern[key][prop] = ((XmlElement)valueNode).GetAttribute("match_pattern");
                        bVVisible[key][prop] = (Boolean.TryParse(((XmlElement)valueNode).GetAttribute("visible"), out b)) ? b : true;

                        j++;
                    }
                    if (j == 0) sKey.Remove(key);
                }

                return true;
            }
        }

        private class PixelValues
        {
            public float BaseWidth = 1280f;
            public float PropRatio = 0.6f;
            public int Margin;

            private Dictionary<string, int> font = new Dictionary<string, int>();
            private Dictionary<string, int> line = new Dictionary<string, int>();
            private Dictionary<string, int> sys = new Dictionary<string, int>();

            public PixelValues()
            {
                Margin = PropPx(10);

                font["C1"] = 11;
                font["C2"] = 12;
                font["H1"] = 14;
                font["H2"] = 16;
                font["H3"] = 20;

                line["C1"] = 14;
                line["C2"] = 18;
                line["H1"] = 22;
                line["H2"] = 24;
                line["H3"] = 30;

                sys["Menu.Height"] = 45;
                sys["OkButton.Height"] = 95;

                sys["HScrollBar.Width"] = 15;
            }

            public int Font(string key) { return PropPx(font[key]); }
            public int Line(string key) { return PropPx(line[key]); }
            public int Sys(string key) { return PropPx(sys[key]); }

            public int Font_(string key) { return font[key]; }
            public int Line_(string key) { return line[key]; }
            public int Sys_(string key) { return sys[key]; }

            public Rect PropScreen(float left, float top, float width, float height)
            {
                return new Rect((int)((Screen.width - Margin * 2) * left + Margin)
                               , (int)((Screen.height - Margin * 2) * top + Margin)
                               , (int)((Screen.width - Margin * 2) * width)
                               , (int)((Screen.height - Margin * 2) * height));
            }

            public Rect PropScreenMH(float left, float top, float width, float height)
            {
                Rect r = PropScreen(left, top, width, height);
                r.y += Sys("Menu.Height");
                r.height -= (Sys("Menu.Height") + Sys("OkButton.Height"));

                return r;
            }

            public Rect PropScreenMH(float left, float top, float width, float height, Vector2 last)
            {
                Rect r = PropScreen((float)(left / (last.x - Margin * 2)), (float)(top / (last.y - Margin * 2)), width, height);
                r.height -= (Sys("Menu.Height") + Sys("OkButton.Height"));

                return r;
            }

            public Rect InsideRect(Rect rect)
            {
                return new Rect(Margin, Margin, rect.width - Margin * 2, rect.height - Margin * 2);
            }

            public Rect InsideRect(Rect rect, int height)
            {
                return new Rect(Margin, Margin, rect.width - Margin * 2, height);
            }

            public int PropPx(int px)
            {
                return (int)(px * (1f + (Screen.width / BaseWidth - 1f) * PropRatio));
            }
        }

        private void Awake()
        {
            ssParam = new SubScreenParam();
            pv = new PixelValues();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }

        private void OnLevelWasLoaded(int level)
        {
            if (!Enum.IsDefined(typeof(TargetLevel), level))
            {
                return;
            }
            bsEnable = false;
            xmlLoaded = ssParam.Init();
            winRect = pv.PropScreenMH(1f - guiWidth, 0f, guiWidth, 1f);
        }

        private void Update()
        {
            if (!Enum.IsDefined(typeof(TargetLevel), Application.loadedLevel))
            {
                if (guiVisible) guiVisible = false;
                return;
            }

            if (ssParam != null)
            {
                if (guiVisible)
                {
                    if (winRect.Contains(Input.mousePosition))
                    {
                        GameMain.Instance.MainCamera.SetControl(false);
                    }
                    else
                    {
                        GameMain.Instance.MainCamera.SetControl(true);
                    }
                }
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    guiVisible = !guiVisible;
                }

                if (bsEnable)
                {

                    InputCheck();
                    showSubCamera();
                    applyScreen();
                }
            }
        }

        private void showSubCamera()
        {
            if (goSubCam != null)
            {
                if (ssParam.bEnabled[PKeyEnable])
                {
                    goSubCam.renderer.enabled = true;
                    goSubCam.camera.enabled = true;
                    if (ssParam.bEnabled[PKeySubCamera])
                    {
                        goSubCam.camera.targetTexture = null;
                        goSubScreen.renderer.enabled = false;
                        goSsLight.light.enabled = false;
                    }
                    else
                    {
                        goSubCam.camera.targetTexture = rTex;
                        goSubScreen.renderer.enabled = true;
                        goSsLight.light.enabled = true;
                    }
                    if (ssParam.bEnabled[PKeyAlwaysLookAtFace])
                    {
                        Transform tr = maid.body0.trsHead;
                        goSubCam.transform.position = tr.position;
                        goSubCam.transform.position += tr.TransformDirection(Vector3.up) * ssParam.fValue[PKeyAlwaysLookAtFace][PPropLookAtFaceFront];//前後
                        goSubCam.transform.position += tr.TransformDirection(Vector3.left) * ssParam.fValue[PKeyAlwaysLookAtFace][PPropLookAtFaceUp];//上下
                        goSubCam.transform.position += tr.TransformDirection(Vector3.back) * ssParam.fValue[PKeyAlwaysLookAtFace][PPropLookAtFaceLeft];//左右
                        goSubCam.transform.LookAt(tr);
                    }
                    else if (ssParam.bEnabled[PKeyAlwaysLookAtXxx])
                    {
                        Transform tr = maid.body0.Pelvis.transform;
                        goSubCam.transform.position = tr.position;
                        goSubCam.transform.position += tr.TransformDirection(Vector3.up) * ssParam.fValue[PKeyAlwaysLookAtXxx][PPropLookAtXxxFront];//前後
                        goSubCam.transform.position += tr.TransformDirection(Vector3.left) * ssParam.fValue[PKeyAlwaysLookAtXxx][PPropLookAtXxxUp];//上下
                        goSubCam.transform.position += tr.TransformDirection(Vector3.back) * ssParam.fValue[PKeyAlwaysLookAtXxx][PPropLookAtXxxLeft];//左右
                        goSubCam.transform.LookAt(tr.position);
                    }
                    else if (ssParam.bEnabled[PKeyAlwaysLookAtMaid])
                    {
                        goSubCam.transform.LookAt(maid.body0.trsHead.transform);
                    }
                }
                else
                {
                    goSubCam.renderer.enabled = false;
                    goSubCam.camera.enabled = false;
                    goSubScreen.renderer.enabled = false;
                }
            }
        }

        private void InputCheck()
        {
            var speed = LowSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speed = HighSpeed;
            }
            if (Input.GetKey(KeyCode.W))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        goSubCam.transform.Rotate(new Vector3(speed * Time.deltaTime * -20f, 0f, 0f));
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        goSubCam.transform.Rotate(new Vector3(speed * Time.deltaTime * 20f, 0f, 0f));
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position += new Vector3(0, 0, speed * Time.deltaTime);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.back) * speed * Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        goSubCam.transform.Rotate(new Vector3(0f, speed * Time.deltaTime * -20f, 0f));
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.left) * speed * Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        goSubCam.transform.Rotate(new Vector3(0f, speed * Time.deltaTime * 20f, 0f));
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.right) * speed * Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position -= new Vector3(0f, speed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.down) * speed * Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.E))
            {
                if (goSubCam.renderer.enabled)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        goSubCam.transform.position += new Vector3(0f, speed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        goSubCam.transform.position += goSubCam.transform.TransformDirection(Vector3.up) * speed * Time.deltaTime;
                    }
                }
            }
        }

        private void createScreen()
        {
            Debug.Log(DebugLogHeader + "::createObjects ");
            goSubScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goSubScreen.layer = SubScreenLayer;
            goSsFrontFilter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goSsFrontFilter.renderer.enabled = false;
            goSsFrontFilter.layer = SubScreenLayer;
            goSsFrontFilter.transform.SetParent(goSubScreen.transform);
            goSsFrontFilter.transform.localScale = new Vector3(1f, 1f, 0.001f);
            goSsFrontFilter.transform.localPosition += new Vector3(0, 0, 0.001f);
            goSsFrontFilter.renderer.material.shader = Shader.Find("Transparent/Diffuse");
            goSubScreen.transform.localScale = new Vector3(1f, 1f, 0.01f);
            goSsLight = new GameObject();
            goSsLight.transform.localPosition += new Vector3(0, 0.5f, 0.5f);
            Light ssLight = goSsLight.AddComponent<Light>();
            ssLight.enabled = false;
            ssLight.light.type = LightType.Directional;
            ssLight.light.transform.LookAt(goSubScreen.transform);
            ssLight.light.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
            ssLight.light.cullingMask &= ~(1 << LayerMask.NameToLayer("BackGround"));
            ssLight.light.cullingMask &= ~(1 << LayerMask.NameToLayer("Charactor"));
            ssLight.intensity = 0.5f;
            goSsLight.transform.SetParent(goSubScreen.transform);
            goSubScreen.renderer.enabled = false;

            goSubCam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goSubCam.renderer.enabled = false;
            goSubCam.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            Camera cam = goSubCam.AddComponent<Camera>();
            cam.cullingMask &= ~(1 << SubScreenLayer);
            cam.enabled = false;
            cam.depth = 9;
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            goSubLight = new GameObject("sub light");
            goSubLight.AddComponent<Light>();
            goSubLight.transform.SetParent(goSubCam.transform);
            goSubLight.light.type = LightType.Spot;
            goSubLight.light.range = 10;
            goSubLight.light.enabled = false;
            goSubLight.light.cullingMask &= ~(1 << SubScreenLayer);

            rTex = new RenderTexture(
                Screen.width / RenderTextureScale,
                Screen.height / RenderTextureScale,
                24);
            cam.targetTexture = rTex;
            goSubScreen.renderer.material.mainTexture = rTex;
            goSubCam.renderer.material.shader = Shader.Find("Transparent/Diffuse");
            goSubScreen.renderer.material.shader = Shader.Find("Transparent/Diffuse");

            maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            Transform mainTransform = GameMain.Instance.MainCamera.transform;
            onClickButton(PKeyMoveToBack);
            goSubCam.transform.position = mainTransform.position;
            goSubCam.transform.LookAt(maid.body0.trsHead.transform);

            StartCoroutine(SetLocalTexture
                 (goSubCam, Application.dataPath + "/../UnityInjector/Config/SubScreenCamera.png"));
            StartCoroutine(SetLocalTexture
                 (goSsFrontFilter, Application.dataPath + "/../UnityInjector/Config/SubScreenFilter.png"));
        }

        private System.Collections.IEnumerator SetLocalTexture(GameObject gameObject, string filePath)
        {
            Debug.Log(DebugLogHeader + "SetLocalTexture:" + filePath);
            WWW file = new WWW("file://" + filePath);
            yield return file;
            gameObject.renderer.material.mainTexture = file.texture;
        }

        public void onClickButton(String key)
        {
            Debug.Log(DebugLogHeader + key);
            if (key.Equals(PKeyLookAtMaid))
            {
                goSubCam.camera.transform.LookAt(maid.body0.trsHead.transform);
            }
            else if (key.Equals(PKeyResetCameraPos))
            {
                Transform mainTransform = GameMain.Instance.MainCamera.transform;
                Vector3 vec = maid.transform.position;
                vec.y = goSubScreen.transform.position.y;
                goSubScreen.transform.LookAt(vec);
                goSubCam.transform.position = new Vector3(mainTransform.position.x, mainTransform.position.y, mainTransform.position.z);
                goSubCam.transform.LookAt(maid.body0.trsHead.transform);
            }
            else if (key.Equals(PKeyMoveToBack))
            {
                Vector3 vec = GameMain.Instance.MainCamera.GetTargetPos();
                vec.y = 1.3f;
                vec.z -= 1.3f;
                goSubScreen.transform.position = vec;
                ssParam.fValue[PKeyBsPos][PPropBSPosX] = goSubScreen.transform.position.x;
                ssParam.fValue[PKeyBsPos][PPropBSPosY] = goSubScreen.transform.position.y;
                ssParam.fValue[PKeyBsPos][PPropBSPosZ] = goSubScreen.transform.position.z;
            }
        }

        public void applyScreen()
        {
            if (ssParam.bEnabled[PKeySubCamera])
            {
                goSubCam.camera.rect = new Rect(ssParam.fValue[PKeySubCamera][PPropSubCameraPosX], ssParam.fValue[PKeySubCamera][PPropSubCameraPosY],
                    ssParam.fValue[PKeySubCamera][PPropSubCameraWidth], ssParam.fValue[PKeySubCamera][PPropSubCameraWidth]);
            }
            else
            {
                goSubCam.camera.rect = new Rect(0, 0, 1f, 1f);
            }
            goSubScreen.transform.position = new Vector3(
            ssParam.fValue[PKeyBsPos][PPropBSPosX],
            ssParam.fValue[PKeyBsPos][PPropBSPosY],
            ssParam.fValue[PKeyBsPos][PPropBSPosZ]);
            var x = ssParam.fValue[PKeyBssize][PPropBsSize];
            var y = x * Screen.height / Screen.width;
            goSubScreen.transform.localScale = new Vector3(x, y, goSubScreen.transform.localScale.z);

            goSubScreen.transform.eulerAngles = new Vector3(ssParam.fValue[PKeyBsAngle][PPropBsAngleX],
                ssParam.fValue[PKeyBsAngle][PPropBsAngleY], ssParam.fValue[PKeyBsAngle][PPropBsAngleZ]);
            Color color = goSubScreen.renderer.material.color;
            color.r = ssParam.fValue[PKeyBsColor][PPropBsColorRed] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.g = ssParam.fValue[PKeyBsColor][PPropBsColorGreen] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.b = ssParam.fValue[PKeyBsColor][PPropBsColorBlue] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.a = ssParam.fValue[PKeyBsColor][PPropBsColorAlpha];
            goSubScreen.renderer.material.color = color;
            goSsLight.light.intensity = ssParam.fValue[PKeyBsColor][PPropScreenLIghtLuminance];

            color = goSubCam.renderer.material.color;
            color.r = ssParam.fValue[PKeyCameraColor][PPropCameraColorRed] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.g = ssParam.fValue[PKeyCameraColor][PPropCameraColorGreen] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.b = ssParam.fValue[PKeyCameraColor][PPropCameraColorBlue] * ssParam.fValue[PKeyBsColor][PPropBsColorLuminance];
            color.a = ssParam.fValue[PKeyCameraColor][PPropCameraColorAlpha];
            goSubCam.renderer.material.color = color;

            if (ssParam.bEnabled[PKeyMainLight])
            {
                Light mainLight = GameMain.Instance.MainLight.light;
                color = mainLight.color;
                color.r = ssParam.fValue[PKeyMainLight][PPropMainLightColorRed] * ssParam.fValue[PKeyMainLight][PPropMainLightLuminance];
                color.g = ssParam.fValue[PKeyMainLight][PPropMainLightColorGreen] * ssParam.fValue[PKeyMainLight][PPropMainLightLuminance];
                color.b = ssParam.fValue[PKeyMainLight][PPropMainLightColorBlue] * ssParam.fValue[PKeyMainLight][PPropMainLightLuminance];
                mainLight.color = color;
            }

            if (ssParam.bEnabled[PKeySubLight])
            {
                goSubLight.light.enabled = true;
                goSubLight.light.spotAngle = ssParam.fValue[PKeySubLight][PPropSubLightRange];
                color = goSubLight.light.color;
                color.r = ssParam.fValue[PKeySubLight][PPropSubLightColorRed] * ssParam.fValue[PKeySubLight][PPropSubLightLuminance];
                color.g = ssParam.fValue[PKeySubLight][PPropSubLightColorGreen] * ssParam.fValue[PKeySubLight][PPropSubLightLuminance];
                color.b = ssParam.fValue[PKeySubLight][PPropSubLightColorBlue] * ssParam.fValue[PKeySubLight][PPropSubLightLuminance];
                goSubLight.light.color = color;
            }
            else
            {
                goSubLight.light.enabled = false;
            }
            if (ssParam.bEnabled[PKeyScreenFilter])
            {
                goSsFrontFilter.renderer.enabled = true;
                color = goSsFrontFilter.renderer.material.color;
                color.r = ssParam.fValue[PKeyScreenFilter][PPropScreenFilterRed] * ssParam.fValue[PKeyScreenFilter][PPropScreenFilterLuminance];
                color.g = ssParam.fValue[PKeyScreenFilter][PPropScreenFilterGreen] * ssParam.fValue[PKeyScreenFilter][PPropScreenFilterLuminance];
                color.b = ssParam.fValue[PKeyScreenFilter][PPropScreenFilterBlue] * ssParam.fValue[PKeyScreenFilter][PPropScreenFilterLuminance];
                color.a = ssParam.fValue[PKeyScreenFilter][PPropScreenFilterAlpha];
                goSsFrontFilter.renderer.material.color = color;
            }
            else
            {
                goSsFrontFilter.renderer.enabled = false;
            }
        }

        public void OnGUI()
        {
            if (!Enum.IsDefined(typeof(TargetLevel), Application.loadedLevel))
            {
                return;
            }
            if (!guiVisible) return;

            maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            if (maid == null) return;

            GUIStyle winStyle = "box";
            winStyle.fontSize = pv.Font("C1");
            winStyle.alignment = TextAnchor.UpperRight;

            if (lastScreenSize != new Vector2(Screen.width, Screen.height))
            {
                winRect = pv.PropScreenMH(winRect.x, winRect.y, guiWidth, 1f, lastScreenSize);
                lastScreenSize = new Vector2(Screen.width, Screen.height);
            }
            winRect = GUI.Window(0, winRect, addGUI, SubScreen.Version, winStyle);

            if (!bsEnable && ssParam.bEnabled[PKeyEnable])
            {
                maid = GameMain.Instance.CharacterMgr.GetMaid(0);
                createScreen();
            }
            bsEnable = ssParam.bEnabled[PKeyEnable];
        }

        private void addGUI(int winID)
        {
            int mod_num = ssParam.KeyCount;
            Rect baseRect = pv.InsideRect(this.winRect);
            Rect headerRect = new Rect(baseRect.x, baseRect.y, baseRect.width, pv.Line("H3"));
            Rect scrollRect = new Rect(baseRect.x, baseRect.y + headerRect.height + pv.Margin
                                      , baseRect.width + pv.PropPx(5), baseRect.height - headerRect.height - pv.Margin);
            Rect conRect = new Rect(0, 0, scrollRect.width - pv.Sys_("HScrollBar.Width") - pv.Margin, 0);
            Rect outRect = new Rect();
            GUIStyle lStyle = "label";
            GUIStyle tStyle = "toggle";
            GUIStyle bStyle = "button";
            Color color = new Color(1f, 1f, 1f, 0.98f);

            for (int i = 0; i < mod_num; i++)
            {
                string key = ssParam.sKey[i];

                conRect.height += pv.Line("H1");
                if (ssParam.sType[key] == "check")
                {
                    conRect.height += pv.Margin;
                }
                else if (ssParam.sType[key] == "button")
                {
                    conRect.height += pv.Margin;
                }
                else
                {
                    for (int j = 0; j < ssParam.ValCount(key); j++) conRect.height += pv.Line("H1");
                    conRect.height += pv.Margin * 2;
                }
            }

            lStyle.normal.textColor = color;
            tStyle.normal.textColor = color;
            bStyle.normal.textColor = color;
            lStyle.fontSize = pv.Font("H3");
            bStyle.fontSize = pv.Font("H1");

            drawWinHeader(headerRect, "サブスクリーン", lStyle);

            // スクロールビュー
            scrollViewVector = GUI.BeginScrollView(scrollRect, scrollViewVector, conRect);

            // 各modスライダー
            outRect.Set(0, 0, conRect.width, 0);
            for (int i = 0; i < mod_num; i++)
            {
                string key = ssParam.sKey[i];

                //----
                outRect.width = conRect.width;
                outRect.height = pv.Line("H1");
                color = new Vector4(1f, 1f, 1f, 0.98f);
                lStyle.normal.textColor = color;
                tStyle.normal.textColor = color;

                tStyle.fontSize = pv.Font("H1");

                if (ssParam.sType[key] == "button")
                {
                    if (GUI.Button(outRect, ssParam.sDescription[key], bStyle))
                    {
                        onClickButton(key);
                    }
                    outRect.y += 40 + pv.Margin;
                    continue;
                }

                if (ssParam.sType[key] == "check")
                {
                    ssParam.bEnabled[key] = GUI.Toggle(outRect, ssParam.bEnabled[key], ssParam.sDescription[key] + " (" + key + ")", tStyle);
                    outRect.y += outRect.height;
                    outRect.y += pv.Margin;
                    continue;
                }
                if (ssParam.sType[key] == "toggle")
                {
                    ssParam.bEnabled[key] = GUI.Toggle(outRect, ssParam.bEnabled[key], ssParam.sDescription[key] + " (" + key + ")", tStyle);
                    outRect.y += outRect.height;
                    outRect.y += pv.Margin;
                    if (!ssParam.bEnabled[key]) continue;
                }
                else
                {
                    // slider
                    GUI.Label(outRect, ssParam.sDescription[key], tStyle);
                    outRect.y += outRect.height;
                }
                int val_num = ssParam.ValCount(key);
                for (int j = 0; j < val_num; j++)
                {
                    string prop = ssParam.sPropName[key][j];

                    float value = ssParam.fValue[key][prop];
                    float vmin = ssParam.fVmin[key][prop];
                    float vmax = ssParam.fVmax[key][prop];
                    string label = ssParam.sLabel[key][prop] + " : " + value.ToString("F");
                    string vType = ssParam.sVType[key][prop];

                    outRect.width = conRect.width;
                    outRect.height = pv.Line("H1");
                    lStyle.fontSize = pv.Font("H1");
                    if (value < vmin) value = vmin;
                    if (value > vmax) value = vmax;
                    if (vType == "scale" && vmin < 1f)
                    {
                        if (vmin < 0f) vmin = 0f;
                        if (value < 0f) value = 0f;

                        float tmpmin = -Mathf.Abs(vmax - 1f);
                        float tmpmax = Mathf.Abs(vmax - 1f);
                        float tmp = (value < 1f) ? tmp = Mathf.Abs((1f - value) / (1f - vmin)) * tmpmin : value - 1f;

                        if (tmp < tmpmin) tmp = tmpmin;
                        if (tmp > tmpmax) tmp = tmpmax;

                        tmp = drawModValueSlider(outRect, tmp, tmpmin, tmpmax, label, lStyle);

                        ssParam.fValue[key][prop] = (tmp < 0f) ? 1f - tmp / tmpmin * Mathf.Abs(1f - vmin) : 1f + tmp;
                    }
                    else if (vType == "int")
                    {
                        value = (int)Mathf.Round(value);
                        ssParam.fValue[key][prop] = (int)Mathf.Round(drawModValueSlider(outRect, value, vmin, vmax, label, lStyle));
                    }
                    else ssParam.fValue[key][prop] = drawModValueSlider(outRect, value, vmin, vmax, label, lStyle);

                    outRect.y += outRect.height;
                }

                outRect.y += pv.Margin * 2;
            }

            GUI.EndScrollView();
            GUI.DragWindow();
        }

        private int fixPx(int px)
        {
            float mag = 1f + (Screen.width / 1280f - 1f) * 0.6f;

            return (int)(mag * px);
        }

        private void drawWinHeader(Rect rect, string s, GUIStyle style)
        {
            GUI.Label(rect, s, style);
            {
                ;
            }
        }

        private float drawModValueSlider(Rect outRect, float value, float min, float max, string label, GUIStyle lstyle)
        {
            float conWidth = outRect.width;

            outRect.width = conWidth * 0.3f;
            GUI.Label(outRect, label, lstyle);
            outRect.x += outRect.width;

            outRect.width = conWidth * 0.7f;
            outRect.y += pv.PropPx(5);
            return GUI.HorizontalSlider(outRect, value, min, max);
        }


    }
}