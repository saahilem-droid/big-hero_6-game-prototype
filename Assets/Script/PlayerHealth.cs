using UnityEngine;
public enum AttackType
{
    RightHook,
    Uppercut
}

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    public Animator animator;

    public void TakeDamage(int amount, AttackType attackType, Vector3 attackerPosition)

{
    health -= amount;
    animator.SetBool("IsHit", true);


    // Play correct hit animation
    switch (attackType)
    {
        case AttackType.RightHook:
            animator.SetTrigger("Hit_RightHook");
            break;

        case AttackType.Uppercut:
            animator.SetTrigger("Hit_Uppercut");
            break;
    }

    // --- KNOCKBACK SECTION ---
    Rigidbody rb = GetComponent<Rigidbody>();

    Vector3 knockDir = (transform.position - attackerPosition).normalized;


    if (attackType == AttackType.Uppercut)
    {
       // rb.AddForce(Vector3.up * 5f + knockDir * 3f, ForceMode.Impulse);
    }
    else
    {
       // rb.AddForce(knockDir * 4f, ForceMode.Impulse);
    }

    if (health <= 0)
    {
        Debug.Log("Player Dead");
    }
}
public void EndHit()
{
    animator.SetBool("IsHit", false);
}


}

