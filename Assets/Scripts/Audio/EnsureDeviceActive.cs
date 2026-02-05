using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace dungeonduell
{
    public class EnsureDeviceActive : MonoBehaviour // Fail Safe
    {
        void Start()
        {
            foreach (PlayerInput playerInput in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            {
                try
                {
                    InputSystem.EnableDevice(playerInput.user.pairedDevices[0]);
                    if (playerInput.user.pairedDevices[0] is Mouse | playerInput.user.pairedDevices[0] is Keyboard)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        InputSystem.EnableDevice(Mouse.current);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}