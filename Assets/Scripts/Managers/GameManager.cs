using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Handles the progression of the game and the events that take place during gameplay
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

	// Active bombs on the grid
	private readonly List<HexagonBomb> bombs = new List<HexagonBomb>( 2 );

	// Is waiting for the grid to update itself?
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

		// Check if this is a clockwise swipe or a counter-clockwise swipe
		Vector2 center = CameraManager.Instance.WorldToScreenPoint( selection.transform.localPosition );
		bool clockwise = Vector2.SignedAngle( eventData.pressPosition - center, eventData.position - center ) < 0f;

		// Check if rotating the selection by a certain amount results in a match on the grid
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
			selection.Tuple.RotateClockwise( clockwise ? 1 : -1 ); // So that the selection will rotate 360 degrees

		StartCoroutine( RotateSelection( clockwise, rotationAmount, match ) );
	}

	private IEnumerator RotateSelection( bool clockwise, int amount, HexagonMatch match )
	{
		isBusy = true;

		// Wait for the rotate animation to finish
		yield return StartCoroutine( AnimationManager.Instance.RotateSelection( selection, amount * ( clockwise ? -120f : 120f ) ) );

		// The grid will be updated if there is a match
		if( match != null )
		{
			// Don't show the selection while updating the grid
			selection.IsVisible = false;

			// Wait for a short interval so that users can also realize the match
			yield return new WaitForSeconds( 0.5f );

			// A column index that is selected randomly from the matching pieces
			int possibleBombColumn = match[Random.Range( 0, match.Count )].X;

			// Update the score and etc.
			ProcessMatch( match );

			// Start filling in the blank slots (slots previously occupied by matching pieces) but don't wait for it to finish yet
			Coroutine fillBlanksCoroutine = StartCoroutine( GridManager.Instance.FillBlankSlots() );
			if( score >= nextBombSpawnScore )
			{
				// Spawn a bomb at a random column if we've reached the target score
				nextBombSpawnScore += bombInterval;

				HexagonBomb bomb = PoolManager.Instance.PopBomb();
				bomb.Initialize( GridManager.Instance[possibleBombColumn][GridManager.Instance.Height - 1], bombExplosionCounter + 1 ); // Counter will decrement after this round
				bombs.Add( bomb );
			}

			// Wait for the blank slots to be filled
			yield return fillBlanksCoroutine;

			// Check if there are another matches on the grid after the blank slots are filled
			// If so, continue to update the grid until there is no match left
			List<HexagonMatch> matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			while( matchesOnGrid != null && matchesOnGrid.Count > 0 )
			{
				yield return new WaitForSeconds( 0.5f );
				for( int i = 0; i < matchesOnGrid.Count; i++ )
					ProcessMatch( matchesOnGrid[i] );

				yield return StartCoroutine( GridManager.Instance.FillBlankSlots() );
				matchesOnGrid = GridManager.Instance.GetAllMatchingPiecesOnGrid();
			}

			// Decrement the counters of the bombs and end the game if a bomb reaches 0
			for( int i = bombs.Count - 1; i >= 0; i-- )
			{
				if( bombs[i].Tick() )
				{
					GameOver();
					yield break;
				}
			}

			// Update the selection with the new pieces
			selection.SelectTupleAt( selection.transform.localPosition );

			// Check if there are no more possible matches on the grid (i.e. deadlock)
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
			// Destroy the matching pieces in a fashionable way
			GridManager.Instance[match[i].X][match[i].Y] = null; // Mark that slot as empty
			AnimationManager.Instance.BlowPieceAway( match[i] );

			// Check if a bomb was attached to the destroyed piece
			for( int j = bombs.Count - 1; j >= 0; j-- )
			{
				if( bombs[j].AttachedPiece == match[i] )
				{
					PoolManager.Instance.Push( bombs[j] );

					// This bomb is now defused, move the last bomb to this index
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

		// Show the game over screen
		UIManager.Instance.GameOver( score, highscore );
	}
}