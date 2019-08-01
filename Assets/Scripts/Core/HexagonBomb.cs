using TMPro;
using UnityEngine;

// Attaches itself to a hexagon piece and ends the game if counter reaches zero
public class HexagonBomb : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private TextMeshPro remainingTurnsText;
#pragma warning restore 0649

	public HexagonPiece AttachedPiece { get; private set; }
	private int remainingTurns;

	public void Initialize( HexagonPiece attachedPiece, int remainingTurns )
	{
		AttachedPiece = attachedPiece;
		this.remainingTurns = remainingTurns;

		transform.SetParent( attachedPiece.transform, false );
		remainingTurnsText.text = remainingTurns.ToString();
	}

	// Returns true if BOOOM
	public bool Tick()
	{
		remainingTurns--;
		remainingTurnsText.text = remainingTurns.ToString();

		return remainingTurns <= 0;
	}
}