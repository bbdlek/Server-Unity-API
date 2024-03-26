using MVS.Realtime;

namespace MVS.Helios
{
    public class CustomVariablesUnity
    {
        internal static void Register()
        {
            CustomVariables.TryRegisterVariable(1, "position");
            CustomVariables.TryRegisterVariable(2, "rotation");
            CustomVariables.TryRegisterVariable(3, "scale");
            HeliosNetwork.RealtimeClient.MVSDebug(DebugLevel.INFO, "Register Unity");
        }
    }
}