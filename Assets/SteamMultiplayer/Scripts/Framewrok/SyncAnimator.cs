//using SteamMultiplayer;
//using UnityEditor.Animations;
//using UnityEngine;
//using UnityEngine.Networking;

//public class SyncAnimator : SteamNetworkBehaviour
//{
//    public Animator animator;
//    private int transition_hash;
//    private int animation_hash;

//    private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime)
//    {
//        stateHash = 0;
//        normalizedTime = 0f;
//        if (animator.IsInTransition(0))
//        {
//            var animator_transition_info = animator.GetAnimatorTransitionInfo(0);
//            if (animator_transition_info.fullPathHash == transition_hash) return false;
//            transition_hash = animator_transition_info.fullPathHash;
//            animation_hash = 0;
//            return true;
//        }
//        var current_animator_state_info = animator.GetCurrentAnimatorStateInfo(0);
//        if (current_animator_state_info.fullPathHash == animation_hash) return false;
//        if (animation_hash != 0)
//        {
//            stateHash = current_animator_state_info.fullPathHash;
//            normalizedTime = current_animator_state_info.normalizedTime;
//        }
//        transition_hash = 0;
//        animation_hash = current_animator_state_info.fullPathHash;
//        return true;
//    }




//    Fields
//    private int m_AnimationHash;
//    [SerializeField]
//    private Animator m_Animator;
//    [SerializeField]
//    private uint m_ParameterSendBits;
//    private NetworkWriter m_ParameterWriter;
//    private float m_SendTimer;
//    private int m_TransitionHash;
//    public string param0;
//    public string param1;
//    public string param2;
//    public string param3;
//    public string param4;
//    public string param5;
//    private static AnimationMessage s_AnimationMessage = new AnimationMessage();
//    private static AnimationParametersMessage s_AnimationParametersMessage = new AnimationParametersMessage();
//    private static AnimationTriggerMessage s_AnimationTriggerMessage = new AnimationTriggerMessage();


//    public struct MyAniationMessage
//    {
//        public int stateHash;
//        public float normalizedTime;
//        public int networkID;
//        public MyAniationMessage(int id, int num, float num2)
//        {
//            networkID = id;
//            stateHash = num;
//            normalizedTime = num2;
//        }
//    }

//    Methods

//    private void CheckSendRate()
//    {
//        if ((this.GetNetworkSendInterval() != 0f) && (this.m_SendTimer < Time.get_time()))
//        {
//            this.m_SendTimer = Time.get_time() + this.GetNetworkSendInterval();
//            AnimationParametersMessage msg = new AnimationParametersMessage
//            {
//                netId = base.netId
//            };
//            this.m_ParameterWriter.SeekZero();
//            this.WriteParameters(this.m_ParameterWriter, true);
//            msg.parameters = this.m_ParameterWriter.ToArray();
//            if (base.hasAuthority && (ClientScene.readyConnection != null))
//            {
//                ClientScene.readyConnection.Send(0x29, msg);
//            }
//            else if (base.isServer && !base.localPlayerAuthority)
//            {
//                NetworkServer.SendToReady(base.get_gameObject(), 0x29, msg);
//            }
//        }
//    }

//    private void FixedUpdate()
//    {
//        if (this.m_ParameterWriter == null) return;
//        int num;
//        float num2;
//        this.CheckSendRate();
//        if (!this.CheckAnimStateChanged(out num, out num2)) return;
//        var msg = new MyAniationMessage(identity.ID, num, num2);

//        Send


//    }

//    public bool GetParameterAutoSend(int index)
//    {
//        return ((this.m_ParameterSendBits & (((int)1) << index)) != 0);
//    }

//    internal void HandleAnimMsg(MyAniationMessage msg, NetworkReader reader)
//    {
//        if (true/*!base.hasAuthority*/)
//        {
//            if (msg.stateHash != 0)
//            {
//                this.m_Animator.Play(msg.stateHash, 0, msg.normalizedTime);
//            }
//            this.ReadParameters(reader, false);
//        }
//    }

//    internal void HandleAnimParamsMsg(AnimationParametersMessage msg, NetworkReader reader)
//    {
//        if (!base.hasAuthority)
//        {
//            this.ReadParameters(reader, true);
//        }
//    }

//    internal void HandleAnimTriggerMsg(int hash)
//    {
//        this.m_Animator.SetTrigger(hash);
//    }

//    internal static void OnAnimationClientMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationMessage>(s_AnimationMessage);
//        GameObject obj2 = ClientScene.FindLocalObject(s_AnimationMessage.netId);
//        if (obj2 != null)
//        {
//            NetworkAnimator component = obj2.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                NetworkReader reader = new NetworkReader(s_AnimationMessage.parameters);
//                component.HandleAnimMsg(s_AnimationMessage, reader);
//            }
//        }
//    }

