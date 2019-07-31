using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
		restartButton.onClick.AddListener( () => SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex ) );
	}

	public void UpdateScore( int score )
	{
		scoreText.text = score.ToString();
	}

	public void GameOver( int score, int highscore )
	{
		inputReceiver.gameObject.SetActive( false );
		gameOverText.text = string.Format( gameOverText.text, score, highscore );

		animator.Play( "UI_FadeToGameOver" );
	}
}