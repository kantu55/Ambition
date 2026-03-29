using System;

namespace Ambition.Data.Runtime
{
    [Serializable]
    public class RuntimeOwnedEquipment
    {
        public int EquipId { get; private set; }
        public int RemainingDurabilityMonths { get; private set; }
        public RuntimeOwnedEquipment(int id, int durability)
        {
            EquipId = id;
            RemainingDurabilityMonths = durability;
        }
    }
}