//    internal static void OnAnimationParametersClientMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationParametersMessage>(s_AnimationParametersMessage);
//        GameObject obj2 = ClientScene.FindLocalObject(s_AnimationParametersMessage.netId);
//        if (obj2 != null)
//        {
//            NetworkAnimator component = obj2.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                NetworkReader reader = new NetworkReader(s_AnimationParametersMessage.parameters);
//                component.HandleAnimParamsMsg(s_AnimationParametersMessage, reader);
//            }
//        }
//    }

//    internal static void OnAnimationParametersServerMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationParametersMessage>(s_AnimationParametersMessage);
//        if (LogFilter.logDev)
//        {
//            Debug.Log(string.Concat(new object[] { "OnAnimationParametersMessage for netId=", s_AnimationParametersMessage.netId, " conn=", netMsg.conn }));
//        }
//        GameObject contextObj = NetworkServer.FindLocalObject(s_AnimationParametersMessage.netId);
//        if (contextObj != null)
//        {
//            NetworkAnimator component = contextObj.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                NetworkReader reader = new NetworkReader(s_AnimationParametersMessage.parameters);
//                component.HandleAnimParamsMsg(s_AnimationParametersMessage, reader);
//                NetworkServer.SendToReady(contextObj, 0x29, s_AnimationParametersMessage);
//            }
//        }
//    }

//    internal static void OnAnimationServerMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationMessage>(s_AnimationMessage);
//        if (LogFilter.logDev)
//        {
//            Debug.Log(string.Concat(new object[] { "OnAnimationMessage for netId=", s_AnimationMessage.netId, " conn=", netMsg.conn }));
//        }
//        GameObject contextObj = NetworkServer.FindLocalObject(s_AnimationMessage.netId);
//        if (contextObj != null)
//        {
//            NetworkAnimator component = contextObj.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                NetworkReader reader = new NetworkReader(s_AnimationMessage.parameters);
//                component.HandleAnimMsg(s_AnimationMessage, reader);
//                NetworkServer.SendToReady(contextObj, 40, s_AnimationMessage);
//            }
//        }
//    }

//    internal static void OnAnimationTriggerClientMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationTriggerMessage>(s_AnimationTriggerMessage);
//        GameObject obj2 = ClientScene.FindLocalObject(s_AnimationTriggerMessage.netId);
//        if (obj2 != null)
//        {
//            NetworkAnimator component = obj2.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                component.HandleAnimTriggerMsg(s_AnimationTriggerMessage.hash);
//            }
//        }
//    }

//    internal static void OnAnimationTriggerServerMessage(NetworkMessage netMsg)
//    {
//        netMsg.ReadMessage<AnimationTriggerMessage>(s_AnimationTriggerMessage);
//        if (LogFilter.logDev)
//        {
//            Debug.Log(string.Concat(new object[] { "OnAnimationTriggerMessage for netId=", s_AnimationTriggerMessage.netId, " conn=", netMsg.conn }));
//        }
//        GameObject contextObj = NetworkServer.FindLocalObject(s_AnimationTriggerMessage.netId);
//        if (contextObj != null)
//        {
//            NetworkAnimator component = contextObj.GetComponent<NetworkAnimator>();
//            if (component != null)
//            {
//                component.HandleAnimTriggerMsg(s_AnimationTriggerMessage.hash);
//                NetworkServer.SendToReady(contextObj, 0x2a, s_AnimationTriggerMessage);
//            }
//        }
//    }

//    public override void OnDeserialize(NetworkReader reader, bool initialState)
//    {
//        if (initialState)
//        {
//            int num = reader.ReadInt32();
//            float num2 = reader.ReadSingle();
//            this.ReadParameters(reader, false);
//            this.m_Animator.Play(num, 0, num2);
//        }
//    }

//    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
//    {
//        if (forceAll)
//        {
//            if (this.m_Animator.IsInTransition(0))
//            {
//                AnimatorStateInfo nextAnimatorStateInfo = this.m_Animator.GetNextAnimatorStateInfo(0);
//                writer.Write(nextAnimatorStateInfo.get_fullPathHash());
//                writer.Write(nextAnimatorStateInfo.get_normalizedTime());
//            }
//            else
//            {
//                AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
//                writer.Write(currentAnimatorStateInfo.get_fullPathHash());
//                writer.Write(currentAnimatorStateInfo.get_normalizedTime());
//            }
//            this.WriteParameters(writer, false);
//            return true;
//        }
//        return false;
//    }

//    public override void OnStartAuthority()
//    {
//        this.m_ParameterWriter = new NetworkWriter();
//    }

