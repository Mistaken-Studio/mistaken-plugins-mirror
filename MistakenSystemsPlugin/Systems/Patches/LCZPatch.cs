#pragma warning disable

using HarmonyLib;
using Mirror;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(Lift), "MovePlayers")]
    public static class LCZPatch
    {
        public static bool Prefix(Lift __instance, Transform target)
        {
            foreach (GameObject gameObject in global::PlayerManager.players)
            {
                GameObject gameObject2;
                if (__instance.InRange(gameObject.transform.position, out gameObject2, 1f) && !(gameObject2.transform == target))
                {
                    global::PlayerMovementSync component = gameObject.GetComponent<global::PlayerMovementSync>();
                    RaycastHit raycastHit;
                    if (Physics.Raycast(gameObject.transform.position, Vector3.down, out raycastHit, 100f, component.CollidableSurfaces))
                    {
                        gameObject.transform.position = raycastHit.point + Vector3.up * 1.23f;
                    }
                    gameObject.transform.parent = gameObject2.transform;
                    Vector3 localPosition = gameObject.transform.localPosition;
                    gameObject.transform.parent = target.transform;
                    gameObject.transform.localPosition = localPosition;
                    gameObject.transform.parent = null;
                    component.AddSafeTime(0.5f, false);
                    component.OverridePosition(gameObject.transform.position, target.transform.rotation.eulerAngles.y - gameObject2.transform.rotation.eulerAngles.y, false);
                    if (NetworkServer.active && LightContainmentZoneDecontamination.DecontaminationController.Singleton.IsDecontaminating)
                    {
                        __instance.CheckMeltPlayer(gameObject);
                    }
                    gameObject.transform.parent = null;
                }
            }
            foreach (global::Scp106PlayerScript scp106PlayerScript in UnityEngine.Object.FindObjectsOfType<global::Scp106PlayerScript>())
            {
                GameObject gameObject3;
                if (scp106PlayerScript != null && __instance.InRange(scp106PlayerScript.portalPosition, out gameObject3, 1f, 2f, 1f) && gameObject3.transform != target)
                {
                    Vector3 position2 = gameObject3.transform.InverseTransformPoint(scp106PlayerScript.portalPosition);
                    scp106PlayerScript.SetPortalPosition(target.TransformPoint(position2));
                }
            }
            global::Lift.Elevator[] array2 = __instance.elevators;
            for (int i = 0; i < array2.Length; i++)
            {
                foreach (Collider collider in Physics.OverlapBox(array2[i].target.transform.position, __instance.maxDistance * 2f * Vector3.one))
                {
                    GameObject gameObject4;
                    if ((collider.GetComponent<global::Pickup>() != null || collider.GetComponent<Grenades.Grenade>() != null) && __instance.InRange(collider.transform.position, out gameObject4, 1f, 1f, 1f) && !(gameObject4.transform == target))
                    {
                        Vector3 position3 = gameObject4.transform.InverseTransformPoint(collider.transform.position);
                        Vector3 vector = gameObject4.transform.InverseTransformVector(collider.transform.eulerAngles);
                        collider.transform.position = target.TransformPoint(position3);
                        collider.transform.eulerAngles = target.TransformVector(vector);
                    }
                }
            }
            array2 = __instance.elevators;
            for (int i = 0; i < array2.Length; i++)
            {
                foreach (Collider collider2 in Physics.OverlapBox(array2[i].target.transform.position, __instance.maxDistance * 2f * Vector3.one))
                {
                    if (collider2.GetComponentInParent<global::Ragdoll>() != null)
                    {
                        global::Ragdoll componentInParent = collider2.GetComponentInParent<global::Ragdoll>();
                        GameObject gameObject5;
                        if (__instance.InRange(collider2.transform.position, out gameObject5, 1f, 1f, 1f) && !(gameObject5.transform == target))
                        {
                            Vector3 position4 = gameObject5.transform.InverseTransformPoint(componentInParent.transform.position);
                            Vector3 vector2 = gameObject5.transform.InverseTransformVector(componentInParent.transform.eulerAngles);
                            componentInParent.transform.position = target.TransformPoint(position4);
                            componentInParent.transform.eulerAngles = target.TransformVector(vector2);
                            componentInParent.RpcSetRootPosition(componentInParent.transform.position);
                        }
                    }
                }
            }
            return false;
        }
    }
}
