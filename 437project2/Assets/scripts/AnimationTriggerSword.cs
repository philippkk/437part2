using UnityEngine;

public class AnimationTriggerSword : MonoBehaviour
{
    public Animator swordAnimator;
    void Start()
    {
        swordAnimator = GetComponent<Animator>();
    }

    void SetSwingFalse()
    {
        swordAnimator.SetBool("swinging", false);
    }
      void SetSwingTrue()
    {
       swordAnimator.SetBool("swinging", true); 
    }
}
