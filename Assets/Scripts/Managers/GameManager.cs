using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : ManagerBase<GameManager>
{
#pragma warning disable 0649
	[SerializeField]
	private InputReceiver inputReceiver;

	[SerializeField]
	private Transform gridSelection;
#pragma warning restore 0649

	protected override void Awake()
	{
		gridSelection = Instantiate( gridSelection );
		gridSelection.gameObject.SetActive( false );
	}

	private void Start()
	{
		inputReceiver.ClickEvent += OnClick;
	}

	private void OnClick( PointerEventData eventData )
	{
		Vector2 point = CameraManager.Instance.ScreenToWorldPoint( eventData.position );
		if( GridManager.Instance.TryGetPiecesAt( point, out HexagonTuple t ) )
		{
			Vector3 positionAverage = ( t.piece1.transform.localPosition + t.piece2.transform.localPosition + t.piece3.transform.localPosition ) / 3f;

			// There are always 2 vertical pieces in the selection, need to find whether the third piece is located at the left or at the right
			bool rightOriented;
			float deltaX = t.piece1.transform.localPosition.x - positionAverage.x;
			if( Mathf.Abs( deltaX ) < GridManager.HEX_WIDTH * 0.4f ) // piece1 is one of the vertical pieces because it is closer to average position
				rightOriented = deltaX < 0f;
			else // piece1 is the piece that is located at the side
				rightOriented = deltaX > 0f;

			gridSelection.transform.localPosition = positionAverage;
			gridSelection.transform.localEulerAngles = new Vector3( 0f, 0f, rightOriented ? 0f : 180f );
			gridSelection.gameObject.SetActive( true );
		}
		else
			gridSelection.gameObject.SetActive( false );
	}
}