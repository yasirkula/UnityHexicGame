using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : ManagerBase<GameManager>
{
#pragma warning disable 0649
	[SerializeField]
	private int scoreMultiplier = 5;

	[SerializeField]
	private int bombExplosionCounter = 8;

	[SerializeField]
	private int bombInterval = 1000;

	[SerializeField]
	private InputReceiver inputReceiver;

	[SerializeField]
	private TupleSelection selection;
#pragma warning restore 0649

	private readonly List<HexagonBomb> bombs = new List<HexagonBomb>( 2 );

	private bool isBusy = false;

	private int score = 0;
	private int nextBombSpawnScore;

	protected override void Awake()
	{
		base.Awake();

		selection = Instantiate( selection );
		selection.transform.localScale = new Vector3( GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH );

		nextBombSpawnScore = bombInterval;

		inputReceiver.ClickEvent += OnClick;
		inputReceiver.SwipeEvent += OnSwipe;
	}

	protected override void ReleaseResources()
	{
		for( int i = bombs.Count - 1; i >= 0; i-- )
			PoolManager.Instance.Push( bombs[i] );

		bombs.Clear();
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

		StartCoroutine( RotateSelection( clockwise, rotationAmount, match ) );
	}

	private IEnumerator RotateSelection( bool clockwise, int amount, HexagonMatch match )
	{
		isBusy = true;
		yield return StartCoroutine( AnimationManager.Instance.RotateSelection( selection, amount * ( clockwise ? -120f : 120f ) ) );

		if( match != null )
		{
			selection.IsVisible = false;

			yield return new WaitForSeconds( 0.5f );

			int possibleBombColumn = match[Random.Range( 0, match.Count )].X;
			ProcessMatch( match );

			Coroutine fillBlanksCoroutine = StartCoroutine( GridManager.Instance.FillBlankSlots() );
			if( score >= nextBombSpawnScore )
			{
				nextBombSpawnScore += bombInterval;

				HexagonBomb bomb = PoolManager.Instance.PopBomb();
				bomb.Initialize( GridManager.Instance[possibleBombColumn][GridManager.Instance.Height - 1], bombExplosionCounter + 1 ); // It will decrement after this round
				bombs.Add( bomb );
			}

			yield return fillBlanksCoroutine;

			List<HexagonMatch> matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			while( matchesOnGrid != null && matchesOnGrid.Count > 0 )
			{
				yield return new WaitForSeconds( 0.5f );
				for( int i = 0; i < matchesOnGrid.Count; i++ )
					ProcessMatch( matchesOnGrid[i] );

				yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );
				matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			}

			for( int i = bombs.Count - 1; i >= 0; i-- )
			{
				if( bombs[i].Tick() )
				{
					GameOver();
					yield break;
				}
			}

			selection.SelectTupleAt( selection.transform.localPosition );

			if( GridManager.Instance.CheckDeadlock() )
			{
				GameOver();
				yield break;
			}
		}

		isBusy = false;
	}

	private void ProcessMatch( HexagonMatch match )
	{
		score += scoreMultiplier * match.Count;
		UIManager.Instance.UpdateScore( score );

		for( int i = match.Count - 1; i >= 0; i-- )
		{
			GridManager.Instance[match[i].X][match[i].Y] = null;
			AnimationManager.Instance.BlowPieceAway( match[i] );

			for( int j = bombs.Count - 1; j >= 0; j-- )
			{
				if( bombs[j].AttachedPiece == match[i] )
				{
					PoolManager.Instance.Push( bombs[j] );

					// This bomb is defused, move the last bomb to this index
					if( j < bombs.Count - 1 )
						bombs[j] = bombs[bombs.Count - 1];

					bombs.RemoveAt( bombs.Count - 1 );
				}
			}
		}

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