using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Executes the operations on the grid
public class GridManager : ManagerBase<GridManager>
{
	// A column on the grid
	public class Column
	{
		private readonly int column;
		private readonly HexagonPiece[] rows;

		private readonly Vector2 bottomCoords; // Cached data to calculate a hexagon piece's position on the grid as quickly as possible
		public float XCoord { get { return bottomCoords.x; } }

		// Changing the hexagon piece at a given row automatically updates that piece's cached coordinates (SetPosition)
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

		// Calculates the position of a hexagon piece
		public Vector3 CalculatePositionAt( int row )
		{
			return new Vector3( bottomCoords.x, bottomCoords.y + row * PIECE_DELTA_Y, 0f );
		}
	}

	// Hexagon pieces' properties in world units, they are all calculated using the width value
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

	// Used to find the matching hexagon pieces on the grid
	private readonly List<HexagonMatch> matchesOnGrid = new List<HexagonMatch>( 4 );
	private readonly HashSet<HexagonPiece> matchesOnGridSet = new HashSet<HexagonPiece>();

	private Column[] grid;
	private Vector2 gridSize; // Total size of the grid in world units

	// Accessing a column by index from outside
	public Column this[int column] { get { return grid[column]; } }

	private void Start()
	{
		if( CreateGrid() )
		{
			gridSize = new Vector3( gridWidth * PIECE_WIDTH - ( gridWidth - 1 ) * PIECES_INTERSECTION_WIDTH, gridHeight * PIECE_HEIGHT + PIECE_HEIGHT * 0.5f, 0f );
			CameraManager.Instance.SetGridBounds( new Bounds( gridSize * 0.5f, gridSize ) );
		}
	}

	protected override void ReleaseResources()
	{
		DestroyGrid();
		grid = null;
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

		// Continuously create the grid until there is no deadlock on the grid (i.e. there is at least one possible match)
		while( true )
		{
			for( int x = 0; x < gridWidth; x++ )
			{
				for( int y = 0; y < gridHeight; y++ )
				{
					HexagonPiece piece = PoolManager.Instance.PopPiece();
					grid[x][y] = piece;

					piece.transform.localPosition = grid[x].CalculatePositionAt( y );
					RandomizePieceColor( piece, true ); // true: ensures that the picked color for this hexagon piece won't cause a match at the start
				}
			}

			if( !CheckDeadlock() )
				break;

			// There is deadlock, start the process over
			DestroyGrid();
		}

		return true;
	}

	private void DestroyGrid()
	{
		if( grid == null )
			return;

		for( int x = 0; x < gridWidth; x++ )
		{
			for( int y = 0; y < gridHeight; y++ )
				PoolManager.Instance.Push( grid[x][y] );
		}
	}

	// Pick a random color for the hexagon piece
	private void RandomizePieceColor( HexagonPiece piece, bool ensureNonMatchingColor )
	{
		int colorIndex = Random.Range( 0, colors.Length );
		if( ensureNonMatchingColor )
		{
			// Make sure that assigning this color to the hexagon piece won't cause a match on the grid
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

	// Returns true if giving a particular color to a hexagon piece results in a match on the grid
	// Checks only the hexagon pieces that were created before this hexagon piece, no need to check the
	// hexagon pieces that are not yet created
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

	// If points resides inside the grid, locate the three adjacent hexagon pieces (tuple) that are closest to the point
	public bool TryGetTupleAt( Vector2 point, out HexagonTuple tuple )
	{
		if( point.x <= 0f || point.x >= gridSize.x || point.y <= 0f || point.y >= gridSize.y )
		{
			tuple = new HexagonTuple();
			return false;
		}

		// Calculate the row and column indices of the hexagon piece that the point resides inside
		int x, y;
		Utils.GetCoordinatesFrom( point, out x, out y );

		// Find the hexagon piece's corner that is closest to the point
		// This corner is guaranteed to have two adjacent hexagon pieces (i.e. GetPickableCorner)
		HexagonPiece.Corner corner = grid[x][y].GetPickableCorner( grid[x][y].GetClosestCorner( point - (Vector2) grid[x][y].transform.localPosition ) );
		tuple = GetTupleAtCorner( x, y, corner );

		return true;
	}

	// Returns the tuple located at the corner of a particular hexagon piece
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

	// Returns true if there are no possible moves that result in a match on the grid
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
					// For each possible tuple on the grid
					HexagonTuple tuple = GetTupleAtCorner( x, y, (HexagonPiece.Corner) i );
					if( !tuple.IsEmpty )
					{
						// Check if rotating the tuple once or twice results in a match
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

						// There is no match after 2 rotations, rotate the tuple one last time to restore its original state
						tuple.RotateClockwise();
					}
				}
			}
		}

		return true;
	}

	// Finds all matches on the grid
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

	// Finds the match that have at least one hexagon piece from this tuple
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

	// Find the match that this hexagon piece is part of
	public HexagonMatch GetMatchingPiecesAt( HexagonPiece piece )
	{
		// Don't search the piece for match if it is already searched before
		if( !matchesOnGridSet.Add( piece ) )
			return null;

		HexagonMatch result = PoolManager.Instance.PopMatch();
		GetMatchingPiecesAt( piece, result );
		if( result.Count > 0 )
			return result;

		PoolManager.Instance.Push( result );
		return null;
	}

	// Find the match that this hexagon piece is part of (implementation)
	private void GetMatchingPiecesAt( HexagonPiece piece, HexagonMatch match )
	{
		bool isPieceAddedToMatch = false;

		// Iterate over each possible tuple that is formed with this hexagon piece and see if that tuple is a match
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

	// Fill the blank slots with animation
	public IEnumerator FillBlankSlots()
	{
		for( int x = 0; x < gridWidth; x++ )
		{
			// Check if there is a blank slot on this column
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

			// If there is at least one blank slot on this column
			if( numberOfBlankSlots > 0 )
			{
				// First, fill the blanks slots with the hexagon pieces above them (if any)
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

				// Fill the remaining blank slots with new hexagon pieces falling from above
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

		// Wait until all pieces sit on their positions
		while( AnimationManager.Instance.IsAnimating )
			yield return null;
	}
}