using MVS.Realtime;

namespace MVS.Helios
{
    public class CustomVariablesUnity
    {
        public static int PosKey = 0;
        public static int RotKey = 1;
        public static int ScaleKey = 2;
        
        internal static void Register()
        {
            CustomVariables.TryRegisterVariable(PosKey, "position");
            CustomVariables.TryRegisterVariable(RotKey, "rotation");
            CustomVariables.TryRegisterVariable(ScaleKey, "scale");
            HeliosNetwork.RealtimeClient.MVSDebug(DebugLevel.INFO, "Register Unity");
        }
    }
}