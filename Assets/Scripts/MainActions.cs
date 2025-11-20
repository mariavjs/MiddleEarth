using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(1); //cena jogo
    }

    public void Shop()
    {
        SceneManager.LoadScene(2); //cena menu inicial
    }
}