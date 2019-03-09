using UnityEngine;
using System.Collections;

public class DynamiteController : MonoBehaviour
{

    [SerializeField]
    GameObject[] lights;
    RatApexDynaController ratApexDynaController;
    AudioSource[] audios;
    const float delay = 1f;
    int currentLight = 0;
    public static float explosionRange = 3f;
    const float explosionMaxHit = 20f;
    const int explosionForce = 6000;
    int layerMask;
    int i = 0;
    int length = 0;

    void Start()
    {
        layerMask = ((1 << LayerMask.NameToLayer("PlayerBody")) | (1 << LayerMask.NameToLayer("Magnetic")) | (1 << LayerMask.NameToLayer("Units")));
        ratApexDynaController = GetComponentInParent<RatApexDynaController>();
        audios = GetComponents<AudioSource>();
        TurnLight(0, false);
        TurnLight(1, false);
        TurnLight(2, false);
    }

    internal void Reset()
    {       
        TurnLight(0, false);
        TurnLight(1, false);
        TurnLight(2, false);
    }

    public void StartCountdown()
    {
        currentLight = 0;
        audios[0].Play();
        TurnLight(0, true);
        TurnLight(1, true);
        TurnLight(2, true);
        StartCoroutine(TurnLightOff());
    }

    IEnumerator TurnLightOff()
    {
        yield return new WaitForSeconds(delay);
        audios[1].Play();
        TurnLight(currentLight, false);
        currentLight++;
        CheckToExplode();
    }


    void CheckToExplode()
    {
        if (currentLight < 3)
        {
            StartCoroutine(TurnLightOff());
        }
        else
        {
            StartCoroutine(MakeExplosion());
        }
    }

    IEnumerator MakeExplosion()
    {
        yield return new WaitForSeconds(delay);
        ratApexDynaController.KillEnemy();
    }


    public void CheckToEffectOnInteractiveObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(ratApexDynaController.gameObject.transform.position, explosionRange, layerMask);
        length = colliders.Length;

        if (length > 0)
        {
            for (i = 0; i < length; i++)
            {
                ISimpleInteractive simple = colliders[i].gameObject.GetComponentInParent<ISimpleInteractive>();
                if (simple != null)
                {
                    if (simple.GetPosition() != ratApexDynaController.gameObject.transform.position) //not react on own body
                    {
                        simple.ExplosionHit(explosionForce, ratApexDynaController.gameObject.transform.position, explosionRange, explosionMaxHit);
                    }                    
                }
                else
                {
                    DamageController playerDamageController = colliders[i].gameObject.GetComponent<DamageController>();
                    if (playerDamageController != null)
                    {
                        playerDamageController.TakeExplosionHit(explosionMaxHit, ratApexDynaController.gameObject.transform.position);
                    }
                }
            }
        }
    }


    void TurnLight(int num, bool enabled)
    {
        lights[num].SetActive(enabled);
    }

    
}
