using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
	public class MovePieceAnimation
	{
		private HexagonPiece piece;
		private Vector2 initialPosition;
		private Vector2 targetPosition;

		private float t;
		private float tMultiplier;

		public void Initialize( HexagonPiece piece, float animationSpeed )
		{
			this.piece = piece;
			initialPosition = piece.transform.localPosition;
			targetPosition = GridManager.Instance[piece.X].CalculatePositionAt( piece.Y );

			tMultiplier = animationSpeed / Vector2.Distance( targetPosition, initialPosition );
			t = 0f;
		}

		public bool Execute( float deltaTime )
		{
			t += deltaTime * tMultiplier;
			if( t < 1f )
			{
				piece.transform.localPosition = Vector2.LerpUnclamped( initialPosition, targetPosition, t );
				return true;
			}

			piece.transform.localPosition = targetPosition;
			return false;
		}
	}

	public class BlowPieceAnimation
	{
		private HexagonPiece piece;
		private Vector2 velocity;

		private float t;
		private float tMultiplier;

		public void Initialize( HexagonPiece piece, float animationSpeed )
		{
			this.piece = piece;
			velocity = ( Random.insideUnitCircle + new Vector2( 0f, 0.15f ) ) * ( GridManager.Instance.Width * 1.2f ) * GridManager.PIECE_WIDTH;

			t = 0f;
			tMultiplier = animationSpeed;

			piece.SortingOrder = 2;
		}

		public bool Execute( float deltaTime )
		{
			t += deltaTime * tMultiplier;
			velocity.y -= deltaTime * 10f;
			if( t < 1f )
			{
				float scale = ( 1f - t ) * GridManager.PIECE_WIDTH;

				piece.transform.Translate( velocity * deltaTime );
				piece.transform.localScale = new Vector3( scale, scale, scale );

				return true;
			}

			piece.transform.localScale = new Vector3( GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH, GridManager.PIECE_WIDTH );
			piece.SortingOrder = 0;
			PoolManager.Instance.Push( piece );

			return false;
		}
	}

#pragma warning disable 0649
	[SerializeField]
	private float moveAnimationSpeed = 7.5f;

	[SerializeField]
	private float blowAnimationSpeed = 2f;

	[SerializeField]
	private float selectionRotateSpeed = 700f;
#pragma warning restore 0649

	public bool IsAnimating { get { return moveAnimations.Count > 0; } }

	private readonly List<MovePieceAnimation> moveAnimations = new List<MovePieceAnimation>( 64 );
	private readonly List<BlowPieceAnimation> blowAnimations = new List<BlowPieceAnimation>( 8 );

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if( deltaTime <= 0f )
			return;

		for( int i = moveAnimations.Count - 1; i >= 0; i-- )
		{
			if( !moveAnimations[i].Execute( deltaTime ) )
			{
				PoolManager.Instance.Push( moveAnimations[i] );

				// This move operation has finished, move the last move operation to this index
				if( i < moveAnimations.Count - 1 )
					moveAnimations[i] = moveAnimations[moveAnimations.Count - 1];

				moveAnimations.RemoveAt( moveAnimations.Count - 1 );
			}
		}

		for( int i = blowAnimations.Count - 1; i >= 0; i-- )
		{
			if( !blowAnimations[i].Execute( deltaTime ) )
			{
				PoolManager.Instance.Push( blowAnimations[i] );

				// This blow operation has finished, move the last blow operation to this index
				if( i < blowAnimations.Count - 1 )
					blowAnimations[i] = blowAnimations[blowAnimations.Count - 1];

				blowAnimations.RemoveAt( blowAnimations.Count - 1 );
			}
		}
	}

	public void MovePieceToPosition( HexagonPiece piece )
	{
		MovePieceAnimation moveAnimation = PoolManager.Instance.PopMoveAnimation();
		moveAnimation.Initialize( piece, moveAnimationSpeed );
		moveAnimations.Add( moveAnimation );
	}

	public void BlowPieceAway( HexagonPiece piece )
	{
		BlowPieceAnimation blowAnimation = PoolManager.Instance.PopBlowAnimation();
		blowAnimation.Initialize( piece, blowAnimationSpeed );
		blowAnimations.Add( blowAnimation );
	}

	public IEnumerator RotateSelection( TupleSelection selection, float degrees )
	{
		Quaternion currentAngles = Quaternion.Euler( selection.transform.localEulerAngles );
		Vector3 initialRotation = new Vector3();
		Vector3 targetRotation = new Vector3( 0f, 0f, degrees );

		HexagonPiece piece1 = selection.Tuple.piece1;
		HexagonPiece piece2 = selection.Tuple.piece2;
		HexagonPiece piece3 = selection.Tuple.piece3;

		Vector3 selectionCenter = selection.transform.localPosition;
		Vector3 dir1 = piece1.transform.localPosition - selectionCenter;
		Vector3 dir2 = piece2.transform.localPosition - selectionCenter;
		Vector3 dir3 = piece3.transform.localPosition - selectionCenter;

		float t = 0f;
		float tMultiplier = selectionRotateSpeed / Mathf.Abs( degrees );
		while( ( t = t + Time.deltaTime * tMultiplier ) < 1f )
		{
			Quaternion rotation = Quaternion.Euler( Vector3.LerpUnclamped( initialRotation, targetRotation, t ) );

			selection.transform.localRotation = currentAngles * rotation;
			piece1.transform.localPosition = selectionCenter + rotation * dir1;
			piece2.transform.localPosition = selectionCenter + rotation * dir2;
			piece3.transform.localPosition = selectionCenter + rotation * dir3;

			yield return null;
		}

		selection.transform.localRotation = currentAngles * Quaternion.Euler( targetRotation );
		piece1.transform.localPosition = GridManager.Instance[piece1.X].CalculatePositionAt( piece1.Y );
		piece2.transform.localPosition = GridManager.Instance[piece2.X].CalculatePositionAt( piece2.Y );
		piece3.transform.localPosition = GridManager.Instance[piece3.X].CalculatePositionAt( piece3.Y );
	}
}