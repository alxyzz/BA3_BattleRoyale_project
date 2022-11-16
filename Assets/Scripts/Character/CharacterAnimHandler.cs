using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimHandler : NetworkBehaviour
{
    [SerializeField] private Animator _tpAnimator;
    [SerializeField] private Animator _fpAnimator;

    [Command]
    public void CmdTpSetFloat(int hashId, float val)
    {
        RpcTpSetFloat(hashId, val);
    }
    [ClientRpc]
    private void RpcTpSetFloat(int hashId, float val)
    {
        _tpAnimator.SetFloat(hashId, val);
    }
    public void FpSetFloat(int hashId, float val)
    {
        _fpAnimator.SetFloat(hashId, val);
    }

    [Command]
    public void CmdTpSetBool(int hashId, bool val)
    {
        RpcTpSetBool(hashId, val);
    }
    [ClientRpc]
    private void RpcTpSetBool(int hashId, bool val)
    {
        _tpAnimator.SetBool(hashId, val);
    }
    public void FpSetBool(int hashId, bool val)
    {
        _fpAnimator.SetBool(hashId, val);
    }

    [Command]
    public void CmdTpSetTrigger(int hashId)
    {
        RpcTpSetTrigger(hashId);
    }
    [ClientRpc]
    private void RpcTpSetTrigger(int hashId)
    {
        _tpAnimator.SetTrigger(hashId);
    }
    public void FpSetTrigger(int hashId)
    {
        _fpAnimator.SetTrigger(hashId);
    }
    public void FpResetTrigger(int hashId)
    {
        _fpAnimator.ResetTrigger(hashId);
    }
    [Command]
    public void CmdTpSetLayerWeight(int index, int weight)
    {
        RpcTpSetLayerWeight(index, weight);
    }
    [ClientRpc]
    private void RpcTpSetLayerWeight(int index, int weight)
    {
        _tpAnimator.SetLayerWeight(index, weight);
    }
}
