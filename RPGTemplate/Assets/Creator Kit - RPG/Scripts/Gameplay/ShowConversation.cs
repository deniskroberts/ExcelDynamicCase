﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ExcelDomain;
using Assets.Creator_Kit___RPG.Persistence;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;
using UnityEngine;
using System.Linq;
using ExcelUnityPipeline;
using Assets.Creator_Kit___RPG.Logic;
using Assets.Creator_Kit___RPG.Scripts.UI;
using Unity.VisualScripting;

namespace RPGM.Events
{
    /// <summary>
    /// This event will start a conversation with an NPC using a conversation script.
    /// </summary>
    /// <typeparam name="ShowConversation"></typeparam>
    public class ShowConversation : Event<ShowConversation>
    {
        public static NPCController LastNpc;

        public NPCController npc;
        public GameObject gameObject;
        public ConversationScript conversation;
        public string conversationItemKey;

        public override void Execute()
        {
            LastNpc = npc;
            ConversationPiece ci;
            //default to first conversation item if no key is specified, else find the right conversation item.
            if (string.IsNullOrEmpty(conversationItemKey))
                ci = conversation.items[0];
            else
                ci = conversation.Get(conversationItemKey);

            if (!string.IsNullOrEmpty(ci.unlockedFunction))
            {
                SaveManager.SaveUnlockedFunction(ci.unlockedFunction);
            }

            if (!string.IsNullOrEmpty(ci.tag))
            {
                TaggingManager.ProcessTag(ci.tag, false);
            }

            string textToSpeak = ci.text;

            if (ci.text == "REWARD")
            {
                textToSpeak = string.Join(", ", SaveManager.GetFunctionsToUnlock(npc.rewardClassification).OrderBy(x => x));
            }

            if (ci.text == "BATTLE")
            {
                GameObject gameObject = GameObject.Find("PipeBootstrap");
                GameObject musicGameObject = GameObject.Find("Controllers");
                MusicController musicController = musicGameObject.GetComponent<MusicController>();
                musicController.CrossFadeIntoBattle(npc.audioClip);

                PipeBootstrap pipeBootstrap = gameObject.GetComponent<PipeBootstrap>();

                BattleParameters battleParameters = BattleManager.GetBattleParameters(npc.gameObject.name, npc.rewardClassification, out float minutes);

                CountdownTimer countdownTimer = GameObject.FindObjectOfType<CountdownTimer>(true);

                countdownTimer.Show(gameObject.transform.position, minutes);

                npc.StartCoroutine(ExecuteBattleRoutine(pipeBootstrap, battleParameters));
            }

            //if this item contains an unstarted quest, schedule a start quest event for the quest.
            if (ci.quest != null)
            {
                if (!ci.quest.isStarted)
                {
                    var ev = Schedule.Add<StartQuest>(1);
                    ev.quest = ci.quest;
                    ev.npc = npc;
                }
                if (ci.quest.isFinished && ci.quest.questCompletedConversation != null)
                {
                    ci = ci.quest.questCompletedConversation.items[0];
                }
            }

            //calculate a position above the player's sprite.
            var position = gameObject.transform.position;
            var sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                position += new Vector3(0, 2 * sr.size.y + (ci.options.Count == 0 ? 0.1f : 0.2f), 0);
            }

            //show the dialog
            model.dialog.Show(position, textToSpeak);
            var animator = gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("Talk", true);
                var ev = Schedule.Add<StopTalking>(2);
                ev.animator = animator;
            }

            if (ci.audio != null)
            {
                UserInterfaceAudio.PlayClip(ci.audio);
            }

            //speak some gibberish at two speech syllables per word.
            //UserInterfaceAudio.Speak(gameObject.GetInstanceID(), ci.text.Split(' ').Length * 2, 1);

            //if this conversation item has an id, register it in the model.
            if (!string.IsNullOrEmpty(ci.id))
                model.RegisterConversation(gameObject, ci.id);

            //setup conversation choices, if any.
            if (ci.options.Count == 0)
            {
                //do nothing
            }
            else
            {
                //Create option buttons below the dialog.
                for (var i = 0; i < ci.options.Count; i++)
                {
                    model.dialog.SetButton(i, ci.options[i].text);
                }

                //if user pickes this option, schedule an event to show the new option.
                model.dialog.onButton += (index) =>
                {
                    //hide the old text, so we can display the new.
                    model.dialog.Hide();

                    //This is the id of the next conversation piece.
                    var next = ci.options[index].targetId;

                    //Make sure it actually exists!
                    if (conversation.ContainsKey(next))
                    {
                        //find the conversation piece object and setup a new event with correct parameters.
                        var c = conversation.Get(next);
                        var ev = Schedule.Add<ShowConversation>(0.25f);
                        ev.conversation = conversation;
                        ev.gameObject = gameObject;
                        ev.conversationItemKey = next;
                    }
                    else
                    {
                        Debug.LogError($"No conversation with ID:{next}");
                    }
                };

            }

            //if conversation has an icon associated, this will display it.
            model.dialog.SetIcon(ci.image);
        }

        private IEnumerator ExecuteBattleRoutine(PipeBootstrap pipeBootstrap, BattleParameters battleParameters)
        {
            // 1.  Tell whatever system you use to stop processing input.
            model.input.ChangeNonBattleState(InputController.State.Battle);

            // 2.  Kick off the pipeline *without* blocking.
            var task = pipeBootstrap.RunBattle(battleParameters);

            // 3.  Yield until it completes.
            while (!task.IsCompleted)
                yield return null;

            // 4.  Handle success / failure.
            if (task.IsFaulted)
                Debug.LogException(task.Exception);   // make the error visible
        }
    }
}