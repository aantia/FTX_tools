﻿
public class RootobjectPosition
{
    public bool success { get; set; }
    public Position[] result { get; set; }
}

public class Position
{
    public string future { get; set; }
    public float size { get; set; }
    public string side { get; set; }
    public float netSize { get; set; }
    public float longOrderSize { get; set; }
    public float shortOrderSize { get; set; }
    public float cost { get; set; }
    public float? entryPrice { get; set; }
    public float unrealizedPnl { get; set; }
    public float realizedPnl { get; set; }
    public float initialMarginRequirement { get; set; }
    public float maintenanceMarginRequirement { get; set; }
    public float openSize { get; set; }
    public float collateralUsed { get; set; }
    public float? estimatedLiquidationPrice { get; set; }
}
public class RootobjectBalance
{
    public bool success { get; set; }
    public Balance[] result { get; set; }
}

public class Balance
{
    public string coin { get; set; }
    public float free { get; set; }
    public float spotBorrow { get; set; }
    public float total { get; set; }
    public float usdValue { get; set; }
    public float availableWithoutBorrow { get; set; }
}

