using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour { 

    GraphicRaycaster m_GraphicRayCaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    MidAirManager m_midAirManager;
    TouchHandler m_TouchHandler;

    [SerializeField]
    private Button resetButton;

    // Start is called before the first frame update
    void Start()
    {
        m_TouchHandler = FindObjectOfType<TouchHandler>();

        m_GraphicRayCaster = FindObjectOfType<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();
        m_midAirManager = FindObjectOfType<MidAirManager>();

        Vuforia.DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);

        resetButton.onClick.AddListener(()=> {
            Reset();
        });
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("The UI is being touched: " + IsCanvasPressed());
    }

    public void Reset()
    {
        Debug.Log("Reset was clicked");
        m_midAirManager.ResetScene();

    }

    public bool IsCanvasPressed()
    {
    
        
        m_PointerEventData = new PointerEventData(m_EventSystem)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        m_GraphicRayCaster.Raycast(m_PointerEventData, results);

        bool resultIsCanvasPressed = false;
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Toggle>() ||
                result.gameObject.GetComponent<Button>() ||
                result.gameObject.GetComponent<CanvasGroup>() ||
                result.gameObject.GetComponent<Image>())
            {
                resultIsCanvasPressed = true;
                break;
            }
        }
        
        return resultIsCanvasPressed;
    }

    void OnDevicePoseStatusChanged(Vuforia.TrackableBehaviour.Status status, Vuforia.TrackableBehaviour.StatusInfo statusInfo)
    {
        Debug.Log("OnDevicePoseStatusChanged(" + status + ", " + statusInfo + ")");

        switch (statusInfo)
        {
            case Vuforia.TrackableBehaviour.StatusInfo.INITIALIZING:
                //Debug.Log("Tracker Initializing");
                break;
            case Vuforia.TrackableBehaviour.StatusInfo.EXCESSIVE_MOTION:
                //Debug.Log("Excessive Motion");
                break;
            case Vuforia.TrackableBehaviour.StatusInfo.INSUFFICIENT_FEATURES:
                //Debug.Log("Insufficient Features");
                break;
            default:
                break;
        }

    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");

        Vuforia.DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }
}
