using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class MidAirManager : MonoBehaviour
{

    public MidAirPositionerBehaviour m_MidAirPositioner;
    public GameObject m_MidAirAugmentation;
    public AnchorBehaviour m_MidAirAnchor;
    public ContentPositioningBehaviour m_ContentPositioningBehaviour;

    public static bool AnchorExists
    {
        get { return anchorExists; }
        private set { anchorExists = value; }
    }

    StateManager m_StateManager;
    SmartTerrain m_SmartTerrain;
    PositionalDeviceTracker m_PositionalDeviceTracker;
    
    int AutomaticHitTestFrameCount;
    int m_AnchorCounter;
    static bool anchorExists;

    // Start is called before the first frame update
    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.RegisterTrackerStartedCallback(OnTrackerStarted);
        DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);

        //m_MidAirAnchor = m_MidAirAugmentation.GetComponentInParent<AnchorBehaviour>();
        UtilityHelper.EnableRendererColliderCanvas(m_MidAirAugmentation, false);

    }

    // Update is called once per frame
    void Update()
    {
        if (!VuforiaRuntimeUtilities.IsPlayMode() && !AnchorExists)
        {
            AnchorExists = DoAnchorsExist();
        }

    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");

        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.UnregisterTrackerStartedCallback(OnTrackerStarted);
        DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    public void PlaceObjectInMidAir(Transform midAirTransform)
    {
        Debug.Log("PlaceObjectInMidAir() called.");

        m_ContentPositioningBehaviour.AnchorStage = m_MidAirAnchor;
        Debug.Log("1. m_ContentPositioningBehaviour.AnchorStage OR m_MidAirArchor is not null -> proceed");


        m_ContentPositioningBehaviour.PositionContentAtMidAirAnchor(midAirTransform);
        Debug.Log("2. m_ContentPositioningBehaviour.PositionContentAtMidAirAnchor(midAirTransform) -> proceed");

        UtilityHelper.EnableRendererColliderCanvas(m_MidAirAugmentation, true);
        Debug.Log("3. UtilityHelper.EnableRendererColliderCanvas(m_MidAirAugmentation, true); -> proceed");

        m_MidAirAugmentation.transform.localPosition = Vector3.zero;
        Debug.Log("4. m_MidAirAugmentation.transform.localPosition = Vector3.zero; -> proceed");

        UtilityHelper.RotateTowardCamera(m_MidAirAugmentation);
        Debug.Log("5. UtilityHelper.RotateTowardCamera(m_MidAirAugmentation); -> out");
    }

    public void ResetScene()
    {
        Debug.Log("ResetScene() called.");

        m_MidAirAugmentation.transform.position = Vector3.zero;
        m_MidAirAugmentation.transform.localEulerAngles = Vector3.zero;
        UtilityHelper.EnableRendererColliderCanvas(m_MidAirAugmentation, false);

        DeleteAnchors();
    }

    public void ResetTrackers()
    {
        Debug.Log("ResetTrackers() called.");

        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();
        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        // Stop and restart trackers
        m_SmartTerrain.Stop(); // stop SmartTerrain tracker before PositionalDeviceTracker
        m_PositionalDeviceTracker.Reset();
        m_SmartTerrain.Start(); // start SmartTerrain tracker after PositionalDeviceTracker
    }

    void DeleteAnchors()
    {
        m_MidAirAnchor.UnConfigureAnchor();
        AnchorExists = DoAnchorsExist();
    }

    bool DoAnchorsExist()
    {
        if (m_StateManager != null)
        {
            IEnumerable<TrackableBehaviour> trackableBehaviours = m_StateManager.GetActiveTrackableBehaviours();

            foreach (TrackableBehaviour behaviour in trackableBehaviours)
            {
                if (behaviour is AnchorBehaviour)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void OnVuforiaStarted() {
        Debug.Log("OnVuforiaStarted() called.");

        m_StateManager = TrackerManager.Instance.GetStateManager();
        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();
        if (m_PositionalDeviceTracker != null && m_SmartTerrain != null)
        {
            if (!m_PositionalDeviceTracker.IsActive)
                m_PositionalDeviceTracker.Start();
            if (m_PositionalDeviceTracker.IsActive && !m_SmartTerrain.IsActive)
                m_SmartTerrain.Start();
        }
        else
        {
            if (m_PositionalDeviceTracker == null)
                Debug.Log("PositionalDeviceTracker returned null. GroundPlane not supported on this device.");
            if (m_SmartTerrain == null)
                Debug.Log("SmartTerrain returned null. GroundPlane not supported on this device.");
        }
    }

    void OnVuforiaPaused(bool paused)
    {
        Debug.Log("OnVuforiaPaused(" + paused.ToString() + ") called.");

        if (paused) {
            //ResetScene();
        }
    }

    void OnTrackerStarted()
    {
        Debug.Log("OnTrackerStarted() called.");

        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();

        if (m_PositionalDeviceTracker != null)
        {
            if (!m_PositionalDeviceTracker.IsActive)
                m_PositionalDeviceTracker.Start();

            Debug.Log("PositionalDeviceTracker is Active?: " + m_PositionalDeviceTracker.IsActive +
                      "\nSmartTerrain Tracker is Active?: " + m_SmartTerrain.IsActive);
        }
    }

    void OnDevicePoseStatusChanged(TrackableBehaviour.Status status, TrackableBehaviour.StatusInfo statusInfo)
    {
        Debug.Log("OnDevicePoseStatusChanged(" + status + ", " + statusInfo + ")");
    }

}
