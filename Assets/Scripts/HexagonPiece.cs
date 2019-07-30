using UnityEngine;

public struct HexagonTuple
{
	public readonly HexagonPiece piece1, piece2, piece3;

	public HexagonTuple( HexagonPiece piece1, HexagonPiece piece2, HexagonPiece piece3 )
	{
		this.piece1 = piece1;
		this.piece2 = piece2;
		this.piece3 = piece3;
	}
}

public enum Corner { BottomLeft, Left, TopLeft, TopRight, Right, BottomRight };

public class HexagonPiece : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private SpriteRenderer spriteRenderer;
#pragma warning restore 0649

	public int ColorIndex { get; private set; }

	public void SetColor( int colorIndex, Color color )
	{
		ColorIndex = colorIndex;
		spriteRenderer.color = color;
	}

	public Corner GetClosestCorner( Vector2 localPoint )
	{
		bool leftSide = localPoint.x < 0f;
		Vector2 p1 = new Vector2( GridManager.HEX_WIDTH * ( leftSide ? -0.25f : 0.25f ), GridManager.HEX_HEIGHT * -0.5f );
		Vector2 p2 = new Vector2( GridManager.HEX_WIDTH * ( leftSide ? -0.5f : 0.5f ), 0f );
		Vector2 p3 = new Vector2( GridManager.HEX_WIDTH * ( leftSide ? -0.25f : 0.25f ), GridManager.HEX_HEIGHT * 0.5f );

		if( ( p1 - localPoint ).sqrMagnitude < ( p2 - localPoint ).sqrMagnitude )
		{
			if( ( p1 - localPoint ).sqrMagnitude < ( p3 - localPoint ).sqrMagnitude )
				return leftSide ? Corner.BottomLeft : Corner.BottomRight;

			return leftSide ? Corner.TopLeft : Corner.TopRight;
		}

		if( ( p2 - localPoint ).sqrMagnitude < ( p3 - localPoint ).sqrMagnitude )
			return leftSide ? Corner.Left : Corner.Right;

		return leftSide ? Corner.TopLeft : Corner.TopRight;
	}

	public Corner GetClosestPickableCorner( Vector2 localPoint, int x, int y )
	{
		Corner corner = GetClosestCorner( localPoint );

		if( x == 0 )
		{
			if( corner == Corner.BottomLeft )
				corner = Corner.BottomRight;
			else if( corner == Corner.Left )
				corner = Corner.Right;
			else if( corner == Corner.TopLeft )
				corner = Corner.TopRight;
		}
		else if( x == GridManager.Instance.Width - 1 )
		{
			if( corner == Corner.BottomRight )
				corner = Corner.BottomLeft;
			else if( corner == Corner.Right )
				corner = Corner.Left;
			else if( corner == Corner.TopRight )
				corner = Corner.TopLeft;
		}

		if( y == 0 )
		{
			if( corner == Corner.BottomLeft )
				corner = Corner.Left;
			else if( corner == Corner.BottomRight )
				corner = Corner.Right;

			if( x % 2 == 0 )
			{
				if( corner == Corner.Left )
					corner = Corner.TopLeft;
				else if( corner == Corner.Right )
					corner = Corner.TopRight;
			}
		}
		else if( y == GridManager.Instance.Height - 1 )
		{
			if( corner == Corner.TopLeft )
				corner = Corner.Left;
			else if( corner == Corner.TopRight )
				corner = Corner.Right;

			if( x % 2 == 1 )
			{
				if( corner == Corner.Left )
					corner = Corner.BottomLeft;
				else if( corner == Corner.Right )
					corner = Corner.BottomRight;
			}
		}

		return corner;
	}
}