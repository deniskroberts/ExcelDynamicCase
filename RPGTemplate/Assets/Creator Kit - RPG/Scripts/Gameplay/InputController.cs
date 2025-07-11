using Assets.Creator_Kit___RPG.Persistence;
using Assets.Creator_Kit___RPG.Scripts.Gameplay;
using Assets.Creator_Kit___RPG.Scripts.UI;
using ExcelUnityPipeline;
using RPGM.Core;
using RPGM.Gameplay;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGM.UI
{
    /// <summary>
    /// Sends user input to the correct control systems.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        public float stepSize = 0.1f;
        GameModel model = Schedule.GetModel<GameModel>();

        public enum State
        {
            CharacterControl,
            DialogControl,
            FunctionInventoryControl,
            Battle,
            Pause
        }

        State state;

        public void ChangeNonBattleState(State state)
        {
            if (this.state == State.Battle)
            {
                return;
            }

            this.state = state;
        }

        public void EndBattleState(BattleResult battleResult)
        {
            CountdownTimer countdownTimer = GameObject.FindObjectOfType<CountdownTimer>();
            countdownTimer.Hide();

            NPCController npcController = Events.ShowConversation.LastNpc;

            string[] unlockedFunctions = null;
            string[] deletedFunctions = null;

            if (battleResult.IsSuccess)
            {
                unlockedFunctions = SaveManager.UnlockRandomFunctions(npcController.rewardClassification);
            }
            else
            {
                deletedFunctions = SaveManager.LockRandomFunctions();
            }

            Events.ShowConversation ev = Schedule.Add<Events.ShowConversation>();

            // Grab (or auto-create) the reusable ConversationScript
            var convo = ConversationHost.Instance;

            // Overwrite its data for this one-off dialogue
            convo.items = new List<ConversationPiece>
            {
                new ConversationPiece
                {
                    id = "None",
                    text = battleResult.IsSuccess
                           ? (unlockedFunctions?.Any() is true ? $"You won and unlocked the following functions!\r\n{string.Join(", ", unlockedFunctions)}" : "You won, but no new unlocks this time!")
                           : (deletedFunctions?.Any() is true ? $"You lost and gave away the following functions!\r\n{string.Join(", ", deletedFunctions)}" : "You lost, no unlocks this time!"),
                    options = new List<ConversationOption>()
                }
            };

            ev.conversation = convo;
            ev.npc = npcController;
            ev.gameObject = npcController.gameObject;
            ev.conversationItemKey = string.Empty;

            this.state = State.CharacterControl;
        }

        void Update()
        {
            switch (state)
            {
                case State.CharacterControl:
                    CharacterControl();
                    break;
                case State.DialogControl:
                    DialogControl();
                    break;
                case State.FunctionInventoryControl:
                    FunctionInventoryControl();
                    break;
                case State.Battle:
                    BattleControl();
                    break;
            }
        }

        void DialogControl()
        {
            model.player.nextMoveCommand = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                model.dialog.FocusButton(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                model.dialog.FocusButton(+1);
            if (Input.GetKeyDown(KeyCode.Space))
                model.dialog.SelectActiveButton();
        }

        void BattleControl()
        {
            model.player.nextMoveCommand = Vector3.zero;
        }

        void CharacterControl()
        {
            if (Input.GetKey(KeyCode.UpArrow))
                model.player.nextMoveCommand = Vector3.up * stepSize;
            else if (Input.GetKey(KeyCode.DownArrow))
                model.player.nextMoveCommand = Vector3.down * stepSize;
            else if (Input.GetKey(KeyCode.LeftArrow))
                model.player.nextMoveCommand = Vector3.left * stepSize;
            else if (Input.GetKey(KeyCode.RightArrow))
                model.player.nextMoveCommand = Vector3.right * stepSize;
            else
                model.player.nextMoveCommand = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.M))
            {
                model.functionInventory.Show(model.player.gameObject.transform.position, "Press M to close");
            }
        }

        void FunctionInventoryControl()
        {
            model.player.nextMoveCommand = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.M))
            {
                model.functionInventory.Hide();
                ChangeNonBattleState(State.CharacterControl);
            }
        }
    }
}