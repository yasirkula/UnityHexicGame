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
	private int score = 0;

	protected override void Awake()
	{
		base.Awake();
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

		int rotationAmount;
		HexagonMatch match = null;
		for( rotationAmount = 1; rotationAmount < 3; rotationAmount++ )
		{
			selection.Tuple.RotateClockwise( clockwise ? 1 : -1 );
			match = GridManager.Instance.GetMatchingPiecesAt( selection.Tuple );
			if( match != null )
				break;
		}

		if( match == null )
			selection.Tuple.RotateClockwise( clockwise ? 1 : -1 );
		
		StartCoroutine( RotateTuple( clockwise, rotationAmount, match ) );
	}

	private IEnumerator RotateTuple( bool clockwise, int amount, HexagonMatch match )
	{
		isBusy = true;
		yield return StartCoroutine( AnimationManager.Instance.RotateSelection( selection, amount * ( clockwise ? -120f : 120f ) ) );

		if( match != null )
		{
			selection.IsVisible = false;

			yield return new WaitForSeconds( 0.5f );
			ProcessMatch( match );

			yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );

			List<HexagonMatch> matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			while( matchesOnGrid != null && matchesOnGrid.Count > 0 )
			{
				yield return new WaitForSeconds( 0.5f );
				for( int i = 0; i < matchesOnGrid.Count; i++ )
					ProcessMatch( matchesOnGrid[i] );

				yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );
				matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			}

			selection.SelectTupleAt( selection.transform.localPosition );
		}

		if( GridManager.Instance.CheckDeadlock() )
			GameOver();
		else
			isBusy = false;
	}

	private void ProcessMatch( HexagonMatch match )
	{
		score += 5 * match.Count;
		
		UIManager.Instance.UpdateScore( score );
		GridManager.Instance.DestroyMatchingPieces( match );
		PoolManager.Instance.Push( match );
	}

	private void GameOver()
	{
		isBusy = true;
		
		int highscore = PlayerPrefs.GetInt( "Highscore", 0 );
		if( score > highscore )
		{
			highscore = score;

			PlayerPrefs.SetInt( "Highscore", score );
			PlayerPrefs.Save();
		}

		UIManager.Instance.GameOver( score, highscore );
	}
}