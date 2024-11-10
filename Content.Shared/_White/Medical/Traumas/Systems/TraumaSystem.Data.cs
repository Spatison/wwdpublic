﻿using Content.Shared.FixedPoint;
using Content.Shared._White.Medical.Wounds;

namespace Content.Shared._White.Medical.Traumas.Systems;

public partial class TraumaSystem
{
    #region Data

    private readonly Dictionary<BoneSeverity, FixedPoint2> _boneThresholds = new()
    {
        { BoneSeverity.Normal, 0 },
        { BoneSeverity.Damaged, 20 },
        { BoneSeverity.Broken, 50 },
    };

    private readonly Dictionary<BoneSeverity, FixedPoint2> _bonePainModifiers = new()
    {
        { BoneSeverity.Normal, 0.5 },
        { BoneSeverity.Damaged, 0.75 },
        { BoneSeverity.Broken, 1 },
    };

    private readonly Dictionary<WoundableSeverity, FixedPoint2> _boneTraumaChanceMultipliers = new()
    {
        { WoundableSeverity.Minor, 0.1 },
        { WoundableSeverity.Moderate, 0.3 },
        { WoundableSeverity.Severe, 0.5 },
        { WoundableSeverity.Critical, 0.7 },
        { WoundableSeverity.Loss, 0 }, // If we lost the woundable holding this bone, I guess we don't care, if is it broken.
    };

    private readonly Dictionary<WoundableSeverity, FixedPoint2> _boneDamageMultipliers = new()
    {
        { WoundableSeverity.Minor, 0.4 },
        { WoundableSeverity.Moderate, 0.6 },
        { WoundableSeverity.Severe, 0.9 },
        { WoundableSeverity.Critical, 1.25 },
        { WoundableSeverity.Loss, 1.6 }, // Fun.
    };

    #endregion
}
