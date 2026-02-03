using System;

namespace ValorantAimTrainer.Data
{
    [Serializable]
    public class SessionStats
    {
        public int TotalShots;
        public int Hits;
        public int Misses;
        public int Headshots;
        public int BodyShots;
        public int TargetsSpawned;
        public int TargetsHit;
        public int TargetsMissed;
        public float SessionDuration;
        public float TotalReactionTime;
        public int ReactionTimeCount;

        // Elimination mode specific
        public bool IsEliminationMode;
        public float CompletionTime;
        public int TotalTargetsToEliminate;

        public float Accuracy => TotalShots > 0 ? (float)Hits / TotalShots * 100f : 0f;
        public float HeadshotPercentage => Hits > 0 ? (float)Headshots / Hits * 100f : 0f;
        public float AverageReactionTime => ReactionTimeCount > 0 ? TotalReactionTime / ReactionTimeCount * 1000f : 0f;
        public float TargetsPerMinute => SessionDuration > 0 ? TargetsHit / (SessionDuration / 60f) : 0f;
        public int RemainingTargets => TotalTargetsToEliminate - TargetsHit;

        public void Reset()
        {
            TotalShots = 0;
            Hits = 0;
            Misses = 0;
            Headshots = 0;
            BodyShots = 0;
            TargetsSpawned = 0;
            TargetsHit = 0;
            TargetsMissed = 0;
            SessionDuration = 0f;
            TotalReactionTime = 0f;
            ReactionTimeCount = 0;
            IsEliminationMode = false;
            CompletionTime = 0f;
            TotalTargetsToEliminate = 0;
        }

        public void RegisterShot(bool isHit, bool isHeadshot)
        {
            TotalShots++;

            if (isHit)
            {
                Hits++;
                if (isHeadshot)
                    Headshots++;
                else
                    BodyShots++;
            }
            else
            {
                Misses++;
            }
        }

        public void RegisterTargetHit(float reactionTime)
        {
            TargetsHit++;
            TotalReactionTime += reactionTime;
            ReactionTimeCount++;
        }

        public void RegisterTargetMissed()
        {
            TargetsMissed++;
        }

        public void RegisterTargetSpawned()
        {
            TargetsSpawned++;
        }
    }
}
