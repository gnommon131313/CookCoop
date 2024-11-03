using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace UIMain.UIGame
{
    internal sealed class Hint : UIMain
    {
        [SerializeField] private TextMeshProUGUI _player0InputText;
        [SerializeField] private TextMeshProUGUI _player1InputText;
        [SerializeField] private TextMeshProUGUI _player2InputText;
        [SerializeField] private TextMeshProUGUI _player3InputText;
        [SerializeField] private TextMeshProUGUI _generalInputText;

        private Hint() { }

        [Inject]
        private void Construct()
        {
        }

        protected override void Start()
        {
            base.Start();



            //_player0InputText.text = $"" +
            //    $"({_inputHandler.InputControls.Player0.Axis.bindings[0].name})-move" +
            //    $"({_inputHandler.InputControls.Player0.Action0.bindings[0].path})-take" +
            //    $"({_inputHandler.InputControls.Player0.Action1.bindings[0].effectivePath})-pullout" +
            //    $"({_inputHandler.InputControls.Player0.Action2.bindings[0].effectivePath})-use";

            //Debug.Log("! " + _inputHandler.InputControls.Player0.Axis.bindings[0].effectivePath);
            



            //var action = _inputHandler.InputControls.Player0.Axis;

            //foreach (var binding in action.bindings)
            //{
            //    //if(binding as Keyboard)
            //    Debug.Log("Binding: " + binding.path);
            //}
        }
    }
}
