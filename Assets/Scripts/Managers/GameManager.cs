using System.Collections;
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

	private HexagonTuple selectedTuple;

	protected override void Awake()
	{
		gridSelection = Instantiate( gridSelection );
		gridSelection.gameObject.SetActive( false );
	}

	private void Start()
	{
		inputReceiver.ClickEvent += OnClick;
		inputReceiver.SwipeEvent += OnSwipe;
	}

	private void OnClick( PointerEventData eventData )
	{
		if( gridSelection.gameObject.activeSelf )
			selectedTuple.SetSelected( false );

		Vector2 point = CameraManager.Instance.ScreenToWorldPoint( eventData.position );
		if( GridManager.Instance.TryGetPiecesAt( point, out selectedTuple ) )
		{
			Vector3 positionAverage = ( selectedTuple.piece1.transform.localPosition + selectedTuple.piece2.transform.localPosition + selectedTuple.piece3.transform.localPosition ) / 3f;

			// There are always 2 vertical pieces in the selection, need to find whether the third piece is located at the left or at the right
			bool rightOriented;
			float deltaX = selectedTuple.piece1.transform.localPosition.x - positionAverage.x;
			if( Mathf.Abs( deltaX ) < GridManager.HEX_WIDTH * 0.4f ) // piece1 is one of the vertical pieces because it is closer to average position
				rightOriented = deltaX < 0f;
			else // piece1 is the piece that is located at the side
				rightOriented = deltaX > 0f;

			selectedTuple.SetSelected( true );

			gridSelection.transform.localPosition = positionAverage;
			gridSelection.transform.localEulerAngles = new Vector3( 0f, 0f, rightOriented ? 0f : 180f );
			gridSelection.gameObject.SetActive( true );
		}
		else
			gridSelection.gameObject.SetActive( false );
	}

	private void OnSwipe( PointerEventData eventData )
	{
		if( !gridSelection.gameObject.activeSelf )
			return;

		Vector2 center = CameraManager.Instance.WorldToScreenPoint( gridSelection.localPosition );
		bool clockwise = Vector2.SignedAngle( eventData.pressPosition - center, eventData.position - center ) < 0f;

		StartCoroutine( RotateTuple( clockwise ) );
	}

	private IEnumerator RotateTuple( bool clockwise )
	{
		Vector3 currentAngles = gridSelection.localEulerAngles;
		Vector3 targetAngles = currentAngles + new Vector3( 0f, 0f, ( clockwise ? -120f : 120f ) );

		selectedTuple.piece1.transform.SetParent( gridSelection, true );
		selectedTuple.piece2.transform.SetParent( gridSelection, true );
		selectedTuple.piece3.transform.SetParent( gridSelection, true );

		float t = 0f;
		while( t < 1f )
		{
			gridSelection.localEulerAngles = Vector3.LerpUnclamped( currentAngles, targetAngles, t );
			t += Time.deltaTime * 6f;
			yield return null;
		}

		gridSelection.localEulerAngles = targetAngles;

		selectedTuple.piece1.transform.SetParent( null, true );
		selectedTuple.piece2.transform.SetParent( null, true );
		selectedTuple.piece3.transform.SetParent( null, true );
	}
}