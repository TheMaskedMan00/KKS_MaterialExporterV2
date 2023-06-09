﻿using HarmonyLib;
using Studio;
using System.Collections;
#if AI || HS2
using AIChara;
#endif

namespace KK_Plugins
{
    public partial class FKIK
    {
        internal static class Hooks
        {
            private static bool ChangingChara;

            private static int ChangingCharaNeckPtn;

            /// <summary>
            /// Enable simultaneous kinematics on pose load
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(PauseCtrl.FileInfo), nameof(PauseCtrl.FileInfo.Apply))]
            private static void Apply(PauseCtrl.FileInfo __instance, OCIChar _char)
            {
                if (__instance.enableFK && __instance.enableIK)
                    EnableFKIK(_char);
            }

            /// <summary>
            /// Enable simultaneous kinematics on character load. Pass the FK/IK state to the postfix
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(AddObjectFemale), nameof(AddObjectFemale.Add), typeof(ChaControl), typeof(OICharInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject), typeof(bool), typeof(int))]
            private static void AddObjectFemalePrefix(OICharInfo _info, ref bool[] __state)
            {
                __state = new bool[2];
                __state[0] = _info.enableFK && _info.enableIK;
                __state[1] = _info.activeFK[6];
            }

            /// <summary>
            /// FK/IK state has been overwritten, check against the FK/IK state from prefix
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(AddObjectFemale), nameof(AddObjectFemale.Add), typeof(ChaControl), typeof(OICharInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject), typeof(bool), typeof(int))]
            private static void AddObjectFemalePostfix(OICharInfo _info, ChaControl _female, ref bool[] __state)
            {
                if (__state[0])
                {
                    EnableFKIK(_female);
                    if (__state[1])
                        _info.activeFK[6] = true;
                }
            }

            /// <summary>
            /// Enable simultaneous kinematics on character load. Pass the FK/IK state to the postfix
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(AddObjectMale), nameof(AddObjectMale.Add), typeof(ChaControl), typeof(OICharInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject), typeof(bool), typeof(int))]
            private static void AddObjectMalePrefix(OICharInfo _info, ref bool[] __state)
            {
                __state = new bool[2];
                __state[0] = _info.enableFK && _info.enableIK;
                __state[1] = _info.activeFK[6];
            }

            /// <summary>
            /// FK/IK state has been overwritten, check against the FK/IK state from prefix
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(AddObjectMale), nameof(AddObjectMale.Add), typeof(ChaControl), typeof(OICharInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject), typeof(bool), typeof(int))]
            private static void AddObjectMalePostfix(OICharInfo _info, ChaControl _male, ref bool[] __state)
            {
                if (__state[0])
                {
                    EnableFKIK(_male);
                    if (__state[1])
                        _info.activeFK[6] = true;
                }
            }

            /// <summary>
            /// Set a flag when changing characters in Studio
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ChangeChara))]
            private static void ActiveKinematicMode(OCIChar __instance)
            {
                ChangingChara = true;
                ChangingCharaNeckPtn = __instance != null && __instance.neckLookCtrl != null ? __instance.neckLookCtrl.ptnNo : -1;
            }

            /// <summary>
            /// Enable simultaneous kinematics on character change. Pass the FK/IK state to the postfix
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
            private static void ActiveKinematicModePrefix(OCIChar __instance, ref bool __state) => __state = __instance.oiCharInfo.enableFK && __instance.oiCharInfo.enableIK;

            /// <summary>
            /// FK/IK state has been overwritten, check against the FK/IK state from prefix
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
            private static void ActiveKinematicModePostfix(OCIChar __instance, ref bool __state)
            {
                if (__state && ChangingChara)
                    Instance.StartCoroutine(EnableFKIKCoroutine(__instance));
            }

            private static IEnumerator EnableFKIKCoroutine(OCIChar ociChar)
            {
                yield return null;
                ChangingChara = false;
                if (ChangingCharaNeckPtn != -1)
                    ociChar.ChangeLookNeckPtn(ChangingCharaNeckPtn);
                ChangingCharaNeckPtn = -1;
                EnableFKIK(ociChar);
            }

            [HarmonyPostfix, HarmonyPatch(typeof(MPCharCtrl.IKInfo), nameof(MPCharCtrl.IKInfo.Init))]
            private static void IKInfo_Init() => UI.InitUI();

            [HarmonyPostfix, HarmonyPatch(typeof(MPCharCtrl.IKInfo), nameof(MPCharCtrl.IKInfo.UpdateInfo))]
            private static void IKInfo_UpdateInfo(OCIChar _char) => UI.UpdateUI(_char);
        }
    }
}
