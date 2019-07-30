using UnityEngine;

public abstract class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T>
{
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if( Instance == null )
			Instance = (T) this;
		else if( this != Instance )
			Destroy( this );
	}
}