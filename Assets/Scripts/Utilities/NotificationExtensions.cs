using UnityEngine;
using System;
using System.Collections;
using Handler = System.Action<System.Object, System.Object>;
public static class NotificationExtensions
{
    public static void PostNotification(this object obj, Notifications notificationName)
    {
        NotificationCenter.instance.PostNotification(notificationName, obj);
    }

    public static void PostNotification(this object obj, Notifications notificationName, object e)
    {
        NotificationCenter.instance.PostNotification(notificationName, obj, e);
    }

    public static void AddObserver(this object obj, Handler handler, Notifications notificationName)
    {
        NotificationCenter.instance.AddObserver(handler, notificationName);
    }

    public static void AddObserver(this object obj, Handler handler, Notifications notificationName, object sender)
    {
        NotificationCenter.instance.AddObserver(handler, notificationName, sender);
    }

    public static void RemoveObserver(this object obj, Handler handler, Notifications notificationName)
    {
        NotificationCenter.instance.RemoveObserver(handler, notificationName);
    }

    public static void RemoveObserver(this object obj, Handler handler, Notifications notificationName, System.Object sender)
    {
        NotificationCenter.instance.RemoveObserver(handler, notificationName, sender);
    }
}