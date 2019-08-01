// A tuple consists of three adjacent hexagon pieces
public struct HexagonTuple
{
	public readonly HexagonPiece piece1, piece2, piece3;

	public bool IsEmpty { get { return piece1 == null || piece2 == null || piece3 == null; } }
	public bool IsMatching { get { return piece1.ColorIndex == piece2.ColorIndex && piece2.ColorIndex == piece3.ColorIndex; } }

	public HexagonTuple( HexagonPiece piece1, HexagonPiece piece2, HexagonPiece piece3 )
	{
		this.piece1 = piece1;
		this.piece2 = piece2;
		this.piece3 = piece3;
	}

	// Selected pieces are drawn above the others so that they don't fall behind the other pieces while being rotated
	public void SetSelected( bool isSelected )
	{
		piece1.SortingOrder = isSelected ? 1 : 0;
		piece2.SortingOrder = isSelected ? 1 : 0;
		piece3.SortingOrder = isSelected ? 1 : 0;
	}

	// Changes the order of the pieces in the tuple, this doesn't actually change their transform.position since it is
	// often desirable to rotate the tuple for certain calculations (e.g. to check if there will be a match at certain rotation)
	// without actually modifying their transform.position values. The transform.position values are usually animated to
	// reach the target position (e.g. while rotation the selection or adding new pieces to the grid from above)
	public void RotateClockwise( int count = 1 )
	{
		count = count % 3;
		if( count < 0 )
			count += 3;
		else if( count == 0 )
			return;

		int x = piece1.X;
		int y = piece1.Y;

		if( count == 1 )
		{
			GridManager.Instance[piece2.X][piece2.Y] = piece1;
			GridManager.Instance[piece3.X][piece3.Y] = piece2;
			GridManager.Instance[x][y] = piece3;
		}
		else
		{
			GridManager.Instance[piece3.X][piece3.Y] = piece1;
			GridManager.Instance[piece2.X][piece2.Y] = piece3;
			GridManager.Instance[x][y] = piece2;
		}
	}

	public void RotateCounterClockwise( int count = 1 )
	{
		RotateClockwise( -count );
	}
}