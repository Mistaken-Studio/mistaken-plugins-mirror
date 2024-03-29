﻿using Exiled.API.Features;
using System;

namespace Gamer.Utilities
{
    [System.Obsolete("Use Module Logger")]
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
    public static class Logger
    {
        public static void Debug(string tag, string message)
        {
            Log.Debug($"[{tag}] {message}");
        }

        public static void Info(string tag, string message)
        {
            Log.Info($"[{tag}] {message}");
        }

        public static void Warn(string tag, string message)
        {
            Log.Warn($"[{tag}] {message}");
        }

        public static void Error(string tag, string message)
        {
            Log.Error($"[{tag}] {message}");
        }

        public static void Send(string message, Discord.LogLevel level, ConsoleColor color = ConsoleColor.Gray)
        {
            Log.Send(message, level, color);
        }

        public static void Send(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            Log.SendRaw(message, color);
        }
    }
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
}
