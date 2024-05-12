using MVS.Realtime;

namespace MVS.Helios
{
    public class CustomVariablesUnity
    {
        internal static void Register()
        {
            CustomVariables.TryRegisterVariable(0, "position");
            CustomVariables.TryRegisterVariable(1, "rotation");
            CustomVariables.TryRegisterVariable(2, "scale");
            HeliosNetwork.RealtimeClient.MVSDebug(DebugLevel.INFO, "Register Unity");
        }
    }
}