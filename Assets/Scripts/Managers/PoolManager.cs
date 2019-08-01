using UnityEngine;
using UnityEngine.SceneManagement;

// Handles pooling of commonly used objects for memory efficiency and reduced GC
public class PoolManager : ManagerBase<PoolManager>
{
#pragma warning disable 0649
	[SerializeField]
	private HexagonPiece piecePrefab;

	[SerializeField]
	private HexagonBomb bombPrefab;
#pragma warning restore 0649

	private readonly SimplePool<HexagonPiece> piecePool = new SimplePool<HexagonPiece>();
	private readonly SimplePool<HexagonBomb> bombPool = new SimplePool<HexagonBomb>();
	private readonly SimplePool<HexagonMatch> matchPool = new SimplePool<HexagonMatch>();
	private readonly SimplePool<AnimationManager.MovePieceAnimation> moveAnimationsPool = new SimplePool<AnimationManager.MovePieceAnimation>();
	private readonly SimplePool<AnimationManager.BlowPieceAnimation> blowAnimationsPool = new SimplePool<AnimationManager.BlowPieceAnimation>();

	protected override void Awake()
	{
		base.Awake();

		// Initialize the pools
		if( Instance == this )
		{
			// This manager should persist after level restarts
			transform.SetParent( null, false );
			DontDestroyOnLoad( gameObject );

			piecePool.CreateFunction = () =>
			{
				HexagonPiece result = Instantiate( piecePrefab );
				SceneManager.MoveGameObjectToScene( result.gameObject, gameObject.scene ); // So that the piece will be DontDestroyOnLoad'ed
				result.transform.localScale = new Vector3( GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH );

				return result;
			};
			piecePool.OnPop = ( piece ) => piece.gameObject.SetActive( true );
			piecePool.OnPush = ( piece ) => piece.gameObject.SetActive( false );
			piecePool.Populate( 32 );

			bombPool.CreateFunction = () =>
			{
				HexagonBomb result = Instantiate( bombPrefab );
				SceneManager.MoveGameObjectToScene( result.gameObject, gameObject.scene ); // So that the bomb will be DontDestroyOnLoad'ed
				result.transform.localScale = new Vector3( GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH );

				return result;
			};
			bombPool.OnPop = ( bomb ) => bomb.gameObject.SetActive( true );
			bombPool.OnPush = ( bomb ) =>
			{
				bomb.gameObject.SetActive( false );
				bomb.transform.SetParent( null, false ); // Bombs are attached to their target hexagon pieces, release the bomb
			};
			bombPool.Populate( 2 );

			matchPool.CreateFunction = () => new HexagonMatch();
			matchPool.OnPush = ( match ) => match.Clear();
			matchPool.Populate( 8 );

			moveAnimationsPool.CreateFunction = () => new AnimationManager.MovePieceAnimation();
			moveAnimationsPool.Populate( 64 );

			blowAnimationsPool.CreateFunction = () => new AnimationManager.BlowPieceAnimation();
			blowAnimationsPool.Populate( 8 );
		}
	}

	public HexagonPiece PopPiece()
	{
		return piecePool.Pop();
	}

	public HexagonBomb PopBomb()
	{
		return bombPool.Pop();
	}

	public HexagonMatch PopMatch()
	{
		return matchPool.Pop();
	}

	public AnimationManager.MovePieceAnimation PopMoveAnimation()
	{
		return moveAnimationsPool.Pop();
	}

	public AnimationManager.BlowPieceAnimation PopBlowAnimation()
	{
		return blowAnimationsPool.Pop();
	}

	public void Push( HexagonPiece piece )
	{
		piecePool.Push( piece );
	}

	public void Push( HexagonBomb bomb )
	{
		bombPool.Push( bomb );
	}

	public void Push( HexagonMatch match )
	{
		matchPool.Push( match );
	}

	public void Push( AnimationManager.MovePieceAnimation moveAnimation )
	{
		moveAnimationsPool.Push( moveAnimation );
	}

	public void Push( AnimationManager.BlowPieceAnimation blowAnimation )
	{
		blowAnimationsPool.Push( blowAnimation );
	}
}