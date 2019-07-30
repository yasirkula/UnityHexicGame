using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public static CameraManager Instance { get; private set; }

#pragma warning disable 0649
	[SerializeField]
	private float cameraPadding = 0.05f;
#pragma warning restore 0649

	private Camera _camera;
	private Vector2 gridExtents;

	private void Awake()
	{
		Instance = this;
		_camera = GetComponent<Camera>();
	}

	public void SetGridBounds( Bounds bounds )
	{
		transform.localPosition = bounds.center;

		gridExtents = bounds.extents * ( 1f + cameraPadding );
		RecalculateOrthographicSize();
	}

	private void RecalculateOrthographicSize()
	{
		float screenAspect = _camera.aspect;
		float levelAspect = gridExtents.x / gridExtents.y;

		if( screenAspect >= levelAspect )
			_camera.orthographicSize = gridExtents.y;
		else
			_camera.orthographicSize = gridExtents.x / screenAspect;
	}
}