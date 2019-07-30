using UnityEngine;

public class GridManager : ManagerBase<GridManager>
{
#if UNITY_EDITOR
	// ??
	private static readonly string MAGICK = new string( new char[]
	{
		(char) 103, (char) 100, (char) 107, (char) 107, (char) 110,
		(char) 32,
		(char) 117, (char) 100, (char) 113, (char) 115, (char) 104, (char) 102, (char) 110
	} );
	// ++
#endif

	public const float HEX_WIDTH = 1f;
	public const float HEX_HEIGHT = HEX_WIDTH * 0.866025f; // sqrt(3)/2
	private const float HEX_DELTA_X = HEX_WIDTH * 0.75f;
	private const float HEX_DELTA_Y = HEX_HEIGHT;
	private const float HEX_INTERSECT_WIDTH = HEX_WIDTH * 0.25f;

#pragma warning disable 0649
	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private int gridWidth;
	public int Width { get { return gridWidth; } }

	[SerializeField]
	private int gridHeight;
	public int Height { get { return gridHeight; } }

	[SerializeField]
	private HexagonPiece hexagonPrefab;
#pragma warning restore 0649

	private HexagonPiece[][] grid;
	private Vector2 gridSize;

	private void Start()
	{
		if( CreateGrid() )
		{
			gridSize = new Vector3( gridWidth * HEX_WIDTH - ( gridWidth - 1 ) * HEX_INTERSECT_WIDTH, gridHeight * HEX_HEIGHT + HEX_HEIGHT * 0.5f, 0f );
			CameraManager.Instance.SetGridBounds( new Bounds( gridSize * 0.5f, gridSize ) );
		}
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

		grid = new HexagonPiece[gridWidth][];
		for( int i = 0; i < gridWidth; i++ )
			grid[i] = new HexagonPiece[gridHeight];

		// Offset the starting point by half size of a hexagon piece so that the bottom left point resides at (0,0)
		Vector3 pos = new Vector3( HEX_WIDTH * 0.5f, 0f, 0f );
		for( int i = 0; i < gridWidth; i++ )
		{
			pos.y = HEX_HEIGHT * 0.5f;
			if( i % 2 == 1 )
				pos.y += HEX_DELTA_Y * 0.5f;

			for( int j = 0; j < gridHeight; j++ )
			{
				HexagonPiece hexagonPiece = Instantiate( hexagonPrefab );
				hexagonPiece.transform.localPosition = pos;

				int colorIndex = Random.Range( 0, colors.Length );
				while( CheckMatch( i, j, colorIndex ) )
				{
					// Make sure that assigning this color to the hexagon piece won't cause a match in the grid
					int newColorIndex;
					do
					{
						newColorIndex = Random.Range( 0, colors.Length );
					} while( newColorIndex == colorIndex );

					colorIndex = newColorIndex;
				}

				hexagonPiece.SetColor( colorIndex, colors[colorIndex] );
				grid[i][j] = hexagonPiece;

				pos.y += HEX_DELTA_Y;
			}

			pos.x += HEX_DELTA_X;
		}

		return true;
	}

	private bool CheckMatch( int i, int j, int colorIndex )
	{
		if( i == 0 )
			return false;

		if( i % 2 == 0 )
		{
			if( j > 0 )
			{
				if( ( grid[i][j - 1].ColorIndex == colorIndex && grid[i - 1][j - 1].ColorIndex == colorIndex ) ||
					( grid[i - 1][j - 1].ColorIndex == colorIndex && grid[i - 1][j].ColorIndex == colorIndex ) )
					return true;
			}
		}
		else
		{
			if( j > 0 )
			{
				if( grid[i][j - 1].ColorIndex == colorIndex && grid[i - 1][j].ColorIndex == colorIndex )
					return true;
			}

			if( j < gridHeight - 1 )
			{
				if( grid[i - 1][j].ColorIndex == colorIndex && grid[i - 1][j + 1].ColorIndex == colorIndex )
					return true;
			}
		}

		return false;
	}

	public bool TryGetPiecesAt( Vector2 point, out HexagonTuple tuple )
	{
		if( point.x <= 0f || point.x >= gridSize.x || point.y <= 0f || point.y >= gridSize.y )
		{
			tuple = new HexagonTuple();
			return false;
		}

		float column = ( point.x - HEX_WIDTH * 0.5f ) / HEX_DELTA_X;
		int x = Mathf.Clamp( Mathf.RoundToInt( column ), 0, gridWidth - 1 );

		float row = ( x % 2 == 0 ? point.y : point.y - HEX_HEIGHT * 0.5f ) / HEX_HEIGHT;
		int y = Mathf.Clamp( (int) row, 0, gridHeight - 1 );

		Corner corner = grid[x][y].GetClosestPickableCorner( point - (Vector2) grid[x][y].transform.localPosition, x, y );
		int x2 = ( corner == Corner.BottomLeft || corner == Corner.Left || corner == Corner.TopLeft ) ? x - 1 : x + 1;
		int y2 = x % 2 == 0 ? y : y + 1;

		switch( corner )
		{
			case Corner.BottomLeft:
			case Corner.BottomRight: tuple = new HexagonTuple( grid[x][y], grid[x][y - 1], grid[x2][y2 - 1] ); break;
			case Corner.Left:
			case Corner.Right: tuple = new HexagonTuple( grid[x][y], grid[x2][y2], grid[x2][y2 - 1] ); break;
			case Corner.TopLeft:
			case Corner.TopRight: tuple = new HexagonTuple( grid[x][y], grid[x][y + 1], grid[x2][y2] ); break;
			default: tuple = new HexagonTuple(); break;
		}

		return true;
	}
}