using System.Collections.Generic;
using System.Linq;
using MVS.Helios;

namespace MVS.Realtime
{
    public class CustomVariables
    {
        internal static readonly Dictionary<int, string> VarDic = new Dictionary<int, string>();

        internal static void Register()
        {
            HeliosNetwork.RealtimeClient.MVSDebug(DebugLevel.INFO, "Register");
        }

        public static bool TryRegisterVariable(int key, string variableName)
        {
            if (VarDic.ContainsValue(variableName) || VarDic.ContainsKey(key))
            {
                HeliosNetwork.RealtimeClient.MVSDebug(DebugLevel.ERROR, $"Key {key} or VariableName {variableName} is Already Exists. Please Change");
                return false;
            }
            VarDic.Add(key, variableName);
            return true;
        }

        public static int GetKeyByName(string variableName)
        {
            var keyValuePair = VarDic.FirstOrDefault(x => x.Value == variableName);
            
            return keyValuePair.Equals(default(KeyValuePair<int, string>)) ? -1 : keyValuePair.Key;
        }

        public static string GetNameByKey(int key)
        {
            return VarDic.ContainsKey(key) ? VarDic[key] : null;
        }
    }
}