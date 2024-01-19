﻿using HarmonyLib;
using Vehicles;
using Verse;

namespace taranchuk_flightcombat
{
    [HarmonyPatch(typeof(VehiclePathing), nameof(VehiclePathing.GotoLocationVehicles))]
    public static class VehiclePathing_GotoLocationVehicles_Patch
    {
        public static bool Prefix(IntVec3 __0, Pawn __1, ref bool __result)
        {
            if (__1 is VehiclePawn vehicle)
            {
                var comp = vehicle.GetComp<CompFlightMode>();
                if (comp != null && comp.InAir)
                {
                    comp.SetTarget(__0);
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}