using UnityEngine;

public class GridManager : MonoBehaviour
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

	private const float HEX_WIDTH = 1f;
	private const float HEX_HEIGHT = HEX_WIDTH * 0.866025f; // sqrt(3)/2
	private const float HEX_DELTA_X = HEX_WIDTH * 0.75f;
	private const float HEX_DELTA_Y = HEX_HEIGHT;
	private const float HEX_EXCESS_X = HEX_WIDTH * 0.25f;

#pragma warning disable 0649
	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private int gridWidth;

	[SerializeField]
	private int gridHeight;

	[SerializeField]
	private HexagonPiece hexagonPrefab;
#pragma warning restore 0649

	private HexagonPiece[][] grid;

	private void Start()
	{
		if( CreateGrid() )
		{
			Vector3 size = new Vector3( gridWidth * HEX_WIDTH - ( gridWidth - 1 ) * HEX_EXCESS_X, gridHeight * HEX_HEIGHT + HEX_HEIGHT * 0.5f, 0f );
			Vector3 position = size * 0.5f - new Vector3( HEX_WIDTH * 0.5f, HEX_HEIGHT * 0.5f, 10f );

			CameraManager.Instance.SetGridBounds( new Bounds( position, size ) );
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

		Vector3 pos = new Vector3();
		for( int i = 0; i < gridWidth; i++ )
		{
			pos.y = i % 2 == 0 ? 0f : ( HEX_DELTA_Y * 0.5f );

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
}