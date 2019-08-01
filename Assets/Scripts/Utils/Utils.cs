using UnityEngine;

// Utility functions
public static class Utils
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

	// Finds the row and column indices of the hexagon piece that is located at the position
	public static void GetCoordinatesFrom( Vector2 position, out int x, out int y )
	{
		float column = ( position.x - GridManager.PIECE_WIDTH * 0.5f ) / GridManager.PIECE_DELTA_X;
		x = Mathf.Clamp( Mathf.RoundToInt( column ), 0, GridManager.Instance.Width - 1 );

		float row = ( x % 2 == 0 ? position.y : position.y - GridManager.PIECE_HEIGHT * 0.5f ) / GridManager.PIECE_HEIGHT;
		y = Mathf.Clamp( (int) row, 0, GridManager.Instance.Height - 1 );
	}
}