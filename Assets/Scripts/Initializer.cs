using UnityEngine;

public static class Initializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void InitializeDisplays()
    {
        if (Display.displays.Length < 2)
        {
            Debug.LogError("Could not initialize second display");
            return;
        }

        var display = Display.displays[1];
        display.Activate();
    }
    dlaskfjalnf
}
