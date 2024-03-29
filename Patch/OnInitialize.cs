﻿using HarmonyLib;
using Notify.AutoAnnouncer;
using Notify.AutoAnnouncer.Timers;
using Notify.Helpers;
using System;

namespace Notify.Hooks;

[HarmonyPatch]
public class OnInitialize
{
#nullable disable
    private static AutoAnnouncerTimer _autoAnnouncerTimer;
#nullable enable


    private static void InvokePlugins()
    {
        Plugin.Logger.LogInfo("Game has bootstrapped. Worlds and systems now exist.");

        GameFrame.Initialize();

        DBHelper.EnabledFeatures[NotifyFeature.online] = Plugin.AnnounceOnline.Value;
        DBHelper.EnabledFeatures[NotifyFeature.offline] = Plugin.AnnounceeOffline.Value;
        DBHelper.EnabledFeatures[NotifyFeature.newuser] = Plugin.AnnounceNewUser.Value;
        DBHelper.EnabledFeatures[NotifyFeature.vblood] = Plugin.AnnounceVBlood.Value;
        DBHelper.EnabledFeatures[NotifyFeature.auto] = Plugin.AutoAnnouncerConfig.Value;
        DBHelper.EnabledFeatures[NotifyFeature.motd] = Plugin.MessageOfTheDay.Value;


        DBHelper.setVBloodFinalConcatCharacters(Plugin.VBloodFinalConcatCharacters.Value);

        DBHelper.setIntervalAutoAnnouncer(Plugin.IntervalAutoAnnouncer.Value);
        Plugin.Logger.LogInfo("GameData Init");
        _autoAnnouncerTimer = new AutoAnnouncerTimer();
        if (Plugin.AutoAnnouncerConfig.Value)
        {
            StartAutoAnnouncer();
        }
    }

    // these are intentionally different classes, even if their bodies _currently_ are the same
    [HarmonyPatch("ProjectM.GameBootstrap", "Start")]
    [HarmonyPostfix]
    public static void ServerDetours()
    {
        InvokePlugins();
    }

    public static void StartAutoAnnouncer()
    {
        _autoAnnouncerTimer.Start(
            world =>
            {
                Plugin.Logger.LogInfo("Starting AutoAnnouncer");
                AutoAnnouncerFunction.OnTimedAutoAnnouncer();
            },
            input =>
            {
                if (input is not int secondAutoAnnouncer)
                {
                    Plugin.Logger.LogError("AutoAnnouncer timer delay function parameter is not a valid integer");
                    return TimeSpan.MaxValue;
                }

                var seconds = DBHelper.getIntervalAutoAnnouncer();
                Plugin.Logger.LogInfo($"Next AutoAnnouncer will start in {seconds} seconds.");
                return TimeSpan.FromSeconds(seconds);
            });
    }

    public static void StopAutoAnnouncer()
    {
        _autoAnnouncerTimer?.Stop();
    }
}

