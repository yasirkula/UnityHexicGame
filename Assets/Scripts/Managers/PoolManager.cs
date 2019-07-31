using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : ManagerBase<PoolManager>
{
#pragma warning disable 0649
	[SerializeField]
	private HexagonPiece piecePrefab;
#pragma warning restore 0649

	private readonly SimplePool<HexagonPiece> piecePool = new SimplePool<HexagonPiece>();
	private readonly SimplePool<HexagonMatch> matchPool = new SimplePool<HexagonMatch>();
	private readonly SimplePool<AnimationManager.MovePieceAnimation> moveAnimationsPool = new SimplePool<AnimationManager.MovePieceAnimation>();
	private readonly SimplePool<AnimationManager.BlowPieceAnimation> blowAnimationsPool = new SimplePool<AnimationManager.BlowPieceAnimation>();

	protected override void Awake()
	{
		base.Awake();

		if( Instance == this )
		{
			transform.SetParent( null, false );
			DontDestroyOnLoad( gameObject );

			piecePool.CreateFunction = () =>
			{
				HexagonPiece result = Instantiate( piecePrefab );
				SceneManager.MoveGameObjectToScene( result.gameObject, gameObject.scene ); // So that the piece will be DontDestroyOnLoad

				return result;
			};
			piecePool.OnPop = ( piece ) => piece.gameObject.SetActive( true );
			piecePool.OnPush = ( piece ) => piece.gameObject.SetActive( false );
			piecePool.Populate( 32 );

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