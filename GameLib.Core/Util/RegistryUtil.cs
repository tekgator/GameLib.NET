using Microsoft.Win32;

namespace Gamelib.Core.Util;

public static class RegistryUtil
{
    /// <summary>
    /// Gets the executable of the registered shell command application
    /// </summary>
    /// <returns>The path of the shell command executable; <see langword="null"/> if not found</returns>
    public static string? GetShellCommand(string shellCommand)
    {
        string? executablePath = null;

        executablePath ??= GetValue(RegistryHive.CurrentUser, $@"Software\Classes\{shellCommand}\shell\open\command", null);
        executablePath ??= GetValue(RegistryHive.LocalMachine, $@"Software\Classes\{shellCommand}\shell\open\command", null);
        executablePath ??= GetValue(RegistryHive.ClassesRoot, $@"{shellCommand}\Shell\Open\Command", null);

        executablePath = PathUtil.RemoveArgsFromExecutable(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }

    /// <summary>
    /// Gets the registry key for the passed Hive and KeyName
    /// Firstly it checks in the 32bit environment, if no key found in the 64bit environment
    /// </summary>
    /// <returns>The respective <see cref="RegistryKey"/>; <see langword="null"/> if not found</returns>
    public static RegistryKey? GetKey(RegistryHive hive, string keyName, bool checkSubKeyCount = false)
    {
        using var baseKey32 = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
        var regKey32 = baseKey32.OpenSubKey(keyName);

        if (checkSubKeyCount && regKey32?.SubKeyCount > 0)
            return regKey32;

        if (!checkSubKeyCount && regKey32 is not null)
            return regKey32;

        regKey32?.Dispose();

        using var baseKey64 = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        var regKey64 = baseKey64.OpenSubKey(keyName);

        if (checkSubKeyCount && regKey64?.SubKeyCount > 0)
            return regKey64;

        if (!checkSubKeyCount && regKey64 is not null)
            return regKey64;

        regKey64?.Dispose();

        return null;
    }

    /// <summary>
    /// Gets the value of the passed in registry key
    /// <seealso cref="GetKey(RegistryHive, string, bool)"/>
    /// </summary>
    /// <returns>
    /// The value of the registry key as a <see langword="string"/><br/>
    /// Otherwise the passed in default value string is returned
    /// </returns>
    public static string? GetValue(RegistryHive hive, string keyName, string? valueName, string? defaultValue = null)
    {
        using var regKey = GetKey(hive, keyName);
        return regKey?.GetValue(valueName) as string ?? defaultValue;
    }
}
