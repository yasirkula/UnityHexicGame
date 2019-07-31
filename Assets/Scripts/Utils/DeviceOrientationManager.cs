using UnityEngine;

// Taken from: https://github.com/yasirkula/UnityGenericPool/blob/master/SimplePool.cs
public class DeviceOrientationManager : MonoBehaviour
{
	private const float ORIENTATION_CHECK_INTERVAL = 0.5f;

	private float nextOrientationCheckTime;

	private static ScreenOrientation m_currentOrientation;
	public static ScreenOrientation CurrentOrientation
	{
		get
		{
			return m_currentOrientation;
		}
		private set
		{
			if( m_currentOrientation != value )
			{
				m_currentOrientation = value;
				Screen.orientation = value;

				if( OnScreenOrientationChanged != null )
					OnScreenOrientationChanged( value );
			}
		}
	}

	public static event System.Action<ScreenOrientation> OnScreenOrientationChanged;

#if UNITY_ANDROID || UNITY_IOS
	[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
	private static void Init()
	{
		DontDestroyOnLoad( new GameObject( "DeviceOrientationManager", typeof( DeviceOrientationManager ) ) );
	}
#endif

	private void Awake()
	{
		m_currentOrientation = Screen.orientation;
		nextOrientationCheckTime = Time.realtimeSinceStartup + 1f;
	}

	private void Update()
	{
		if( Time.realtimeSinceStartup >= nextOrientationCheckTime )
		{
			switch( Input.deviceOrientation )
			{
				case DeviceOrientation.LandscapeLeft: CurrentOrientation = ScreenOrientation.LandscapeLeft; break;
				case DeviceOrientation.LandscapeRight: CurrentOrientation = ScreenOrientation.LandscapeRight; break;
				case DeviceOrientation.PortraitUpsideDown: CurrentOrientation = ScreenOrientation.PortraitUpsideDown; break;
				case DeviceOrientation.Portrait: CurrentOrientation = ScreenOrientation.Portrait; break;
			}

			nextOrientationCheckTime = Time.realtimeSinceStartup + ORIENTATION_CHECK_INTERVAL;
		}
	}
}