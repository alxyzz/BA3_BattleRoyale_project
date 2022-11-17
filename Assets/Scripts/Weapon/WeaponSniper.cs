using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class WeaponSniper : WeaponInHand
{
    protected int _scopeLevel;  // 0 is close FOV is 60; 1 is FOV 10; 2 is FOV 1
    [SerializeField] protected int _maxScopeLevel = 2;
    protected override float FireSpreadRadius => GetFireSpreadRadius(_scopeLevel == 0 ? _identity.Data.FireSpread : 0.0005f);

    public override bool CanToggleScope()
    {
        return !_isFiring && !_isReloading && !IsHolstered;
    }
    public override void ToggleScope()
    {
        base.ToggleScope();
        _scopeLevel = (_scopeLevel + 1) % (_maxScopeLevel + 1);
        SetThingsByScopeLevel(_scopeLevel);
    }
    
    public override void FireBurst(out List<Vector3> directions)
    {
        SetThingsByScopeLevel(0);
        SetInspect(false);
        _identity.CurrentAmmo--;
        _isFiring = true;
        HitScan(out directions);
        _recoilValue = 1;
        Camera.main.GetComponent<CameraShake>().ShakeTo(
            GetClampedRecoilRot(-5),
            0.05f,
            _identity.Data.RecoilRecoveryDuration.Evaluate(1));
        // _cRecoilRecovery = StartCoroutine(RecoilRecovery());
        StartCoroutine(FireReady());
    }
    public override void FireStop()
    {
        return;
    }
    private IEnumerator FireReady()
    {
        yield return new WaitForSeconds(_identity.Data.FireDelay);
        SetThingsByScopeLevel(_scopeLevel);
        _recoilValue = 0;
        _isFiring = false;
    }
    public override void StartReload()
    {
        base.StartReload();
        _scopeLevel = 0;
        SetThingsByScopeLevel(0);
    }
    public override void SetInspect(bool inspect)
    {
        base.SetInspect(inspect);
        if (inspect)
        {
            _scopeLevel = 0;
            SetThingsByScopeLevel(0);
        }
    }
}
