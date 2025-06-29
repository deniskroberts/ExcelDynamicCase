using Assets.Creator_Kit___RPG.Logic;
using Assets.Creator_Kit___RPG.Persistence;
using Assets.ExcelDomain;
using RPGM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Creator_Kit___RPG.Scripts.UI
{
    [ExecuteInEditMode]
    public class FunctionInventoryLayout : MonoBehaviour
    {
        public float padding = 0.25f;
        public SpriteRenderer spriteRenderer;
        public TextMeshPro textMeshPro;
        public TMP_Text itemTextMeshPro;
        public TMP_Text rankTextMeshPro;
        public ScrollRect unlockedFunctions;
        public ScrollRect lockedFunctions;
        public ScrollRect completedLevels;

        Vector2 minSize;

        void Awake()
        {
            minSize = spriteRenderer.size;
        }

        public void SetText(string text)
        {
            SetDialogText(text);

            SaveManager.LoadGameData(out SaveData saveData);

            List<string> savedUnlockedFunctions = saveData.UnlockedFunctions.OrderBy(x => x).ToList();
            HashSet<string> unlockedFunctionsByHash = savedUnlockedFunctions.ToHashSet();

            string[] savedLockedFunctions = ExcelFunctions.AllFunctions.Where(x => !unlockedFunctionsByHash.Contains(x)).ToArray();

            int[] basic = BattleManager.GetQuestionsByRewardClassification(QuestionRewardClassification.BasicAggregates);
            int[] advanced = BattleManager.GetQuestionsByRewardClassification(QuestionRewardClassification.AdvancedAggregates);
            int[] expert = BattleManager.GetQuestionsByRewardClassification(QuestionRewardClassification.ExpertAggregates);
            int[] divine = BattleManager.GetQuestionsByRewardClassification(QuestionRewardClassification.DivineAggregates);



            List<int> pureCompletedQuestions = saveData.PureCompletedQuestions;
            List<int> completedQuestions = saveData.CompletedQuestions;

            int basicCount = completedQuestions.Where(x => basic.Contains(x)).Count();
            int advancedCount = completedQuestions.Where(x => advanced.Contains(x)).Count();
            int expertCount = completedQuestions.Where(x => expert.Contains(x)).Count();
            int divineCount = completedQuestions.Where(x => divine.Contains(x)).Count();

            string[] completedLevelFill = new string[4]
            {
                $"Easy: {basicCount}/{basic.Length}, {pureCompletedQuestions.Where(x => basic.Contains(x)).Count()}/{basic.Length}",
                $"Advanced: {advancedCount}/{advanced.Length}, {pureCompletedQuestions.Where(x => advanced.Contains(x)).Count()}/{advanced.Length}",
                $"Expert: {expertCount}/{expert.Length}, {pureCompletedQuestions.Where(x => expert.Contains(x)).Count()}/{expert.Length}",
                $"Divine: {divineCount}/{divine.Length}, {pureCompletedQuestions.Where(x => divine.Contains(x)).Count()}/{divine.Length}",
            };

            SetRank(basicCount, advancedCount, expertCount, divineCount);


            Fill(unlockedFunctions.content, savedUnlockedFunctions, 0, savedUnlockedFunctions.Count);
            Fill(lockedFunctions.content, savedLockedFunctions, 0, savedLockedFunctions.Length);
            Fill(completedLevels.content, completedLevelFill, 0, 4);
        }

        void SetRank(int basicCount, int advancedCount, int expertCount, int divineCount)
        {
            double points = basicCount * 10 + advancedCount * 30 + expertCount * 150 + divineCount * 500;
            string rank;

            if (points < 20) { rank = "Sheets User :("; }
            else if (points < 40) { rank = "Baby"; }
            else if (points < 60) { rank = "Toddler"; }
            else if (points < 80) { rank = "Child"; }
            else if (points < 100) { rank = "Beginner"; }
            else if (points < 120) { rank = "Average Joe"; }
            else if (points < 160) { rank = "Basic"; }
            else if (points < 240) { rank = "Probation"; }
            else if (points < 320) { rank = "Novice"; }
            else if (points < 400) { rank = "Unpaid Temp"; }
            else if (points < 480) { rank = "Apprentice"; }
            else if (points < 560) { rank = "Prodigy"; }
            else if (points < 640) { rank = "Coffee Fetcher"; }
            else if (points < 800) { rank = "Junior Analyst"; }
            else if (points < 960) { rank = "Analyst"; }
            else if (points < 1230) { rank = "Workhorse"; }
            else if (points < 1460) { rank = "Snr. Analyst"; }
            else if (points < 1740) { rank = "Team Lead"; }
            else if (points < 1960) { rank = "Consultant"; }
            else if (points < 1250) { rank = "Coffee Drinker"; }
            else if (points < 2460) { rank = "Snr. Consultant"; }
            else if (points < 2750) { rank = "I. Contributor"; }
            else if (points < 2960) { rank = "Manager"; }
            else if (points < 3250) { rank = "Expert"; }
            else if (points < 3460) { rank = "Head of Dept."; }
            else if (points < 4450) { rank = "Master"; }
            else if (points < 5450) { rank = "Grand Master"; }
            else if (points < 6440) { rank = "God"; }
            else if (points < 7430) { rank = "ANgaiAllator"; }
            else { rank = "Completionist :)"; }

            rankTextMeshPro.text = $"{rank} ({points})";
        }

        void Fill(RectTransform parent, IReadOnlyList<string> src, int start, int end)
        {
            int totalChildCount = parent.childCount;
            for (int i = 0; i < totalChildCount; i++)
            {
                Destroy(parent.GetChild(totalChildCount - i - 1).gameObject);
            }

            for (int i = start; i < end; ++i)
            {
                var t = Instantiate(itemTextMeshPro, parent);
                t.text = src[i];
                t.ForceMeshUpdate();                        // ensures the layout is current :contentReference[oaicite:1]{index=1}
            }
        }

        void SetDialogText(string text)
        {
            textMeshPro.text = text;
            ScaleBackgroundToFitText();
        }

        void Update()
        {
            ScaleBackgroundToFitText();
        }


        void ScaleBackgroundToFitText()
        {
            var s = (Vector2)textMeshPro.bounds.size;
            s += Vector2.one * padding;
            s.x = Mathf.Max(minSize.x, s.x);
            s.y = Mathf.Max(minSize.y, s.y);
            spriteRenderer.size = s;
        }

        public float GetHeight()
        {
            return spriteRenderer.size.y;
        }
    }
}
