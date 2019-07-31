using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : ManagerBase<GameManager>
{
#pragma warning disable 0649
	[SerializeField]
	private InputReceiver inputReceiver;

	[SerializeField]
	private TupleSelection selection;
#pragma warning restore 0649

	private bool isBusy = false;

	protected override void Awake()
	{
		selection = Instantiate( selection );
	}

	private void Start()
	{
		inputReceiver.ClickEvent += OnClick;
		inputReceiver.SwipeEvent += OnSwipe;
	}

	private void OnClick( PointerEventData eventData )
	{
		if( isBusy )
			return;

		selection.SelectTupleAt( CameraManager.Instance.ScreenToWorldPoint( eventData.position ) );
	}

	private void OnSwipe( PointerEventData eventData )
	{
		if( isBusy )
			return;

		if( !selection.IsVisible )
			return;

		Vector2 center = CameraManager.Instance.WorldToScreenPoint( selection.transform.localPosition );
		bool clockwise = Vector2.SignedAngle( eventData.pressPosition - center, eventData.position - center ) < 0f;
		int rotationAmount = 0;

		HexagonMatch match = null;
		do
		{
			if( clockwise )
				selection.Tuple.RotateClockwise();
			else
				selection.Tuple.RotateCounterClockwise();

			if( rotationAmount < 2 )
				match = GridManager.Instance.GetMatchingPiecesAt( selection.Tuple );
		} while( ++rotationAmount < 3 && ( match == null || match.Count == 0 ) );

		StartCoroutine( RotateTuple( clockwise, rotationAmount, match ) );
	}

	private IEnumerator RotateTuple( bool clockwise, int amount, HexagonMatch match )
	{
		isBusy = true;
		yield return StartCoroutine( AnimationManager.Instance.RotateSelection( selection, amount * ( clockwise ? -120f : 120f ) ) );

		if( match != null && match.Count > 0 )
		{
			selection.IsVisible = false;

			GridManager.Instance.DestroyMatchingPieces( match );
			PoolManager.Instance.PushMatch( match );

			yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );

			List<HexagonMatch> matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			while( matchesOnGrid != null && matchesOnGrid.Count > 0 )
			{
				for( int i = 0; i < matchesOnGrid.Count; i++ )
				{
					GridManager.Instance.DestroyMatchingPieces( matchesOnGrid[i] );
					PoolManager.Instance.PushMatch( matchesOnGrid[i] );
				}

				yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );

				matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			}

			selection.SelectTupleAt( selection.transform.localPosition );
		}

		isBusy = false;
	}
}