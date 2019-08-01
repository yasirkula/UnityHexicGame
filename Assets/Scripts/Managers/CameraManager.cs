using UnityEngine;

// Handles the camera related operations
public class CameraManager : ManagerBase<CameraManager>
{
#pragma warning disable 0649
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private float cameraPadding = 0.05f;
#pragma warning restore 0649

	private Vector2? gridExtents;

	// Pieces added to the grid from above are created at this height
	public float YTop { get; private set; }

	protected override void Awake()
	{
		base.Awake();

		// When screen orientation changes on mobile devices, recalculate the orthographic size
		DeviceOrientationManager.OnScreenOrientationChanged += ScreenOrientationChanged;
	}

	protected override void ReleaseResources()
	{
		DeviceOrientationManager.OnScreenOrientationChanged -= ScreenOrientationChanged;
	}

	// Apply the bounds to the camera so that the camera centers the grid and changes its
	// orthographic size to draw all hexagon pieces on the grid
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

	private void ScreenOrientationChanged( ScreenOrientation orientation )
	{
		RecalculateOrthographicSize();
	}

	// Calculate orthographic size so that all hexagon pieces are drawn and
	// there is a fixed amount of padding at the edges
	private void RecalculateOrthographicSize()
	{
		if( gridExtents == null )
			return;

		float width = gridExtents.Value.x;
		float height = gridExtents.Value.y;

		float screenAspect = Screen.width / (float) Screen.height;
		float levelAspect = width / height;

		if( screenAspect >= levelAspect )
			_camera.orthographicSize = height;
		else
			_camera.orthographicSize = width / screenAspect;

		YTop = _camera.ViewportToWorldPoint( new Vector3( 0f, 1.1f, 0f ) ).y;
	}
}