using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFreezeEffect : MonoBehaviour
{
    public ParticleSystem ps ;

    private float pauseTimer ;
    
    void Update()
    {
        pauseTimer += Time.deltaTime ;
        if (pauseTimer > 1.10f)
        {
            if (ps.isPlaying && pauseTimer < 3f)
            {
                ps.Pause(); 
            }
            else if (ps.isPaused && pauseTimer >= 3f)
            {
                ps.Play();
            }
            else if (pauseTimer >= 4.5)
            {
                Destroy(gameObject);
            }
        }
        
    }
}
