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
}

public class RewardChest
{
    #region Classes

    public enum RewardType
    {
        BW_SEGMENTS
        , BW_EXP
        , CATEGORY_YELLOWSTAR
    }

    public class Reward
    {
        private RewardType  type;
        private int         minReward;
        private int         maxReward;
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

        public string GetRewardLineText()
        {
            switch (type)
            {
                case RewardType.BW_SEGMENTS:
                    return  string.Format(
                                "{0} {1} Segments and a bunch more text to make this pretty long"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                , UIManager.instance.GetColorName(ColorCategory.BLACK_AND_WHITE)
                            );
                case RewardType.BW_EXP:
                    return  string.Format(
                                "{0} {1} EXP"
                                , minReward == maxReward ?
                                    minReward.ToString()
                                    : minReward.ToString() + " - " + maxReward.ToString()
                                , UIManager.instance.GetColorName(ColorCategory.BLACK_AND_WHITE)
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
        }

        return null;
    }

    #endregion

    #region Public Functions

    public int GetNumberOfRewards()
    {
        switch(chestType)
        {
            case (RewardChestType.LEVELUP):     return 3;
        }

        return -1;
    }

    public List<Reward> GetChestRewards()
    {
        switch(chestType)
        {
            case (RewardChestType.LEVELUP):
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
        }

        return null;
    }

    public List<(RewardType, int)> GetPrizes()
    {
        List<Reward> potentialRewards   = GetChestRewards();
        List<(RewardType, int)> ret     = new List<(RewardType, int)>();

        for (int i = 0; i < GetNumberOfRewards(); i++)
        {
            float roll                  = Random.Range(0f, 100f);

            Debug.Log("Reward Roll #" + i.ToString() + ": " + roll.ToString());

            Reward reward               = potentialRewards.Find(x => x.IsRollInRange(roll));

            ret.Add((reward.Type, reward.RewardRoll));
        }

        return ret;
    }

    #endregion
}
