using UnityEngine;
using TMPro;

public class playerController : MonoBehaviour
{
    public GameObject sword;
    bool inRangeOfSword = false;
    public TMP_Text interactionText;
    public int goalIndex = 0;
    public bool playingCheckers = false;

    Rigidbody rb;
    public Animator swordAnimator;

    public FirstPersonMovement firstPersonMovement;
    public Camera playerCamera;
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

        if (playingCheckers)
        {
            sword.SetActive(false);
            firstPersonMovement.enabled = false;
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

    public void SetInteractionText(string text)
    {
        interactionText.text = text;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("sword"))
        {
            SetInteractionText("Press F");

            if (Input.GetKeyDown(KeyCode.F))
            {
                sword.SetActive(true);
                goalIndex++;
                Destroy(other.gameObject);
                SetInteractionText("");
            }
        }


    }
}
