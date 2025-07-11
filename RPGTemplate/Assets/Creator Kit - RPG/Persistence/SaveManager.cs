﻿using Assets.ExcelDomain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Creator_Kit___RPG.Persistence
{
    public static class SaveManager
    {
        public const string NOFUNCTIONAVAILABLE = "NOTHING";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "SaveData.json");

        public static bool SaveGame(SaveData data)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Serialize and write
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                return false;
            }
        }

        public static void SaveTag(string tag)
        {
            LoadGameData(out SaveData saveData);
            saveData.Tags.Add(tag);
            saveData.Tags = saveData.Tags.Distinct().ToList();
            SaveGame(saveData);
        }

        public static void SaveUnlockedFunction(string functionNames)
        {
            string[] functions = functionNames.Split(' ');


            LoadGameData(out SaveData saveData);
            saveData.UnlockedFunctions.AddRange(functions);
            saveData.UnlockedFunctions = saveData.UnlockedFunctions.Distinct().OrderBy(x => x).ToList();

            SaveGame(saveData);
        }

        public static void SaveCompletedBattle(int questionId, bool pureAttempt)
        {
            LoadGameData(out SaveData saveData);

            saveData.CompletedQuestions.Add(questionId);
            saveData.CompletedQuestions = saveData.CompletedQuestions.Distinct().OrderBy(x => x).ToList();


            if (pureAttempt)
            {
                saveData.PureCompletedQuestions.Add(questionId);
                saveData.PureCompletedQuestions = saveData.PureCompletedQuestions.Distinct().OrderBy(x => x).ToList();
            }

            SaveGame(saveData);
        }

        public static void SaveUnlockedFunctions(string[] functions, string[] functionsToKill = null)
        {
            LoadGameData(out SaveData saveData);

            List<string> unlockedFunctions = saveData.UnlockedFunctions;
            unlockedFunctions.AddRange(functions);

            if (functionsToKill is null)
            {
                unlockedFunctions = unlockedFunctions.Distinct().OrderBy(x => x).ToList();
            }
            else
            {
                unlockedFunctions = unlockedFunctions.Distinct().Where(x => !functionsToKill.Contains(x)).OrderBy(x => x).ToList();
            }

            unlockedFunctions.Sort();

            saveData.UnlockedFunctions = unlockedFunctions;

            SaveGame(saveData);
        }

        public static string[] UnlockRandomFunctions(QuestionRewardClassification questionRewardClassification)
        {
            string[] allCandidateUnlocks = QuestionRewardsDatabase.FunctionRewardsByClassification[questionRewardClassification];

            string[] lockedCandidateUnlocks = GetFunctionsToUnlock(questionRewardClassification);

            List<string> selectedFunctions = new();

            for (int i = 0; i < 3; i++)
            {
                if (UnityEngine.Random.Range((int)0, 2) == 0 || (lockedCandidateUnlocks.Length == 1 && lockedCandidateUnlocks.First() == NOFUNCTIONAVAILABLE))
                {
                    selectedFunctions.Add(allCandidateUnlocks[UnityEngine.Random.Range((int)0, allCandidateUnlocks.Length)]);
                }
                else
                {
                    selectedFunctions.Add(lockedCandidateUnlocks[UnityEngine.Random.Range((int)0, lockedCandidateUnlocks.Length)]);
                }
            }

            selectedFunctions = selectedFunctions.Distinct().ToList();

            string[] validFunctionsForUnlock = GetFunctionsToUnlock(questionRewardClassification);

            string[] winners = selectedFunctions.Where(x => validFunctionsForUnlock.Contains(x)).ToArray();

            SaveUnlockedFunctions(winners);

            return winners;
        }

        public static string[] LockRandomFunctions()
        {
            LoadGameData(out SaveData saveData);

            List<string> selectedFunctions = new();

            string[] functionsInKillPool = saveData.UnlockedFunctions.Where(x => !x.Contains("RAND") && !x.Contains("BESSEL") && x != "VLOOKUP" && x != "INDEX" && x != "XLOOKUP").ToArray();

            if (!functionsInKillPool.Any())
            {
                return selectedFunctions.ToArray();
            }

            for (int i = 0; i < 1; i++)
            {
                selectedFunctions.Add(functionsInKillPool[UnityEngine.Random.Range((int)0, functionsInKillPool.Length)]);
            }

            selectedFunctions = selectedFunctions.Distinct().ToList();

            SaveUnlockedFunctions(Array.Empty<string>(), selectedFunctions.ToArray());

            return selectedFunctions.ToArray();
        }

        public static string[] GetFunctionsToUnlock(QuestionRewardClassification questionRewardClassification)
        {
            LoadGameData(out SaveData data);

            string[] functionRewards = QuestionRewardsDatabase.FunctionRewardsByClassification[questionRewardClassification];

            string[] unlockableRewards = functionRewards.Where(x => !data.UnlockedFunctions.Contains(x)).ToArray();

            if (unlockableRewards.Any())
            {
                return unlockableRewards;
            }

            return new string[1] { NOFUNCTIONAVAILABLE };
        }

        public static void LoadGameData(out SaveData data)
        {
            data = new SaveData();

            if (!File.Exists(SaveFilePath))
            {
                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveData startingSaveData = new()
                {
                    Tags = new(),
                    UnlockedFunctions = new() { }
                };

                string json = JsonUtility.ToJson(startingSaveData, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                data = JsonUtility.FromJson<SaveData>(json);
                data.UnlockedFunctions ??= new();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
            }
        }
    }
}
