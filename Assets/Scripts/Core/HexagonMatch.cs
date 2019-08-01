using System.Collections.Generic;

// A match on the grid, it can consist of any number of hexagon pieces but these pieces are
// guaranteed to be adjacent to each other and have the same color
public class HexagonMatch
{
	private readonly List<HexagonPiece> matchingPieces = new List<HexagonPiece>( 6 );

	public int Count { get { return matchingPieces.Count; } }
	public HexagonPiece this[int index] { get { return matchingPieces[index]; } }

	public void Add( HexagonPiece piece )
	{
		matchingPieces.Add( piece );
	}

	public void Add( HexagonTuple tuple )
	{
		matchingPieces.Add( tuple.piece1 );
		matchingPieces.Add( tuple.piece2 );
		matchingPieces.Add( tuple.piece3 );
	}

	public void Clear()
	{
		matchingPieces.Clear();
	}
}