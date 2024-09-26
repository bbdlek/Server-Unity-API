using System.Collections;
using System.Collections.Generic;
using MVS.Realtime;
using UnityEngine;

public class TestGroupManager : Singleton<TestGroupManager>, IMakingGroupCallbacks
{
    public void OnClickGroupJoinBtn()
    {
        // TestManager.Instance.Client.OpJoinGroup(1, 1);
    }
    
    public void OnCreatedGroup()
    {
        Debug.Log("OnCreatedGroup");
    }

    public void OnCreatedGroupFailed(string message)
    {
        
    }

    public void OnJoinedGroup()
    {
        Debug.Log("OnJoinedGroup");
    }

    public void OnJoinedGroupFailed(string message)
    {
        
    }

    public void OnLeftGroup()
    {
        
    }
}
