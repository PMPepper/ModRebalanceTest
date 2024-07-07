using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace ModRebalanceTest.src.Data.Scripts.ModRebalanceTest
{
    public static class Utils
    {
        /*public static void ClientDebug(string msg)
        {
            if (Constants.IsClient && Settings.Debug)
            {
                MyAPIGateway.Utilities.ShowMessage("[[BSCS]]: ", msg);
            }
        }*/

        public static void ShowNotification(string msg, int disappearTime = 10000, string font = MyFontEnum.Red)
        {
            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification(msg, disappearTime, font);
        }

        public static void WriteToClient(string msg)
        {
            if (Constants.IsClient)
            {
                MyAPIGateway.Utilities.ShowMessage("[MRT]: ", msg);
            }
        }

        public static void Log(string msg, int logPriority = 0)
        {
            if (logPriority >= Settings.LOG_LEVEL)
            {
                MyLog.Default.WriteLine($"[MRT]: {msg}");
            }

            if(logPriority >= Settings.CLIENT_OUTPUT_LOG_LEVEL)
            {
                MyAPIGateway.Utilities.ShowMessage($"[MRT={logPriority}]: ", msg);
            }
        }

        public static void LogException(Exception e)
        {
            Log($"Exception message = {e.Message}, Stack trace:\n{e.StackTrace}", 3);
        }

        public static void SaveConfig<T>(string variableId, string filename, T data)
        {
            string saveText = MyAPIGateway.Utilities.SerializeToXML(data);

            MyAPIGateway.Utilities.SetVariable(variableId, saveText);

            Log($"Saving config file to: {filename}", 0);

            using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(string)))
            {
                file.Write(saveText);
            }
        }

        public static string GetBlockId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.TypeId).Replace("MyObjectBuilder_", "");
        }

        public static string GetBlockSubtypeId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.SubtypeId);
        }


        public static T[] ConcatArrays<T>(params T[][] p)
        {
            var position = 0;
            var outputArray = new T[p.Sum(a => a.Length)];
            foreach (var curr in p)
            {
                Array.Copy(curr, 0, outputArray, position, curr.Length);
                position += curr.Length;
            }

            return outputArray;
        }
    }
}
