using UnityEngine;

public class HexagonPiece : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private SpriteRenderer spriteRenderer;
#pragma warning restore 0649

	public int ColorIndex { get; private set; }

	public void SetColor( int colorIndex, Color color )
	{
		ColorIndex = colorIndex;
		spriteRenderer.color = color;
	}
}