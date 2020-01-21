﻿using System.Collections.Generic;

/// <summary>
/// Root类控制数据表
/// </summary>
public class Roots
{
    /// <summary>
    /// Test测试数据表
    /// </summary>
    public List<TestTableItem> TestTable { get; set; }

    /// <summary>
    /// 游戏类型数据表
    /// </summary>
    public List<GameTypeTableItem> GameTypeTable { get; set; }
}