using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.Network;
using VRage.Utils;

namespace ModRebalanceTest.src.Data.Scripts.ModRebalanceTest
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    public class ModRebalanceTestSession : MySessionComponentBase
    {
        public static ModRebalanceTestSession Instance; // the only way to access session comp from other classes and the only accepted static field.
        
        private MyEntity LastControlledEntity = null;

        public override void LoadData()
        {
            // amongst the earliest execution points, but not everything is available at this point.

            // These can be used anywhere, not just in this method/class:
            // MyAPIGateway. - main entry point for the API
            // MyDefinitionManager.Static. - reading/editing definitions
            // MyGamePruningStructure. - fast way of finding entities in an area
            // MyTransparentGeometry. and MySimpleObjectDraw. - to draw sprites (from TransparentMaterials.sbc) in world (they usually live a single tick)
            // MyVisualScriptLogicProvider. - mainly designed for VST but has its uses, use as a last resort.
            // System.Diagnostics.Stopwatch - for measuring code execution time.
            // ...and many more things, ask in #programming-modding in keen's discord for what you want to do to be pointed at the available things to use.

            

            if (MyAPIGateway.Utilities.IsDedicated)
            {
                return;
            }

            Instance = this;
        }

        private void onRegisteredCallback()
        {
            //throw new NotImplementedException();
            //HUDTextAPI.
        }

        public override void BeforeStart()
        {
            Utils.WriteToClient("Init ModRebalanceTest");
            // executed before the world starts updating

            //TODO load this from a config file
            var blueprintsToDisable = new String[] {
                "R75ammo",
                "R150ammo",
                "R250ammo",
                "H203Ammo",
                "H203AmmoAP",
                "C30Ammo",
                "C30DUammo",
                "CRAM30mmAmmo",
                "C100mmAmmo",
                "C300AmmoAP",
                "C300AmmoHE",
                "C300AmmoG",
                "C400AmmoAP",
                "C400AmmoHE",
                "C400AmmoCluster",
                "C500AmmoAP",
                "C500AmmoHE",
                "C500AmmoCasaba",
                "PlasmaCell10MJ",
            };

            var definitionsToDisable = new String[] {
                "LargeMissileTurret/CIWS",
                "LargeMissileTurret/C30mmSingleT",
                "LargeMissileTurret/C30mmSingleTSG",
                "LargeMissileTurret/C100mmTurret",
                "LargeMissileTurret/C200mmTurret",
                "LargeMissileTurret/C300mmTurret",
                "LargeMissileTurret/C400mmTurret",
                "ConveyorSorter/C500mmTurret",
                "LargeMissileTurret/C203mmSingleTurret",
                "LargeMissileTurret/H203mmSingleTurret",
                "SmallMissileLauncher/Railgunx",
                "ConveyorSorter/RailgunxTurretS",
                "LargeMissileTurret/RailgunxTurretM",
                "LargeMissileTurret/RailgunxTurret",
                "ConveyorSorter/RailgunxTurret",
                "LargeMissileTurret/AMSlaser",
                "SmallMissileLauncher/RotaryCannon",
                "SmallMissileLauncher/203mmHowitzer",
                "SmallMissileLauncher/C30mmRevolver",
                "SmallMissileLauncher/Railgunx75f",
                "SmallMissileLauncher/Railgunx150f",
                "SmallMissileLauncher/TankPlasmaCannon",
            };

            foreach (var str in blueprintsToDisable)
            {
                string idStr = $"MyObjectBuilder_BlueprintDefinition/{str}";
                MyDefinitionId id;
                try
                {
                    id = MyDefinitionId.Parse(idStr);
                }
                catch (Exception e)
                {
                    Utils.Log($"Failed to parse: {idStr}", 2);
                    Utils.LogException(e);
                    continue;
                }

                var bp = MyDefinitionManager.Static.GetBlueprintDefinition(id);

                if(bp != null)
                {
                    bp.Public = false;
                }
                else
                {
                    Utils.Log($"Failed to find blueprint for: {idStr}", 3);
                }
            }

            foreach (var str in definitionsToDisable)
            {
                string idStr = $"MyObjectBuilder_{str}";
                MyDefinitionId id;
                try
                {
                    id = MyDefinitionId.Parse(idStr);
                }
                catch (Exception e)
                {
                    Utils.Log($"Failed to parse: {idStr}", 2);
                    Utils.LogException(e);
                    continue;
                }

                var definition = MyDefinitionManager.Static.GetDefinition(id);

                if (definition != null)
                {
                    definition.Public = false;
                } else
                {
                    Utils.Log($"Failed to find definition for: {idStr}", 3);
                }
            }
        }

        protected override void UnloadData()
        {
            // executed when world is exited to unregister events and stuff

            Instance = null; // important for avoiding this object to remain allocated in memory
        }

        /*public override void HandleInput()
        {
            // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        }*/

        /*public override void UpdateBeforeSimulation()
        {
            // executed every tick, 60 times a second, before physics simulation and only if game is not paused.
        }*/

        /*public override void Simulate()
        {
            // executed every tick, 60 times a second, during physics simulation and only if game is not paused.
            // NOTE in this example this won't actually be called because of the lack of MyUpdateOrder.Simulation argument in MySessionComponentDescriptor
        }*/

        public override void UpdateAfterSimulation()
        {
            // executed every tick, 60 times a second, after physics simulation and only if game is not paused.

            if (Constants.IsClient)
            {
                // Existing code for controlled entities and predictions
                MyEntity controlledEntity = GetControlledGrid();
                MyEntity cockpitEntity = GetControlledCockpit(controlledEntity);

                if (controlledEntity != LastControlledEntity)
                {
                    //controlled entity has changed
                    //Record new controlled entity
                    LastControlledEntity = controlledEntity;
                }
            }
        }

        /*public override void Draw()
        {
            // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
            // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.
        }*/

        /*public override void SaveData()
        {
            // executed AFTER world was saved
        }*/

        /*public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            // executed during world save, most likely before entities.

            return base.GetObjectBuilder(); // leave as-is.
        }*/

        /*public override void UpdatingStopped()
        {
            // executed when game is paused
        }*/

        public bool IsControlledEntity(IMyEntity entity)
        {
            return entity == LastControlledEntity;
        }


        public static MyEntity GetControlledGrid()
        {
            try
            {
                if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                {
                    return null;
                }

                var controlledEntity = MyAPIGateway.Session.Player.Controller?.ControlledEntity?.Entity;
                if (controlledEntity == null)
                {
                    return null;
                }

                if (controlledEntity is IMyCockpit || controlledEntity is IMyRemoteControl)
                {
                    return (controlledEntity as IMyCubeBlock).CubeGrid as MyEntity;
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Error in GetControlledGrid: {e}");
            }

            return null;
        }

        public static MyEntity GetControlledCockpit(MyEntity controlledGrid)
        {
            if (controlledGrid == null)
                return null;

            var grid = controlledGrid as MyCubeGrid;
            if (grid == null)
                return null;

            foreach (var block in grid.GetFatBlocks())
            {
                var cockpit = block as MyCockpit; // Convert the block to MyCockpit
                if (cockpit != null)
                {
                    if (cockpit.WorldMatrix != null)  // Add null check here
                        return cockpit;
                }
            }
            return null;
        }
    }
}
