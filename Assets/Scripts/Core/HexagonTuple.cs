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

	public void SetSelected( bool isSelected )
	{
		piece1.SortingOrder = isSelected ? 1 : 0;
		piece2.SortingOrder = isSelected ? 1 : 0;
		piece3.SortingOrder = isSelected ? 1 : 0;
	}

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