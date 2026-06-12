using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    [SerializeField] private AudioSource FinishSoundEffect;
    private bool LevelCompleted = false;
    private void Start()
    {
        FinishSoundEffect = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !LevelCompleted)
        {
            FinishSoundEffect.Play();
            LevelCompleted = true;
            Invoke("CompleteLevel", FinishSoundEffect.clip.length);
        }
    }

    // Update is called once per frame
    private void CompleteLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }
}
