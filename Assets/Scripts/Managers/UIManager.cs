using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Handles the UI operations
public class UIManager : ManagerBase<UIManager>
{
#pragma warning disable 0649
	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI gameOverText;

	[SerializeField]
	private Button restartButton;

	[SerializeField]
	private InputReceiver inputReceiver;

	[SerializeField]
	private Animation animator;
#pragma warning restore 0649

	protected override void Awake()
	{
		base.Awake();

		// Restarting the scene can be more beneficial than repopulating the grid in the same scene because
		// unused references (if any) are released between scene changes and probably GC runs, as well
		restartButton.onClick.AddListener( () => SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex ) );
	}

	public void UpdateScore( int score )
	{
		scoreText.text = score.ToString();
	}

	public void GameOver( int score, int highscore )
	{
		inputReceiver.gameObject.SetActive( false );
		gameOverText.text = string.Format( gameOverText.text, score, highscore ); // This text has {0} for score and {1} for highscore

		// Fade from ingame UI to game over UI
		animator.Play( "UI_FadeToGameOver" );
	}
}