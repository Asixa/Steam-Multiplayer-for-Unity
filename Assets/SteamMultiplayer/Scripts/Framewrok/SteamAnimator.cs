//*********************
// 我牛逼不？-----Asixa
//*********************
using System;
using UnityEngine;

namespace SteamMultiplayer
{
    [AddComponentMenu("SteamMultiplayer/Steam Sync Animator")]
    public class SteamAnimator : SteamNetworkBehaviour
    {
        #region Variables
        public Animator animator;
        public int TimesPerSecond = 9;
        private float CurrentTime;
        private int transition_hash;
        private int animation_hash;
        #endregion

        #region Unity Reserved Functions
        public new void Awake()
        {
            base.Awake();
            GetComponent<Identity>().anim = this;
        }
        public void Update()
        {
            if (animator == null) return;
            if (!identity.IsLocalSpawned) return;
            CurrentTime -= Time.deltaTime;
            if (!(CurrentTime <= 0)) return;
            CurrentTime = 1f / TimesPerSecond;
            SMC.SendPacketsQuicklly(new P2PPackage(GetParamter(), P2PPackageType.AnimatorParamter, identity), false);
            int num;
            float num2;
            if (!CheckAnimStateChanged(out num, out num2)) return;
            var msg = new MyAniationMessage( animator);
            SMC.SendPacketsQuicklly(new P2PPackage(msg, P2PPackageType.AnimatorState, identity), false);
        }
        #endregion

        #region Structs
        [Serializable]
        public struct MyAniationMessage
        {
            public int stateHash;
            public float normalizedTime;


            public MyAniationMessage(Animator a)
            {
                if (a.IsInTransition(0))
                {
                    var nextAnimatorStateInfo = a.GetNextAnimatorStateInfo(0);
                    stateHash = nextAnimatorStateInfo.fullPathHash;
                    normalizedTime = nextAnimatorStateInfo.normalizedTime;
                }
                else
                {
                    var nextAnimatorStateInfo = a.GetCurrentAnimatorStateInfo(0);
                    stateHash = nextAnimatorStateInfo.fullPathHash;
                    normalizedTime = nextAnimatorStateInfo.normalizedTime;
                }
            }
        }
        [Serializable]
        public struct MyAniationParamterMessage
        {
            public string name;
            public int _int;
            public float _float;
            public bool _bool;
            public int type;

            public MyAniationParamterMessage(AnimatorControllerParameter p,Animator a)
            {
                _int = a.GetInteger(p.nameHash);
                _float = a.GetFloat(p.nameHash);
                _bool = a.GetBool(p.nameHash);
                name = p.name;
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        type = 1;
                        break;
                    case AnimatorControllerParameterType.Int:
                        type = 3;
                        break;
                    case AnimatorControllerParameterType.Bool:
                        type = 4;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        type = 9;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        #region GetData

        public MyAniationParamterMessage[] GetParamter()
        {
            var p = animator.parameters;
            var o = new MyAniationParamterMessage[p.Length];
            for (var i = 0; i < p.Length; i++) o[i] = new MyAniationParamterMessage(p[i], animator);
            return o;
        }

        //private static AnimatorControllerParameter ToParameter(MyAniationParamterMessage m)
        //{
        //    var p = new AnimatorControllerParameter
        //    {
        //        name = m.name,
        //        defaultBool = m._bool,
        //        defaultFloat = m._float,
        //        defaultInt = m._int
        //    };
            
            
        //    switch (m.type)
        //    {
        //        case 1:
        //            p.type = AnimatorControllerParameterType.Float;
        //            break;
        //        case 3:
        //            p.type = AnimatorControllerParameterType.Int;
        //            break;
        //        case 4:
        //            p.type = AnimatorControllerParameterType.Bool;
        //            break;
        //        case 9:
        //            p.type = AnimatorControllerParameterType.Trigger;
        //            break;
        //        default:
        //            break;
        //    }
        //    return p;
        //}
        #endregion

        #region SetData

        public void SetAnimState(MyAniationMessage msg)
        {
            if (identity.IsLocalSpawned) return;
            if (msg.stateHash == 0) return;
            animator.Play(msg.stateHash, 0, msg.normalizedTime);
        }

        public void SetParamter(MyAniationParamterMessage[] x)
        {
            for (var i = 0; i < animator.parameters.Length; i++)
            {
                var acp = x[i];
                switch (acp.type)
                {
                    case 1:
                        var num = acp._int;
                        animator.SetInteger(acp.name, num);
                        break;
                    case 3:
                        var rel = acp._float;
                        animator.SetFloat(acp.name, rel);
                        break;
                    case 4:
                        var boolen = acp._bool;
                        animator.SetBool(acp.name, boolen);
                        break;
                    case 9:
                        animator.SetTrigger(acp.name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        private bool CheckAnimStateChanged(out int state_hash, out float normalized_time)
        {
            state_hash = 0;
            normalized_time = 0f;
            if (animator.IsInTransition(0))
            {
                var animator_transition_info = animator.GetAnimatorTransitionInfo(0);
                if (animator_transition_info.fullPathHash == transition_hash) return false;
                transition_hash = animator_transition_info.fullPathHash;
                animation_hash = 0;
                return true;
            }
            var current_animator_state_info = animator.GetCurrentAnimatorStateInfo(0);
            if (current_animator_state_info.fullPathHash == animation_hash) return false;
            if (animation_hash != 0)
            {
                state_hash = current_animator_state_info.fullPathHash;
                normalized_time = current_animator_state_info.normalizedTime;
            }
            transition_hash = 0;
            animation_hash = current_animator_state_info.fullPathHash;
            return true;
        }

    }
}
