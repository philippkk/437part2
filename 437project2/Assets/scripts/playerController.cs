using UnityEngine;
using TMPro;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

public class playerController : MonoBehaviour
{
    public GameObject sword;
    bool inRangeOfSword = false;
    public TMP_Text interactionText;
    public TMP_Text healthText;
    public int goalIndex = 0;
    public int health = 3;
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

        healthText.text = "Health: " + health.ToString();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy"))
        {
            Vector3 playerPos = transform.position;
            Vector3 enemyPos = other.gameObject.transform.position;
            playerPos.y = 0;
            enemyPos.y = 0;
            if (Vector3.Distance(playerPos, enemyPos) < 2.2f)
            {
                health--;
                if (health <= 0)
                {
                    interactionText.text = "You died (press r)";
                    firstPersonMovement.enabled = false;
                }
            }
        }
    }
}
