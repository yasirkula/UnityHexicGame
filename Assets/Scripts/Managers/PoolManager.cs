using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : ManagerBase<PoolManager>
{
#pragma warning disable 0649
	[SerializeField]
	private HexagonPiece piecePrefab;
#pragma warning restore 0649

	private readonly List<HexagonPiece> piecePool = new List<HexagonPiece>( 32 );
	private readonly List<HexagonMatch> matchPool = new List<HexagonMatch>( 8 );
	private readonly List<AnimationManager.MoveAnimation> moveAnimationsPool = new List<AnimationManager.MoveAnimation>( 64 );

	protected override void Awake()
	{
		base.Awake();

		transform.SetParent( null, false );
		DontDestroyOnLoad( gameObject );
	}

	public void PushPiece( HexagonPiece piece )
	{
#if UNITY_EDITOR
		for( int i = 0; i < piecePool.Count; i++ )
		{
			if( piecePool[i] == piece )
				throw new System.Exception( "Object is already in pool!" );
		}
#endif

		piece.gameObject.SetActive( false );
		piecePool.Add( piece );
	}

	public HexagonPiece PopPiece()
	{
		HexagonPiece result;
		if( piecePool.Count > 0 )
		{
			result = piecePool[piecePool.Count - 1];
			result.gameObject.SetActive( true );

			piecePool.RemoveAt( piecePool.Count - 1 );
		}
		else
		{
			result = Instantiate( piecePrefab );
			SceneManager.MoveGameObjectToScene( result.gameObject, gameObject.scene ); // So that the piece will be DontDestroyOnLoad
		}

		return result;
	}

	public void PushMatch( HexagonMatch match )
	{
#if UNITY_EDITOR
		for( int i = 0; i < matchPool.Count; i++ )
		{
			if( matchPool[i] == match )
				throw new System.Exception( "Object is already in pool!" );
		}
#endif

		match.Clear();
		matchPool.Add( match );
	}

	public HexagonMatch PopMatch()
	{
		HexagonMatch result;
		if( matchPool.Count > 0 )
		{
			result = matchPool[matchPool.Count - 1];
			matchPool.RemoveAt( matchPool.Count - 1 );
		}
		else
			result = new HexagonMatch();

		return result;
	}

	public void PushMoveAnimation( AnimationManager.MoveAnimation moveAnimation )
	{
#if UNITY_EDITOR
		for( int i = 0; i < moveAnimationsPool.Count; i++ )
		{
			if( moveAnimationsPool[i] == moveAnimation )
				throw new System.Exception( "Object is already in pool!" );
		}
#endif

		moveAnimationsPool.Add( moveAnimation );
	}

	public AnimationManager.MoveAnimation PopMoveAnimation()
	{
		AnimationManager.MoveAnimation result;
		if( moveAnimationsPool.Count > 0 )
		{
			result = moveAnimationsPool[moveAnimationsPool.Count - 1];
			moveAnimationsPool.RemoveAt( moveAnimationsPool.Count - 1 );
		}
		else
			result = new AnimationManager.MoveAnimation();

		return result;
	}
}