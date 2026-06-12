using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Allcontrol.GameManager;

public class item : MonoBehaviour
{
    int cherries = Allcontrol.GameManager.Instance.scores;
    [SerializeField] private Text cherriesText;
    [SerializeField] private AudioSource CollectSoundEffect;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cherry"))
        {
            CollectSoundEffect.Play();
            Destroy(collision.gameObject);
            cherries++;
            cherriesText.text = "Cherries: " + cherries;
            Instance.scores = cherries;
        }
    }
}
