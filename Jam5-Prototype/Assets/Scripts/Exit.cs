using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{

	[SerializeField] private GameObject aura;

	[SerializeField] private GameObject firework;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {       
        if (collision.tag == "Player")
        {          
            if(GameManager.Instance.PlayerCarryChair == collision.name)
            {
                LogUtility.PrintLogFormat("Door", "{0} win!", GameManager.Instance.PlayerCarryChair);
	            StartCoroutine(playAnim());
                AudioManager.Instance.PlaySoundEffect("Win", volume:0.5f);
                GameManager.Instance.EndCurrentRound();
            }

        }
    }

	IEnumerator playAnim()
	{
		aura.SetActive(true);
		firework.SetActive(true);
		firework.GetComponent<ParticleSystem>().Play(true);
		yield return  new WaitForSeconds(1.5f);
		aura.SetActive(false);
		firework.SetActive(false);
	}
}