//    private void ReadParameters(NetworkReader reader, bool autoSend)
//    {
//        for (int i = 0; i < this.m_Animator.get_parameters().Length; i++)
//        {
//            if (!autoSend || this.GetParameterAutoSend(i))
//            {
//                AnimatorControllerParameter parameter = this.m_Animator.get_parameters()[i];
//                if (parameter.get_type() == 3)
//                {
//                    int num2 = (int)reader.ReadPackedUInt32();
//                    this.m_Animator.SetInteger(parameter.get_nameHash(), num2);
//                    this.SetRecvTrackingParam(parameter.get_name() + ":" + num2, i);
//                }
//                if (parameter.get_type() == 1)
//                {
//                    float num3 = reader.ReadSingle();
//                    this.m_Animator.SetFloat(parameter.get_nameHash(), num3);
//                    this.SetRecvTrackingParam(parameter.get_name() + ":" + num3, i);
//                }
//                if (parameter.get_type() == 4)
//                {
//                    bool flag = reader.ReadBoolean();
//                    this.m_Animator.SetBool(parameter.get_nameHash(), flag);
//                    this.SetRecvTrackingParam(parameter.get_name() + ":" + flag, i);
//                }
//            }
//        }
//    }

//    internal void ResetParameterOptions()
//    {
//        Debug.Log("ResetParameterOptions");
//        this.m_ParameterSendBits = 0;
//    }

//    public void SetParameterAutoSend(int index, bool value)
//    {
//        if (value)
//        {
//            this.m_ParameterSendBits |= ((uint)1) << index;
//        }
//        else
//        {
//            this.m_ParameterSendBits &= (uint)~(((int)1) << index);
//        }
//    }

//    private void SetRecvTrackingParam(string p, int i)
//    {
//        p = "Recv Param: " + p;
//        if (i == 0)
//        {
//            this.param0 = p;
//        }
//        if (i == 1)
//        {
//            this.param1 = p;
//        }
//        if (i == 2)
//        {
//            this.param2 = p;
//        }
//        if (i == 3)
//        {
//            this.param3 = p;
//        }
//        if (i == 4)
//        {
//            this.param4 = p;
//        }
//        if (i == 5)
//        {
//            this.param5 = p;
//        }
//    }

//    private void SetSendTrackingParam(string p, int i)
//    {
//        p = "Sent Param: " + p;
//        if (i == 0)
//        {
//            this.param0 = p;
//        }
//        if (i == 1)
//        {
//            this.param1 = p;
//        }
//        if (i == 2)
//        {
//            this.param2 = p;
//        }
//        if (i == 3)
//        {
//            this.param3 = p;
//        }
//        if (i == 4)
//        {
//            this.param4 = p;
//        }
//        if (i == 5)
//        {
//            this.param5 = p;
//        }
//    }

//    public void SetTrigger(int hash)
//    {
//        AnimationTriggerMessage msg = new AnimationTriggerMessage
//        {
//            netId = base.netId,
//            hash = hash
//        };
//        if (base.hasAuthority && base.localPlayerAuthority)
//        {
//            if (NetworkClient.allClients.Count > 0)
//            {
//                NetworkConnection readyConnection = ClientScene.readyConnection;
//                if (readyConnection != null)
//                {
//                    readyConnection.Send(0x2a, msg);
//                }
//            }
//        }
//        else if (base.isServer && !base.localPlayerAuthority)
//        {
//            NetworkServer.SendToReady(base.get_gameObject(), 0x2a, msg);
//        }
//    }

//    public void SetTrigger(string triggerName)
//    {
//        this.SetTrigger(AnimatorState.StringToHash(triggerName));
//    }

//    private void WriteParameters(NetworkWriter writer, bool autoSend)
//    {
//        for (int i = 0; i < this.m_Animator.get_parameters().Length; i++)
//        {
//            if (!autoSend || this.GetParameterAutoSend(i))
//            {
//                AnimatorControllerParameter parameter = this.m_Animator.parameters()[i];
//                if (parameter.get_type() == 3)
//                {
//                    writer.WritePackedUInt32((uint)this.m_Animator.GetInteger(parameter.get_nameHash()));
//                    this.SetSendTrackingParam(parameter.get_name() + ":" + this.m_Animator.GetInteger(parameter.get_nameHash()), i);
//                }
//                if (parameter.get_type() == 1)
//                {
//                    writer.Write(this.m_Animator.GetFloat(parameter.get_nameHash()));
//                    this.SetSendTrackingParam(parameter.get_name() + ":" + this.m_Animator.GetFloat(parameter.get_nameHash()), i);
//                }
//                if (parameter.get_type() == 4)
//                {
//                    writer.Write(this.m_Animator.GetBool(parameter.get_nameHash()));
//                    this.SetSendTrackingParam(parameter.get_name() + ":" + this.m_Animator.GetBool(parameter.get_nameHash()), i);
//                }
//            }
//        }
//    }

//    Properties
//    public AnimatorState animator
//    {
//        get
//        {
//            return this.m_Animator;
//        }
//        set
//        {
//            this.m_Animator = value;
//            this.ResetParameterOptions();
//        }
//    }
//}



