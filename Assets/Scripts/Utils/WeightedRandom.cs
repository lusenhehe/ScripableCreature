/// <author> lusen </author>
/// <date>2026-11-07</date>
/// <summary>
/// 加权随机工具。Unity 没有原生的通用加权抽取实现，这里自行封装。
/// 注意：Unity 无法序列化泛型类型，所以 Inspector 里使用的元素必须是下面的具体结构体。
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public struct WeightedInt
    {
        public int amount;
        public float weight;

        public WeightedInt(int amount, float weight)
        {
            this.amount = amount;
            this.weight = weight;
        }
    }

    [Serializable]
    public struct WeightedFloat
    {
        public float amount;
        public float weight;

        public WeightedFloat(float amount, float weight)
        {
            this.amount = amount;
            this.weight = weight;
        }
    }

    public static class WeightedRandom
    {
        /// <summary>按权重从列表中抽取一个元素；列表为空返回 default。</summary>
        public static T Pick<T>(IList<T> items, Func<T, float> weightSelector, System.Random rng)
        {
            if (items == null || items.Count == 0) return default;
            if (rng == null) rng = new System.Random();

            float total = 0f;
            for (int i = 0; i < items.Count; i++)
                total += Mathf.Max(0f, weightSelector(items[i]));

            // 权重全为 0 时退化为均匀抽取
            if (total <= 0f)
                return items[rng.Next(items.Count)];

            float roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < items.Count; i++)
            {
                roll -= Mathf.Max(0f, weightSelector(items[i]));
                if (roll <= 0f) return items[i];
            }
            return items[items.Count - 1];
        }

        public static int Pick(IList<WeightedInt> items, System.Random rng)
            => items == null || items.Count == 0 ? 0 : Pick<WeightedInt>(items, x => x.weight, rng).amount;

        public static float Pick(IList<WeightedFloat> items, System.Random rng)
            => items == null || items.Count == 0 ? 0f : Pick<WeightedFloat>(items, x => x.weight, rng).amount;
    }
}
