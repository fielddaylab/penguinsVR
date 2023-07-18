using System.Collections;
using UnityEngine;
using BeauUtil;
using FieldDay.Processes;

namespace Waddle {
    public class PenguinThoughtPebbles : PenguinThoughtState {
        public override IEnumerator Sequence(Process process) {
            PenguinBrain brain = Brain(process);
            PenguinPebbleData pebbleData = brain.GetComponent<PenguinPebbleData>();
            while(pebbleData.PebblesToGather > 0) {
                yield return RNG.Instance.Next(2, 4);
                PebbleSource nearbySource = FindRandomSource(pebbleData);
                brain.SetMainState(PenguinStates.Walk, new PenguinWalkData() { TargetPosition = nearbySource.transform.position });
                yield return null;
                while(brain.Steering.HasTarget) {
                    yield return null;
                }
                brain.Animator.SetTrigger("T_PebblePickup");
                brain.Animator.SetBool("PebbleCarried", true);
                yield return 1;
                yield return new WaitForSeconds(2);
                brain.SetMainState(PenguinStates.Walk, new PenguinWalkData() { TargetPosition = pebbleData.PebbleDropOff.position });
                yield return null;
                while(brain.Steering.HasTarget) {
                    yield return null;
                }

                brain.Animator.SetTrigger("T_PebbleDropOff");
                brain.Animator.SetBool("PebbleCarried", false);
                yield return new WaitForSeconds(3);

                pebbleData.PebblesToGather -= 1;
            }
        }

        static public PebbleSource FindRandomSource(PenguinPebbleData data) {
            return RNG.Instance.Choose(data.PebbleSources);
        }
    }
}