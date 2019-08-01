using UnityEngine;

// A selection around a tuple, the selection is highlighted with a white outline (by default)
public class TupleSelection : MonoBehaviour
{
	public HexagonTuple Tuple { get; private set; }

	private bool m_isVisible;
	public bool IsVisible
	{
		get { return m_isVisible; }
		set
		{
			if( m_isVisible != value )
			{
				m_isVisible = value;

				if( !Tuple.IsEmpty )
					Tuple.SetSelected( value );

				gameObject.SetActive( value );
			}
		}
	}

	private void Awake()
	{
		gameObject.SetActive( false );
	}

	// If the point is located inside the grid, finds the tuple closest to the point and selects it
	public void SelectTupleAt( Vector2 position )
	{
		IsVisible = false;

		HexagonTuple tuple;
		if( GridManager.Instance.TryGetTupleAt( position, out tuple ) )
		{
			Vector3 positionAverage = ( tuple.piece1.transform.localPosition + tuple.piece2.transform.localPosition + tuple.piece3.transform.localPosition ) / 3f;

			// A selection always has two vertical pieces and one adjacent piece to the right or to the left
			bool rightOriented;
			if( tuple.piece1.X == tuple.piece2.X )
				rightOriented = tuple.piece3.X > tuple.piece1.X;
			else if( tuple.piece2.X == tuple.piece3.X )
				rightOriented = tuple.piece1.X > tuple.piece3.X;
			else
				rightOriented = tuple.piece2.X > tuple.piece1.X;

			transform.localPosition = positionAverage;
			transform.localEulerAngles = new Vector3( 0f, 0f, rightOriented ? 0f : 180f );

			Tuple = tuple;
			IsVisible = true;
		}
	}
}