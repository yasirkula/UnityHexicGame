using UnityEngine;

public class CameraManager : ManagerBase<CameraManager>
{
#pragma warning disable 0649
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private float cameraPadding = 0.05f;
#pragma warning restore 0649

	private Vector2? gridExtents;

	public float YTop { get; private set; }

	private void Start()
	{
		YTop = _camera.ViewportToWorldPoint( new Vector3( 0f, 1.1f, 0f ) ).y;
	}

	public void SetGridBounds( Bounds bounds )
	{
		Vector3 position = bounds.center;
		position.z = -10f;
		_camera.transform.localPosition = position;

		gridExtents = bounds.extents * ( 1f + cameraPadding );
		RecalculateOrthographicSize();
	}

	public Vector2 ScreenToWorldPoint( Vector2 screenPoint )
	{
		return _camera.ScreenToWorldPoint( screenPoint );
	}

	public Vector2 WorldToScreenPoint( Vector3 worldPoint )
	{
		return _camera.WorldToScreenPoint( worldPoint );
	}

	private void RecalculateOrthographicSize()
	{
		if( gridExtents == null )
			return;

		float width = gridExtents.Value.x;
		float height = gridExtents.Value.y;

		float screenAspect = _camera.aspect;
		float levelAspect = width / height;

		if( screenAspect >= levelAspect )
			_camera.orthographicSize = height;
		else
			_camera.orthographicSize = width / screenAspect;
	}
}