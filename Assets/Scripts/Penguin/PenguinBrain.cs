using BeauUtil;
using FieldDay.Processes;
using UnityEngine;

namespace Waddle {
    public class PenguinBrain : ProcessBehaviour {
        public Transform Position;
        public Animator Animator;
        public PenguinLookSmoothing LookSmoothing;
        public PenguinSteeringComponent Steering;

        [Header("Wandering")]
        public PenguinWanderData WanderParameters = new PenguinWanderData() { IdleWait = 4, IdleWaitRandom = 4, WanderRadius = 4 };

        [Header("-- DEBUG -- ")]
        [SerializeField] private Transform m_DEBUGLookAt;
        [SerializeField] private Transform m_DEBUGWalkTo;

        protected ProcessId m_LookProcess;
        protected ProcessId m_ThoughtProcess;

        protected override void Start() {
            m_LookProcess = StartProcess(PenguinLookStates.Default, "PenguinLook");
            if (m_DEBUGLookAt != null) {
                m_LookProcess.TransitionTo(PenguinLookStates.LookAtTransform, new PenguinLookData() { TargetTransform = m_DEBUGLookAt });
            }

            StartMainProcess(PenguinStates.Idle, "PenguinMain");

            if (m_DEBUGWalkTo != null) {
                m_MainProcess.TransitionTo(PenguinStates.Walk, new PenguinWalkData() { TargetObject = m_DEBUGWalkTo });
            }

            WanderParameters.Tether = Position.position;

            m_ThoughtProcess = StartProcess(PenguinThoughts.Wander, "PenguinThought");
            if (GetComponent<PenguinPebbleData>()) {
                m_ThoughtProcess.TransitionTo(PenguinThoughts.PebbleGather);
            } else if (GetComponent<PenguinGuideParams>()) {
                m_ThoughtProcess.TransitionTo(PenguinThoughts.Guide);
            }
        }

        #region Look State

        public void SetLookState(ProcessStateDefinition lookState) {
            m_LookProcess.TransitionTo(lookState);
        }

        public void SetLookState(ProcessStateDefinition lookState, PenguinLookData lookData) {
            m_LookProcess.TransitionTo(lookState, lookData);
        }

        #endregion // Look State

        #region Main State

        public void SetMainState(ProcessStateDefinition mainState) {
            m_MainProcess.TransitionTo(mainState);
        }

        public void SetMainState<T>(ProcessStateDefinition mainState, in T data) where T : unmanaged {
            m_MainProcess.TransitionTo(mainState, data);
        }

        public void SetWalkState(Vector3 targetPos, float targetPosThreshold = 0.2f) {
            m_MainProcess.TransitionTo(PenguinStates.Walk, new PenguinWalkData() { TargetDistanceThreshold = targetPosThreshold, TargetPosition = targetPos });
        }

        public void SetWalkState(Transform targetPos, float targetPosThreshold = 0.2f) {
            m_MainProcess.TransitionTo(PenguinStates.Walk, new PenguinWalkData() { TargetDistanceThreshold = targetPosThreshold, TargetObject = targetPos });
        }

        #endregion // Main State

        #region Signal

        public override void Signal(StringHash32 signalId, object signalArgs = null) {
            base.Signal(signalId, signalArgs);
            m_LookProcess.Signal(signalId, signalArgs);
            m_ThoughtProcess.Signal(signalId, signalArgs);
        }

        #endregion // Signal
    }
}