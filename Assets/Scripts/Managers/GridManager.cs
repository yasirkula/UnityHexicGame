using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : ManagerBase<GridManager>
{
	public class Column
	{
		private readonly int column;
		private readonly HexagonPiece[] rows;

		private readonly Vector2 bottomCoords;
		public float XCoord { get { return bottomCoords.x; } }

		public HexagonPiece this[int row]
		{
			get { return rows[row]; }
			set
			{
				rows[row] = value;
				if( value != null )
					value.SetPosition( column, row );
			}
		}

		public Column( int column, int size )
		{
			this.column = column;
			rows = new HexagonPiece[size];

			// Offset the starting point by half size of a hexagon piece so that the bottom left point resides at (0,0)
			bottomCoords = new Vector2( PIECE_WIDTH * 0.5f + column * PIECE_DELTA_X, column % 2 == 0 ? PIECE_HEIGHT * 0.5f : PIECE_HEIGHT );
		}

		public Vector3 CalculatePositionAt( int row )
		{
			return new Vector3( bottomCoords.x, bottomCoords.y + row * PIECE_DELTA_Y, 0f );
		}
	}

	public const float PIECE_WIDTH = 1f;
	public const float PIECE_HEIGHT = PIECE_WIDTH * 0.866025f; // sqrt(3)/2
	public const float PIECE_DELTA_X = PIECE_WIDTH * 0.75f;
	public const float PIECE_DELTA_Y = PIECE_HEIGHT;
	public const float PIECES_INTERSECTION_WIDTH = PIECE_WIDTH * 0.25f;

#pragma warning disable 0649
	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private int gridWidth;
	public int Width { get { return gridWidth; } }

	[SerializeField]
	private int gridHeight;
	public int Height { get { return gridHeight; } }
#pragma warning restore 0649

	private readonly List<HexagonMatch> matchesOnGrid = new List<HexagonMatch>( 4 );
	private readonly HashSet<HexagonPiece> matchesOnGridSet = new HashSet<HexagonPiece>();

	private bool isQuitting = false;

	private Column[] grid;
	private Vector2 gridSize;

	public Column this[int column] { get { return grid[column]; } }

	private void Start()
	{
		if( CreateGrid() )
		{
			gridSize = new Vector3( gridWidth * PIECE_WIDTH - ( gridWidth - 1 ) * PIECES_INTERSECTION_WIDTH, gridHeight * PIECE_HEIGHT + PIECE_HEIGHT * 0.5f, 0f );
			CameraManager.Instance.SetGridBounds( new Bounds( gridSize * 0.5f, gridSize ) );
		}
	}

	private void OnApplicationQuit()
	{
		isQuitting = true;
	}

	private void OnDestroy()
	{
		if( !isQuitting )
			DestroyGrid();
	}

	private bool CreateGrid()
	{
		if( gridWidth < 2 || gridHeight < 2 )
		{
			Debug.LogError( "Grid can't be smaller than 2x2!" );
			return false;
		}

		if( colors.Length < 2 )
		{
			Debug.LogError( "There must be at least 2 colors defined!" );
			return false;
		}

		grid = new Column[gridWidth];
		for( int x = 0; x < gridWidth; x++ )
			grid[x] = new Column( x, gridHeight );

		while( true )
		{
			for( int x = 0; x < gridWidth; x++ )
			{
				for( int y = 0; y < gridHeight; y++ )
				{
					HexagonPiece piece = PoolManager.Instance.PopPiece();
					grid[x][y] = piece;

					piece.transform.localPosition = grid[x].CalculatePositionAt( y );
					RandomizePieceColor( piece, true );
				}
			}

			if( !CheckDeadlock() )
				break;

			DestroyGrid();
		}

		return true;
	}

	private void DestroyGrid()
	{
		for( int x = 0; x < gridWidth; x++ )
		{
			for( int y = 0; y < gridHeight; y++ )
				PoolManager.Instance.Push( grid[x][y] );
		}
	}

	private void RandomizePieceColor( HexagonPiece piece, bool ensureNonMatchingColor )
	{
		int colorIndex = Random.Range( 0, colors.Length );
		if( ensureNonMatchingColor )
		{
			// Make sure that assigning this color to the hexagon piece won't cause a match in the grid
			while( CreatedPieceCheckMatch( piece.X, piece.Y, colorIndex ) )
			{
				int newColorIndex;
				do
				{
					newColorIndex = Random.Range( 0, colors.Length );
				} while( newColorIndex == colorIndex );

				colorIndex = newColorIndex;
			}
		}

		piece.SetColor( colorIndex, colors[colorIndex] );
	}

	private bool CreatedPieceCheckMatch( int x, int y, int colorIndex )
	{
		if( x == 0 )
			return false;

		if( x % 2 == 0 )
		{
			if( y > 0 )
			{
				if( ( grid[x][y - 1].ColorIndex == colorIndex && grid[x - 1][y - 1].ColorIndex == colorIndex ) ||
					( grid[x - 1][y - 1].ColorIndex == colorIndex && grid[x - 1][y].ColorIndex == colorIndex ) )
					return true;
			}
		}
		else
		{
			if( y > 0 )
			{
				if( grid[x][y - 1].ColorIndex == colorIndex && grid[x - 1][y].ColorIndex == colorIndex )
					return true;
			}

			if( y < gridHeight - 1 )
			{
				if( grid[x - 1][y].ColorIndex == colorIndex && grid[x - 1][y + 1].ColorIndex == colorIndex )
					return true;
			}
		}

		return false;
	}

	public bool TryGetTupleAt( Vector2 point, out HexagonTuple tuple )
	{
		if( point.x <= 0f || point.x >= gridSize.x || point.y <= 0f || point.y >= gridSize.y )
		{
			tuple = new HexagonTuple();
			return false;
		}

		int x, y;
		Utils.GetCoordinatesFrom( point, out x, out y );

		HexagonPiece.Corner corner = grid[x][y].GetPickableCorner( grid[x][y].GetClosestCorner( point - (Vector2) grid[x][y].transform.localPosition ) );
		tuple = GetTupleAtCorner( x, y, corner );

		return true;
	}

	private HexagonTuple GetTupleAtCorner( int x, int y, HexagonPiece.Corner corner )
	{
		// Make sure that there is a tuple at this corner
		if( grid[x][y].GetPickableCorner( corner ) == corner )
		{
			int y2 = x % 2 == 0 ? y : y + 1;
			switch( corner )
			{
				// It is important that the pieces are stored in clockwise order
				case HexagonPiece.Corner.BottomLeft: return new HexagonTuple( grid[x][y], grid[x][y - 1], grid[x - 1][y2 - 1] );
				case HexagonPiece.Corner.BottomRight: return new HexagonTuple( grid[x][y], grid[x + 1][y2 - 1], grid[x][y - 1] );
				case HexagonPiece.Corner.Left: return new HexagonTuple( grid[x][y], grid[x - 1][y2 - 1], grid[x - 1][y2] );
				case HexagonPiece.Corner.Right: return new HexagonTuple( grid[x][y], grid[x + 1][y2], grid[x + 1][y2 - 1] );
				case HexagonPiece.Corner.TopLeft: return new HexagonTuple( grid[x][y], grid[x - 1][y2], grid[x][y + 1] );
				case HexagonPiece.Corner.TopRight: return new HexagonTuple( grid[x][y], grid[x][y + 1], grid[x + 1][y2] );
			}
		}

		return new HexagonTuple();
	}

	public bool CheckDeadlock()
	{
		matchesOnGridSet.Clear();

		// We can skip odd columns, if there is a match, it will be found while iterating the even columns
		for( int x = 0; x < gridWidth; x += 2 )
		{
			for( int y = 0; y < gridHeight; y++ )
			{
				for( int i = 0; i < 6; i++ )
				{
					HexagonTuple tuple = GetTupleAtCorner( x, y, (HexagonPiece.Corner) i );
					if( !tuple.IsEmpty )
					{
						for( int j = 0; j < 2; j++ )
						{
							tuple.RotateClockwise();
							HexagonMatch match = GetMatchingPiecesAt( tuple );
							if( match != null )
							{
								PoolManager.Instance.Push( match );
								tuple.RotateClockwise( 2 - j );

								return false;
							}
						}

						tuple.RotateClockwise();
					}
				}
			}
		}

		return true;
	}

	public List<HexagonMatch> GetAllMatchingPiecesOnGrid()
	{
		matchesOnGrid.Clear();
		matchesOnGridSet.Clear();

		// We can skip odd columns, if there is a match, it will be found while iterating the even columns
		for( int x = 0; x < gridWidth; x += 2 )
		{
			for( int y = 0; y < gridHeight; y++ )
			{
				HexagonMatch match = GetMatchingPiecesAt( grid[x][y] );
				if( match != null )
					matchesOnGrid.Add( match );
			}
		}

		return matchesOnGrid;
	}

	public HexagonMatch GetMatchingPiecesAt( HexagonTuple tuple )
	{
		matchesOnGridSet.Clear();

		HexagonMatch result = GetMatchingPiecesAt( tuple.piece1 );
		if( result == null )
		{
			result = GetMatchingPiecesAt( tuple.piece2 );
			if( result == null )
				result = GetMatchingPiecesAt( tuple.piece3 );
		}

		return result;
	}

	public HexagonMatch GetMatchingPiecesAt( HexagonPiece piece )
	{
		if( !matchesOnGridSet.Add( piece ) )
			return null;

		HexagonMatch result = PoolManager.Instance.PopMatch();
		GetMatchingPiecesAt( piece, result );
		if( result.Count > 0 )
			return result;

		PoolManager.Instance.Push( result );
		return null;
	}

	private void GetMatchingPiecesAt( HexagonPiece piece, HexagonMatch match )
	{
		bool isPieceAddedToMatch = false;

		for( int i = 0; i < 6; i++ )
		{
			HexagonTuple tuple = GetTupleAtCorner( piece.X, piece.Y, (HexagonPiece.Corner) i );
			if( !tuple.IsEmpty && tuple.IsMatching )
			{
				if( !isPieceAddedToMatch )
				{
					match.Add( piece );
					isPieceAddedToMatch = true;
				}

				if( matchesOnGridSet.Add( tuple.piece1 ) )
					GetMatchingPiecesAt( tuple.piece1, match );
				if( matchesOnGridSet.Add( tuple.piece2 ) )
					GetMatchingPiecesAt( tuple.piece2, match );
				if( matchesOnGridSet.Add( tuple.piece3 ) )
					GetMatchingPiecesAt( tuple.piece3, match );
			}
		}
	}

	public void DestroyMatchingPieces( HexagonMatch match )
	{
		if( match != null )
		{
			for( int i = match.Count - 1; i >= 0; i-- )
			{
				grid[match[i].X][match[i].Y] = null;
				AnimationManager.Instance.BlowPieceAway( match[i] );
			}
		}
	}

	public IEnumerator FillBlankSlots()
	{
		for( int x = 0; x < gridWidth; x++ )
		{
			int numberOfBlankSlots = 0;
			int firstBlankSlot = -1;
			for( int y = 0; y < gridHeight; y++ )
			{
				if( grid[x][y] == null )
				{
					if( ++numberOfBlankSlots == 1 )
						firstBlankSlot = y;
				}
			}

			if( numberOfBlankSlots > 0 )
			{
				for( int y = firstBlankSlot; y < gridHeight - numberOfBlankSlots; y++ )
				{
					for( int y2 = y + 1; y2 < gridHeight; y2++ )
					{
						if( grid[x][y2] != null )
						{
							grid[x][y] = grid[x][y2];
							grid[x][y2] = null;

							break;
						}
					}

					AnimationManager.Instance.MovePieceToPosition( grid[x][y] );
				}

				Vector2 fallingPieceStartPoint = new Vector2( grid[x].XCoord, CameraManager.Instance.YTop );
				if( x % 2 == 0 )
					fallingPieceStartPoint.y += PIECE_HEIGHT * 0.5f;

				for( int y = gridHeight - numberOfBlankSlots; y < gridHeight; y++ )
				{
					HexagonPiece piece = PoolManager.Instance.PopPiece();
					RandomizePieceColor( piece, false );
					grid[x][y] = piece;

					piece.transform.localPosition = fallingPieceStartPoint;
					fallingPieceStartPoint.y += PIECE_DELTA_Y;

					AnimationManager.Instance.MovePieceToPosition( piece );
				}
			}
		}

		while( AnimationManager.Instance.IsAnimating )
			yield return null;
	}
}