using UnityEngine;
using TMPro;

public class playerController : MonoBehaviour
{
    public GameObject sword;
    bool inRangeOfSword = false;
    public TMP_Text interactionText;
    public int goalIndex = 0;

    Rigidbody rb;
    public Animator swordAnimator;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interactionText.text = "";
        sword.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (sword.activeSelf)
        {
            handleSwordAnimation();
        }
    }
    void handleSwordAnimation()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            swordAnimator.SetBool("walking", true);
        }
        else
        {
            swordAnimator.SetBool("walking", false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            swordAnimator.SetTrigger("swing");
        }

        if (Input.GetMouseButtonDown(1))
        {
            swordAnimator.SetBool("blocking", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            swordAnimator.SetBool("blocking", false);
            swordAnimator.SetBool("swinging", false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("sword"))
        {
            interactionText.text = "Press F";

            if (Input.GetKeyDown(KeyCode.F))
            {
                sword.SetActive(true);
                goalIndex++;
                Destroy(other.gameObject);
                interactionText.text = "";
            }
        }
    }
}
