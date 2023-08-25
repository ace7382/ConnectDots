using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RewardChestType
{
    LEVELUP
    , SHOP_SMALL_SEGMENTS
    , SHOP_SMALL_EXP
    , SHOP_SINGLE_CATEGORY
    , SHOP_SMALL_RANDOM
    , POWERUP
}

public class RewardChest
{
    #region Classes
    
    public enum RewardType
    {
        BW_SEGMENTS
        , BW_EXP
        , POWERUP_FILLEMPTY
        , POWERUP_HINT
        , POWERUP_REMOVESPECIALTILE
        , CATEGORY_YELLOWSTAR
    }

    public class Reward
    {
        private RewardType  type;
        private int         minReward;  //-1 signifies that it's a non-range reward like unlocking a category
        private int         maxReward;  //-1 signifies that it's a non-range reward like unlocking a category
        private float       minRoll;
        private float       maxRoll;

        public RewardType   Type            { get { return type; } }
        public float        Chance          { get { return maxRoll - minRoll; } }
        public int          RewardRoll      { get { return Random.Range(minReward, maxReward + 1); } }

        public Reward(RewardType type, int minReward, int maxReward, float minRoll, float maxRoll)
        {
            this.type       = type;
            this.minReward  = minReward;
            this.maxReward  = maxReward;
            this.minRoll    = minRoll;
            this.maxRoll    = maxRoll;
        }

        public string GetPotentialRewardLineText()
        {
            switch (type)
            {
                case RewardType.BW_SEGMENTS:
                    return  string.Format(
                                "{0} {1} Segments"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                //, UIManager.instance.GetColorName(ColorCategory.BLACK_AND_WHITE)
                                , ColorCategory.BLACK_AND_WHITE.Name()
                            );
                case RewardType.BW_EXP:
                    return  string.Format(
                                "{0} {1} EXP"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                //, UIManager.instance.GetColorName(ColorCategory.BLACK_AND_WHITE)
                                , ColorCategory.BLACK_AND_WHITE.Name()
                            );
                case RewardType.POWERUP_HINT:
                    return  string.Format(
                                "{0} {1} Powerups"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                , PowerupType.HINT.Name()
                            );
                case RewardType.POWERUP_FILLEMPTY:
                    return  string.Format(
                                "{0} {1} Powerups"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                , PowerupType.FILL_EMPTY.Name()
                            );
                case RewardType.POWERUP_REMOVESPECIALTILE:
                    return  string.Format(
                                "{0} {1} Powerups"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                , PowerupType.REMOVE_SPECIAL_TILE.Name()
                            );
            }

            return "";
        }

        public string GetPrizeLineText()
        {
            switch (type)
            {
                case RewardType.BW_SEGMENTS:
                    return  string.Format(
                                "{0} Segments"
                                , ColorCategory.BLACK_AND_WHITE.Name()
                            );
                case RewardType.BW_EXP:
                    return  string.Format(
                                "{0} EXP"
                                , ColorCategory.BLACK_AND_WHITE.Name()
                            );
                case RewardType.POWERUP_HINT:
                    return  string.Format(
                                "{0} Powerups"
                                , PowerupType.HINT.Name()
                            );
                case RewardType.POWERUP_FILLEMPTY:
                    return  string.Format(
                                "{0} Powerups"
                                , PowerupType.FILL_EMPTY.Name()
                            );
                case RewardType.POWERUP_REMOVESPECIALTILE:
                    return  string.Format(
                                "{0} Powerups"
                                , PowerupType.REMOVE_SPECIAL_TILE.Name()
                            );
            }

            return "";
        }

        public bool IsRollInRange(float roll)
        {
            return roll >= minRoll && (roll < maxRoll || maxRoll == 100f && roll == maxRoll);
        }
    }

    #endregion

    #region Private Variables

    private RewardChestType chestType;

    #endregion

    #region Public Properties

    public RewardChestType ChestType { get { return chestType; } }

    #endregion

    #region Private Constructor

    private RewardChest(RewardChestType chestType)
    {
        this.chestType = chestType;
    }

    #endregion

    #region Static Functions

    public static RewardChest GetChest(RewardChestType chestType)
    {
        switch(chestType)
        {
            case RewardChestType.LEVELUP: return new RewardChest(RewardChestType.LEVELUP);
            case RewardChestType.POWERUP: return new RewardChest(RewardChestType.POWERUP);
        }

        return null;
    }

    #endregion

    #region Public Functions

    public int GetNumberOfRewards()
    {
        switch(chestType)
        {
            case    RewardChestType.LEVELUP:    return 3;
            case    RewardChestType.POWERUP:    return 3;
        }

        return -1;
    }

    public List<Reward> GetChestRewards()
    {
        switch(chestType)
        {
            case RewardChestType.LEVELUP:
            {
                return new List<Reward>()
                {
                      new Reward(RewardType.BW_SEGMENTS , 20    , 50    , 0f    , 50f)
                    , new Reward(RewardType.BW_SEGMENTS , 100   , 200   , 50f   , 75f)
                    , new Reward(RewardType.BW_SEGMENTS , 300   , 400   , 75f   , 87.5f)
                    , new Reward(RewardType.BW_SEGMENTS , 1000  , 1000  , 87.5f , 90f)
                    , new Reward(RewardType.BW_EXP      , 5     , 25    , 90f   , 100f)
                };
            }
            case RewardChestType.POWERUP:
            {
                return new List<Reward>()
                {
                      new Reward(RewardType.POWERUP_HINT                , 1     , 1     , 0f    , 25f)
                    , new Reward(RewardType.POWERUP_FILLEMPTY           , 1     , 1     , 25f   , 50f)
                    , new Reward(RewardType.POWERUP_REMOVESPECIALTILE   , 1     , 1     , 50f   , 75f)
                    , new Reward(RewardType.POWERUP_HINT                , 2     , 2     , 75f   , 83f)
                    , new Reward(RewardType.POWERUP_FILLEMPTY           , 2     , 2     , 83f   , 91f)
                    , new Reward(RewardType.POWERUP_REMOVESPECIALTILE   , 2     , 2     , 91f   , 99f)
                    , new Reward(RewardType.POWERUP_HINT                , 3     , 5     , 99f   , 100f)
                };
            }
        }

        return null;
    }

    public List<Reward> GetPrizes()
    {
        List<Reward> potentialRewards   = GetChestRewards();
        List<Reward> ret                = new List<Reward>();

        for (int i = 0; i < GetNumberOfRewards(); i++)
        {
            float roll                  = Random.Range(0f, 100f);

            Debug.Log("Reward Roll #" + i.ToString() + ": " + roll.ToString());

            Reward reward               = potentialRewards.Find(x => x.IsRollInRange(roll));

            ret.Add(reward);
        }

        return ret;
    }

    #endregion
}
