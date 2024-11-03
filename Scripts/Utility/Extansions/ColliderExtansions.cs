using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColliderExtansions
{
    //public static bool CanTakeDamage(
    //   this Collider other,
    //   float damage,
    //   GameObject owner = null,
    //   bool forSelf = false)
    //{
    //    IHasHealthHandler target = TargetHealthHandler(other, owner, forSelf);

    //    if (target == null) return false;

    //    target.HealthHandler.TakeDamage(damage);
    //    return true;
    //}

    //public static bool CanTakeHeal(
    //    this Collider other,
    //    float heal,
    //    GameObject owner = null,
    //    bool forSelf = false)
    //{
    //    IHasHealthHandler target = TargetHealthHandler(other, owner, forSelf);

    //    if (target == null) return false;

    //    target.HealthHandler.TakeHeal(heal);
    //    return true;
    //}

    //private static IHasHealthHandler TargetHealthHandler(
    //    Collider other,
    //    GameObject owner = null,
    //    bool forSelf = false)
    //{
    //    if (other.gameObject == owner && forSelf == false) return null;

    //    return other.GetComponent<IHasHealthHandler>();
    //}

    //public static bool IsGround(this Collider other)
    //{
    //    if (other.CompareTag("Ground"))
    //        return true;

    //    return false;
    //}

    //public static bool IsTriggerFalse(this Collider other)
    //{
    //    return other.isTrigger == false;
    //}

    //public static bool IsTriggerTrue(this Collider other)
    //{
    //    return other.isTrigger == true;
    //}
}
