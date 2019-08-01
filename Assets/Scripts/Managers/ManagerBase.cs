using UnityEngine;

// Base singleton class for all managers
public abstract class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T>
{
	public static T Instance { get; private set; }

	private bool isQuitting = false;

	protected virtual void Awake()
	{
		if( Instance == null )
			Instance = (T) this;
		else if( this != Instance )
			Destroy( this );
	}

	protected void OnApplicationQuit()
	{
		isQuitting = true;
	}

	protected void OnDestroy()
	{
		if( !isQuitting )
			ReleaseResources();
	}

	protected virtual void ReleaseResources()
	{ }
}