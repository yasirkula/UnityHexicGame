using System.Collections.Generic;

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