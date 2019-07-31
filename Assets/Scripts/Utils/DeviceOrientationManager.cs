using UnityEngine;

// Taken from: https://github.com/yasirkula/UnityGenericPool/blob/master/SimplePool.cs
public class DeviceOrientationManager : MonoBehaviour
{
	private const float ORIENTATION_CHECK_INTERVAL = 0.5f;

	private static ScreenOrientation currentOrientation;
	private float nextOrientationCheckTime;

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
		currentOrientation = Screen.orientation;
		nextOrientationCheckTime = Time.realtimeSinceStartup + 1f;
	}

	private void Update()
	{
		if( Time.realtimeSinceStartup >= nextOrientationCheckTime )
		{
			ScreenOrientation orientation = Screen.orientation;
			if( currentOrientation != orientation )
			{
				currentOrientation = orientation;

				if( OnScreenOrientationChanged != null )
					OnScreenOrientationChanged( orientation );
			}

			nextOrientationCheckTime = Time.realtimeSinceStartup + ORIENTATION_CHECK_INTERVAL;
		}
	}
}