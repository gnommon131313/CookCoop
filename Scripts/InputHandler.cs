using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

internal sealed class InputHandler : IInitializable, IDisposable
{
    private GameData _gameData;

    internal readonly InputControls InputControls = new();

    internal readonly ReactiveCommand Restart = new();
    internal readonly ReactiveCommand Menu = new();

    internal readonly List<ReactiveProperty<Vector3>> PlayersAxis = new();
    internal readonly List<ReactiveCommand> PlayersAction0 = new();
    internal readonly List<ReactiveCommand> PlayersAction1 = new();
    internal readonly List<ReactiveProperty<bool>> PlayersAction2 = new();

    private InputHandler(GameData gameData)
    {
        _gameData = gameData;
    }

    public void Initialize()
    {
        InputControls.Enable();

        void SubscribeToInput()
        {
            void ToGeneral()
            {
                InputControls.General.Restart.started += evt => Restart.Execute();
                InputControls.General.Menu.started += evt => Menu.Execute();
            }

            void ToPlayer()
            {
                void ReservationInput()
                {
                    for (int i = 0; i < _gameData.PlayerPool.Length; i++)
                    {
                        PlayersAxis.Add(new ReactiveProperty<Vector3>());
                        PlayersAction0.Add(new ReactiveCommand());
                        PlayersAction1.Add(new ReactiveCommand());
                        PlayersAction2.Add(new ReactiveProperty<bool>());
                    }
                }

                void Subscribe()
                {
                    void ToAxis()
                    {
                        if (_gameData.PlayerPool.Length >= 1)
                        {
                            InputControls.Player0.Axis.performed += evt => PlayersAxis[0].Value = new Vector3(evt.ReadValue<Vector2>().x, 0, evt.ReadValue<Vector2>().y);
                            InputControls.Player0.Axis.canceled += evt => PlayersAxis[0].Value = Vector3.zero;
                        }
                        if (_gameData.PlayerPool.Length >= 2)
                        {
                            InputControls.Player1.Axis.performed += evt => PlayersAxis[1].Value = new Vector3(evt.ReadValue<Vector2>().x, 0, evt.ReadValue<Vector2>().y);
                            InputControls.Player1.Axis.canceled += evt => PlayersAxis[1].Value = Vector3.zero;
                        }
                        if (_gameData.PlayerPool.Length >= 3)
                        {
                            InputControls.Player2.Axis.performed += evt => PlayersAxis[2].Value = new Vector3(evt.ReadValue<Vector2>().x, 0, evt.ReadValue<Vector2>().y);
                            InputControls.Player2.Axis.canceled += evt => PlayersAxis[2].Value = Vector3.zero;
                        }
                        if (_gameData.PlayerPool.Length >= 4)
                        {
                            InputControls.Player3.Axis.performed += evt => PlayersAxis[3].Value = new Vector3(evt.ReadValue<Vector2>().x, 0, evt.ReadValue<Vector2>().y);
                            InputControls.Player3.Axis.canceled += evt => PlayersAxis[3].Value = Vector3.zero;
                        }
                    }

                    void ToAction1()
                    {
                        if (_gameData.PlayerPool.Length >= 1)
                            InputControls.Player0.Action0.started += evt => PlayersAction0[0].Execute();
                        if (_gameData.PlayerPool.Length >= 2)
                            InputControls.Player1.Action0.started += evt => PlayersAction0[1].Execute();
                        if (_gameData.PlayerPool.Length >= 3)
                            InputControls.Player2.Action0.started += evt => PlayersAction0[2].Execute();
                        if (_gameData.PlayerPool.Length >= 4)
                            InputControls.Player3.Action0.started += evt => PlayersAction0[3].Execute();
                    }

                    void ToAction2()
                    {
                        if (_gameData.PlayerPool.Length >= 1)
                            InputControls.Player0.Action1.started += evt => PlayersAction1[0].Execute();
                        if (_gameData.PlayerPool.Length >= 2)
                            InputControls.Player1.Action1.started += evt => PlayersAction1[1].Execute();
                        if (_gameData.PlayerPool.Length >= 3)
                            InputControls.Player2.Action1.started += evt => PlayersAction1[2].Execute();
                        if (_gameData.PlayerPool.Length >= 4)
                            InputControls.Player3.Action1.started += evt => PlayersAction1[3].Execute();
                    }

                    void ToAction3()
                    {
                        if (_gameData.PlayerPool.Length >= 1)
                        {
                            InputControls.Player0.Action2.performed += evt => PlayersAction2[0].Value = true;
                            InputControls.Player0.Action2.canceled += evt => PlayersAction2[0].Value = false;
                        }
                        if (_gameData.PlayerPool.Length >= 2)
                        {
                            InputControls.Player1.Action2.performed += evt => PlayersAction2[1].Value = true;
                            InputControls.Player1.Action2.canceled += evt => PlayersAction2[1].Value = false;
                        }
                        if (_gameData.PlayerPool.Length >= 3)
                        {
                            InputControls.Player2.Action2.performed += evt => PlayersAction2[2].Value = true;
                            InputControls.Player2.Action2.canceled += evt => PlayersAction2[2].Value = false;
                        }
                        if (_gameData.PlayerPool.Length >= 4)
                        {
                            InputControls.Player3.Action2.performed += evt => PlayersAction2[3].Value = true;
                            InputControls.Player3.Action2.canceled += evt => PlayersAction2[3].Value = false;
                        }
                    }

                    ToAxis();
                    ToAction1();
                    ToAction2();
                    ToAction3();
                }

                ReservationInput();
                Subscribe();
            }

            ToGeneral();
            ToPlayer();
        }

        SubscribeToInput();
    }

    public void Dispose()
    {
        InputControls.Disable();
    }

    internal void REBIND_XXXXXXXXXXXXXXX(string newPath)
    {
        InputControls.General.Restart.ApplyBindingOverride(newPath); // Нужно предавать новый путь кнопки в виде "Keyboard/[KeyName]" (например ApplyBindingOverride("Keyboard/space"))
    }
}