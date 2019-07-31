using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
	public class MoveAnimation
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

#pragma warning disable 0649
	[SerializeField]
	private float animationSpeed = 10f;

	[SerializeField]
	private float selectionRotateSpeed = 150f;
#pragma warning restore 0649

	public bool IsAnimating { get { return moveAnimations.Count > 0; } }

	private readonly List<MoveAnimation> moveAnimations = new List<MoveAnimation>( 64 );

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if( deltaTime <= 0f )
			return;

		for( int i = moveAnimations.Count - 1; i >= 0; i-- )
		{
			MoveAnimation moveOperation = moveAnimations[i];
			if( !moveAnimations[i].Execute( deltaTime ) )
			{
				PoolManager.Instance.PushMoveAnimation( moveAnimations[i] );

				// This move operation has finished, move the last move operation to this index
				if( i < moveAnimations.Count - 1 )
					moveAnimations[i] = moveAnimations[moveAnimations.Count - 1];

				moveAnimations.RemoveAt( moveAnimations.Count - 1 );
			}
		}
	}

	public void AnimatePiece( HexagonPiece piece )
	{
		MoveAnimation moveAnimation = PoolManager.Instance.PopMoveAnimation();
		moveAnimation.Initialize( piece, animationSpeed );
		moveAnimations.Add( moveAnimation );
	}

	public IEnumerator RotateSelection( TupleSelection selection, float degrees )
	{
		Quaternion currentAngles = Quaternion.Euler( selection.transform.localEulerAngles );
		Vector3 initialRotation = new Vector3();
		Vector3 targetRotation = new Vector3( 0f, 0f, degrees );

		Vector3 selectionCenter = selection.transform.localPosition;
		Vector3 dir1 = selection.Tuple.piece1.transform.localPosition - selectionCenter;
		Vector3 dir2 = selection.Tuple.piece2.transform.localPosition - selectionCenter;
		Vector3 dir3 = selection.Tuple.piece3.transform.localPosition - selectionCenter;

		float t = 0f;
		float tMultiplier = selectionRotateSpeed / Mathf.Abs( degrees );
		while( true )
		{
			t += Time.deltaTime * tMultiplier;
			if( t > 1f )
				t = 1f;

			Quaternion rotation = Quaternion.Euler( Vector3.LerpUnclamped( initialRotation, targetRotation, t ) );

			selection.transform.localRotation = currentAngles * rotation;
			selection.Tuple.piece1.transform.localPosition = selectionCenter + rotation * dir1;
			selection.Tuple.piece2.transform.localPosition = selectionCenter + rotation * dir2;
			selection.Tuple.piece3.transform.localPosition = selectionCenter + rotation * dir3;

			if( t >= 1f )
				yield break;

			yield return null;
		}
	}
}