using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Processes;
using UnityEngine;

namespace Waddle {
    public class PenguinBrain : ProcessBehaviour, IBeakInteract {
        public Transform Position;
        public Animator Animator;
        public AudioSource BeakAudio;
        public SFXAsset Vocalizations;
        public PenguinType Type;

        [Header("Control Components")]
        public PenguinLookSmoothing LookSmoothing;
        public PenguinSteeringComponent Steering;
        public PenguinFeetSnapping Feet;

        [Header("Wandering")]
        public PenguinWanderData WanderParameters = new PenguinWanderData() { IdleWait = 4, IdleWaitRandom = 4, WanderRadius = 4 };

        [Header("Audio")]
        public SFXAsset DefaultVocalize;

        [Header("-- DEBUG -- ")]
        [SerializeField] private Transform m_DEBUGLookAt;
        [SerializeField] private Transform m_DEBUGWalkTo;

        protected ProcessId m_LookProcess;
        protected ProcessId m_ThoughtProcess;
        protected TransformState m_OriginalTransform;
        protected AnimatorStateSnapshot m_AnimatorSnapshot;

        protected override void Start() {
            StartThinking();
            PenguinGameManager.OnReset += Restart;
            m_OriginalTransform = TransformState.WorldState(Position);
            m_AnimatorSnapshot = new AnimatorStateSnapshot(Animator);
        }

        private void Restart() {
            m_LookProcess.Kill();
            m_MainProcess.Kill();
            m_ThoughtProcess.Kill();
            m_OriginalTransform.Apply(Position);
            Steering.HasTarget = false;
            m_AnimatorSnapshot.Write(Animator);
            BeakAudio.Stop();
            StartThinking();
        }

        private void StartThinking() {
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
                Type = PenguinType.Pebble;
            } else if (GetComponent<PenguinGuideParams>()) {
                m_ThoughtProcess.TransitionTo(PenguinThoughts.Guide);
                Type = PenguinType.Guide;
            } else if (GetComponent<MatingDancePenguin>()) {
                m_ThoughtProcess.Kill();
                Type = PenguinType.Dance;
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

        public void Vocalize(SFXAsset sound, float volume = 1) {
            SFXUtility.Play(BeakAudio, sound, volume);
        }

        public void Vocalize(AudioClip sound, float volume = 1) {
            SFXUtility.Play(BeakAudio, sound, volume);
        }

        public void ForceToIdle(float fadeDuration = 0) {
            ForceToAnimatorState("Idle", fadeDuration);
        }

        public void ForceToAnimatorState(string state, float fadeDuration = 0) {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName(state)) {
                return;
            }

            if (fadeDuration <= 0) {
                Animator.Play(state, 0);
            } else {
                Animator.CrossFadeInFixedTime(state, fadeDuration, 0);
            }
        }

        #region Interaction Callbacks

        public void OnBeakInteract(PlayerBeakState state, BeakTrigger trigger, Collider collider) {
            if (collider.CompareTag(UnityTags.PenguinBeak) && Type == PenguinType.Dance) {
                var progress = Game.SharedState.Get<PlayerProgressState>();
                if (!progress.HasCompleted(PenguinGameManager.MiniGame.MatingDance)) {
                    return;
                }
                var ctrl = GetComponent<MatingDancePenguin>();
                ctrl.HeartParticles.Emit(8);
                Vocalize(ctrl.KissSound);
                trigger.SetCooldown(collider, 1);
            }
        }

        #endregion // Interaction Callbacks
    }

    public enum PenguinType {
        Default,
        Guide,
        Pebble,
        Dance
    }
}