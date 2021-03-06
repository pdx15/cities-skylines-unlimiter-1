﻿using ColossalFramework;
using ColossalFramework.Math;
using EightyOne.IgnoreAttributes;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

namespace EightyOne.Zones
{

    [TargetType(typeof(ZoneBlock))]
    internal struct FakeZoneBlock
    {
        [RedirectReverse(true)]
        private static void CheckBlock(ref ZoneBlock zoneBlock, ref ZoneBlock other, int[] xBuffer, ItemClass.Zone zone, Vector2 startPos, Vector2 xDir,
            Vector2 zDir, Quad2 quad)
        {
            UnityEngine.Debug.Log($"{zoneBlock}-{other}-{xBuffer}-{zone}-{startPos}-{xDir}-{zDir}-{quad}");
        }

        [RedirectReverse(true)]
        private static bool IsGoodPlace(ref ZoneBlock zoneBlock, Vector2 position)
        {
            UnityEngine.Debug.Log($"{zoneBlock}-{position}");
            return false;
        }

        [RedirectReverse(true)]
        private static void CalculateImplementation2(ref ZoneBlock zoneBlock, ushort blockID, ref ZoneBlock other, ref ulong valid, ref ulong shared, float minX, float minZ, float maxX, float maxZ)
        {
            UnityEngine.Debug.Log($"{zoneBlock}-{blockID}-{other}-{valid}-{shared}-{minX}-{minZ}-{maxX}-{maxZ}");
        }

        [RedirectMethod]
        public static void CalculateBlock2(ref ZoneBlock block, ushort blockID)
        {
            if (((int)block.m_flags & 3) != 1)
                return;
            int rowCount = block.RowCount;
            Vector2 vector2_1 = new Vector2(Mathf.Cos(block.m_angle), Mathf.Sin(block.m_angle)) * 8f;
            Vector2 vector2_2 = new Vector2(vector2_1.y, -vector2_1.x);
            Vector2 vector2_3 = VectorUtils.XZ(block.m_position);
            Vector2 vector2_4 = vector2_3 - 4f * vector2_1 - 4f * vector2_2;
            Vector2 vector2_5 = vector2_3 + 0.0f * vector2_1 - 4f * vector2_2;
            Vector2 vector2_6 = vector2_3 + 0.0f * vector2_1 + (float)(rowCount - 4) * vector2_2;
            Vector2 vector2_7 = vector2_3 - 4f * vector2_1 + (float)(rowCount - 4) * vector2_2;
            float minX = Mathf.Min(Mathf.Min(vector2_4.x, vector2_5.x), Mathf.Min(vector2_6.x, vector2_7.x));
            float minZ = Mathf.Min(Mathf.Min(vector2_4.y, vector2_5.y), Mathf.Min(vector2_6.y, vector2_7.y));
            float maxX = Mathf.Max(Mathf.Max(vector2_4.x, vector2_5.x), Mathf.Max(vector2_6.x, vector2_7.x));
            float maxZ = Mathf.Max(Mathf.Max(vector2_4.y, vector2_5.y), Mathf.Max(vector2_6.y, vector2_7.y));
            ulong valid = block.m_valid;
            ulong shared = 0UL;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            for (int index = 0; index < instance.m_cachedBlocks.m_size; ++index)
                //begin mod
                CalculateImplementation2(ref block, blockID, ref instance.m_cachedBlocks.m_buffer[index], ref valid, ref shared, minX, minZ, maxX, maxZ);
                //end mod
            //begin mod
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / 64f + FakeZoneManager.HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / 64f + FakeZoneManager.HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            //end mod
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    //begin mod
                    ushort num5 = instance.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                    //end mod
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 vector3 = instance.m_blocks.m_buffer[(int)num5].m_position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 46f - vector3.x, minZ - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)maxX - 46.0), (float)((double)vector3.z - (double)maxZ - 46.0))) < 0.0 && (int)num5 != (int)blockID)
                            //begin mod
                            CalculateImplementation2(ref block, blockID, ref instance.m_blocks.m_buffer[(int)num5], ref valid, ref shared, minX, minZ, maxX, maxZ);
                            //end mod
                        num5 = instance.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            ulong num7 = 144680345676153346UL;
            for (int index = 0; index < 7; ++index)
            {
                valid = (ulong)((long)valid & ~(long)num7 | (long)valid & (long)valid << 1 & (long)num7);
                num7 <<= 1;
            }
            block.m_valid = valid;
            block.m_shared = shared;
        }

        [RedirectMethod]
        [IgnoreIfBuildingThemesEnabled]
        public static void SimulationStep(ref ZoneBlock block, ushort blockID)
        {
            ZoneManager instance1 = Singleton<ZoneManager>.instance;
            int rowCount = block.RowCount;
            Vector2 xDir = new Vector2(Mathf.Cos(block.m_angle), Mathf.Sin(block.m_angle)) * 8f;
            Vector2 zDir = new Vector2(xDir.y, -xDir.x);
            ulong num1 = block.m_valid & (ulong)~((long)block.m_occupied1 | (long)block.m_occupied2);
            int z = 0;
            ItemClass.Zone zone = ItemClass.Zone.Unzoned;
            for (int index = 0; index < 4 && zone == ItemClass.Zone.Unzoned; ++index)
            {
                z = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)rowCount);
                if (((long)num1 & 1L << (z << 3)) != 0L)
                    zone = block.GetZone(0, z);
            }
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district1 = instance2.GetDistrict(block.m_position);
            int num2;
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    num2 = instance1.m_actualResidentialDemand + instance2.m_districts.m_buffer[(int)district1].CalculateResidentialLowDemandOffset();
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    num2 = instance1.m_actualResidentialDemand + instance2.m_districts.m_buffer[(int)district1].CalculateResidentialHighDemandOffset();
                    break;
                case ItemClass.Zone.CommercialLow:
                    num2 = instance1.m_actualCommercialDemand + instance2.m_districts.m_buffer[(int)district1].CalculateCommercialLowDemandOffset();
                    break;
                case ItemClass.Zone.CommercialHigh:
                    num2 = instance1.m_actualCommercialDemand + instance2.m_districts.m_buffer[(int)district1].CalculateCommercialHighDemandOffset();
                    break;
                case ItemClass.Zone.Industrial:
                    num2 = instance1.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[(int)district1].CalculateIndustrialDemandOffset();
                    break;
                case ItemClass.Zone.Office:
                    num2 = instance1.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[(int)district1].CalculateOfficeDemandOffset();
                    break;
                default:
                    return;
            }
            Vector2 vector2_1 = VectorUtils.XZ(block.m_position);
            Vector2 vector2_2 = vector2_1 - 3.5f * xDir + ((float)z - 3.5f) * zDir;
            int[] xBuffer = instance1.m_tmpXBuffer;
            for (int index = 0; index < 13; ++index)
                xBuffer[index] = 0;
            Quad2 quad = new Quad2();
            quad.a = vector2_1 - 4f * xDir + ((float)z - 10f) * zDir;
            quad.b = vector2_1 + 3f * xDir + ((float)z - 10f) * zDir;
            quad.c = vector2_1 + 3f * xDir + ((float)z + 2f) * zDir;
            quad.d = vector2_1 - 4f * xDir + ((float)z + 2f) * zDir;
            Vector2 vector2_3 = quad.Min();
            Vector2 vector2_4 = quad.Max();
            //begin mod
            int num3 = Mathf.Max((int)(((double)vector2_3.x - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
            int num4 = Mathf.Max((int)(((double)vector2_3.y - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
            int num5 = Mathf.Min((int)(((double)vector2_4.x + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num6 = Mathf.Min((int)(((double)vector2_4.y + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            //end mod
            for (int index1 = num4; index1 <= num6; ++index1)
            {
                for (int index2 = num3; index2 <= num5; ++index2)
                {
                    //begin mod
                    ushort num7 = instance1.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                    //end mod
                    int num8 = 0;
                    while ((int)num7 != 0)
                    {
                        Vector3 vector3 = instance1.m_blocks.m_buffer[(int)num7].m_position;
                        if ((double)Mathf.Max(Mathf.Max(vector2_3.x - 46f - vector3.x, vector2_3.y - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)vector2_4.x - 46.0), (float)((double)vector3.z - (double)vector2_4.y - 46.0))) < 0.0)
                            //begin mod
                            CheckBlock(ref block, ref instance1.m_blocks.m_buffer[(int)num7], xBuffer, zone, vector2_2, xDir, zDir, quad );
                            //end mod
                        num7 = instance1.m_blocks.m_buffer[(int)num7].m_nextGridBlock;
                        if (++num8 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int index = 0; index < 13; ++index)
            {
                uint num7 = (uint)xBuffer[index];
                int num8 = 0;
                bool flag1 = ((int)num7 & 196608) == 196608;
                bool flag2 = false;
                while (((int)num7 & 1) != 0)
                {
                    ++num8;
                    flag2 = ((int)num7 & 65536) != 0;
                    num7 >>= 1;
                }
                switch (num8)
                {
                case 5:
                case 6:
                    num8 = (!flag2 ? 4 : num8 - (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) + 2)) | 131072;
                    break;
                case 7:
                    num8 = 4 | 131072;
                    break;
                }
                if (flag1)
                    num8 |= 65536;
                xBuffer[index] = num8;
            }
            int num9 = xBuffer[6] & (int)ushort.MaxValue;
            if (num9 == 0)
                return;
            //begin mod
            bool flag3 = IsGoodPlace(ref block, vector2_2);
            //end mod
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) >= num2)
            {
                if (!flag3)
                {
                    return;
                }
                instance1.m_goodAreaFound[(int)zone] = (short)1024;
            }
            else if (!flag3 && (int)instance1.m_goodAreaFound[(int)zone] > -1024)
            {
                if ((int) instance1.m_goodAreaFound[(int) zone] != 0)
                {
                    return;
                }
                instance1.m_goodAreaFound[(int)zone] = (short)-1;
            }
            else
            {
                int index1 = 6;
                int index2 = 6;
                bool flag1 = true;
                while (true)
                {
                    if (flag1)
                    {
                        while (index1 != 0 && (xBuffer[index1 - 1] & (int)ushort.MaxValue) == num9)
                            --index1;
                        while (index2 != 12 && (xBuffer[index2 + 1] & (int)ushort.MaxValue) == num9)
                            ++index2;
                    }
                    else
                    {
                        while (index1 != 0 && (xBuffer[index1 - 1] & (int)ushort.MaxValue) >= num9)
                            --index1;
                        while (index2 != 12 && (xBuffer[index2 + 1] & (int)ushort.MaxValue) >= num9)
                            ++index2;
                    }
                    int num7 = index1;
                    int num8 = index2;
                    while (num7 != 0 && (xBuffer[num7 - 1] & (int)ushort.MaxValue) >= 2)
                        --num7;
                    while (num8 != 12 && (xBuffer[num8 + 1] & (int)ushort.MaxValue) >= 2)
                        ++num8;
                    bool flag2 = num7 != 0 && num7 == index1 - 1;
                    bool flag4 = num8 != 12 && num8 == index2 + 1;
                    if (flag2 && flag4)
                    {
                        if (index2 - index1 <= 2)
                        {
                            if (num9 <= 2)
                            {
                                if (!flag1)
                                    goto label_87;
                            }
                            else
                                --num9;
                        }
                        else
                            break;
                    }
                    else if (flag2)
                    {
                        if (index2 - index1 <= 1)
                        {
                            if (num9 <= 2)
                            {
                                if (!flag1)
                                    goto label_87;
                            }
                            else
                                --num9;
                        }
                        else
                            goto label_72;
                    }
                    else if (flag4)
                    {
                        if (index2 - index1 <= 1)
                        {
                            if (num9 <= 2)
                            {
                                if (!flag1)
                                    goto label_87;
                            }
                            else
                                --num9;
                        }
                        else
                            goto label_78;
                    }
                    else if (index1 == index2)
                    {
                        if (num9 <= 2)
                        {
                            if (!flag1)
                                goto label_87;
                        }
                        else
                            --num9;
                    }
                    else
                        goto label_87;
                    flag1 = false;
                }
                ++index1;
                --index2;
                  goto label_87;
            label_72:
                ++index1;
                goto label_87;
            label_78:
                --index2;
            label_87:
                int index3;
                int index4;
                if (num9 == 1 && index2 - index1 >= 1)
                {
                    index1 += Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)(index2 - index1));
                    index2 = index1 + 1;
                    index3 = index1 + Singleton<SimulationManager>.instance.m_randomizer.Int32(2U);
                    index4 = index3;
                }
                else
                {
                    do
                    {
                        index3 = index1;
                        index4 = index2;
                        if (index2 - index1 == 2)
                        {
                            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                --index4;
                            else
                                ++index3;
                        }
                        else if (index2 - index1 == 3)
                        {
                            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                index4 -= 2;
                            else
                                index3 += 2;
                        }
                        else if (index2 - index1 == 4)
                        {
                            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                            {
                                index2 -= 2;
                                index4 -= 3;
                            }
                            else
                            {
                                index1 += 2;
                                index3 += 3;
                            }
                        }
                        else if (index2 - index1 == 5)
                        {
                            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                            {
                                index2 -= 3;
                                index4 -= 2;
                            }
                            else
                            {
                                index1 += 3;
                                index3 += 2;
                            }
                        }
                        else if (index2 - index1 >= 6)
                        {
                            if (index1 == 0 || index2 == 12)
                            {
                                if (index1 == 0)
                                {
                                    index1 = 3;
                                    index3 = 2;
                                }
                                if (index2 == 12)
                                {
                                    index2 = 9;
                                    index4 = 10;
                                }
                            }
                            else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                            {
                                index2 = index1 + 3;
                                index4 = index3 + 2;
                            }
                            else
                            {
                                index1 = index2 - 3;
                                index3 = index4 - 2;
                            }
                        }
                    }
                    while (index2 - index1 > 3 || index4 - index3 > 3);
                }
                int a1 = 4;
                int num10 = index2 - index1 + 1;
                BuildingInfo.ZoningMode zoningMode1 = BuildingInfo.ZoningMode.Straight;
                bool flag5 = true;
                for (int index5 = index1; index5 <= index2; ++index5)
                {
                    a1 = Mathf.Min(a1, xBuffer[index5] & (int)ushort.MaxValue);
                    if ((xBuffer[index5] & 131072) == 0)
                        flag5 = false;
                }
                if (index2 > index1)
                {
                    if ((xBuffer[index1] & 65536) != 0)
                    {
                        zoningMode1 = BuildingInfo.ZoningMode.CornerLeft;
                        index4 = index1 + index4 - index3;
                        index3 = index1;
                    }
                    if ((xBuffer[index2] & 65536) != 0 && (zoningMode1 != BuildingInfo.ZoningMode.CornerLeft || Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0))
                    {
                        zoningMode1 = BuildingInfo.ZoningMode.CornerRight;
                        index3 = index2 + index3 - index4;
                        index4 = index2;
                    }
                }
                int a2 = 4;
                int num11 = index4 - index3 + 1;
                BuildingInfo.ZoningMode zoningMode2 = BuildingInfo.ZoningMode.Straight;
                bool flag6 = true;
                for (int index5 = index3; index5 <= index4; ++index5)
                {
                    a2 = Mathf.Min(a2, xBuffer[index5] & (int)ushort.MaxValue);
                    if ((xBuffer[index5] & 131072) == 0)
                        flag6 = false;
                }
                if (index4 > index3)
                {
                    if ((xBuffer[index3] & 65536) != 0)
                        zoningMode2 = BuildingInfo.ZoningMode.CornerLeft;
                    if ((xBuffer[index4] & 65536) != 0 && (zoningMode2 != BuildingInfo.ZoningMode.CornerLeft || Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0))
                        zoningMode2 = BuildingInfo.ZoningMode.CornerRight;
                }
                ItemClass.SubService subService = ItemClass.SubService.None;
                ItemClass.Level level = ItemClass.Level.Level1;
                ItemClass.Service service;
                switch (zone)
                {
                    case ItemClass.Zone.ResidentialLow:
                        service = ItemClass.Service.Residential;
                        subService = ItemClass.SubService.ResidentialLow;
                        break;
                    case ItemClass.Zone.ResidentialHigh:
                        service = ItemClass.Service.Residential;
                        subService = ItemClass.SubService.ResidentialHigh;
                        break;
                    case ItemClass.Zone.CommercialLow:
                        service = ItemClass.Service.Commercial;
                        subService = ItemClass.SubService.CommercialLow;
                        break;
                    case ItemClass.Zone.CommercialHigh:
                        service = ItemClass.Service.Commercial;
                        subService = ItemClass.SubService.CommercialHigh;
                        break;
                    case ItemClass.Zone.Industrial:
                        service = ItemClass.Service.Industrial;
                        break;
                    case ItemClass.Zone.Office:
                        service = ItemClass.Service.Office;
                        break;
                    default:
                        return;
                }
                BuildingInfo info = (BuildingInfo)null;
                Vector3 vector3 = Vector3.zero;
                int num12 = 0;
                int num13 = 0;
                int width = 0;
                BuildingInfo.ZoningMode zoningMode3 = BuildingInfo.ZoningMode.Straight;
                for (int index5 = 0; index5 < 6; ++index5)
                {
                    switch (index5)
                    {
                        case 0:
                            if (zoningMode1 != BuildingInfo.ZoningMode.Straight)
                            {
                                num12 = index1 + index2 + 1;
                                num13 = a1;
                                width = num10;
                                zoningMode3 = zoningMode1;
                                goto default;
                            }
                            else
                                break;
                        case 1:
                            if (zoningMode2 != BuildingInfo.ZoningMode.Straight)
                            {
                                num12 = index3 + index4 + 1;
                                num13 = a2;
                                width = num11;
                                zoningMode3 = zoningMode2;
                                goto default;
                            }
                            else
                                break;
                        case 2:
                            if (zoningMode1 != BuildingInfo.ZoningMode.Straight && a1 >= 4)
                            {
                                num12 = index1 + index2 + 1;
                                num13 = !flag5 ? 2 : 3;
                                width = num10;
                                zoningMode3 = zoningMode1;
                                goto default;
                            }
                            else
                                break;
                        case 3:
                            if (zoningMode2 != BuildingInfo.ZoningMode.Straight && a2 >= 4)
                            {
                                num12 = index3 + index4 + 1;
                                num13 = !flag6 ? 2 : 3;
                                width = num11;
                                zoningMode3 = zoningMode2;
                                goto default;
                            }
                            else
                                break;
                        case 4:
                            num12 = index1 + index2 + 1;
                            num13 = a1;
                            width = num10;
                            zoningMode3 = BuildingInfo.ZoningMode.Straight;
                            goto default;
                        case 5:
                            num12 = index3 + index4 + 1;
                            num13 = a2;
                            width = num11;
                            zoningMode3 = BuildingInfo.ZoningMode.Straight;
                            goto default;
                        default:
                            vector3 = block.m_position + VectorUtils.X_Y((float)((double)num13 * 0.5 - 4.0) * xDir + (float)((double)num12 * 0.5 + (double)z - 10.0) * zDir);
                            switch (zone)
                            {
                              case ItemClass.Zone.ResidentialLow:
                              case ItemClass.Zone.ResidentialHigh:
                                ZoneBlock.GetResidentialType(vector3, zone, width, num13, out subService, out level);
                                break;
                              case ItemClass.Zone.CommercialLow:
                              case ItemClass.Zone.CommercialHigh:
                                ZoneBlock.GetCommercialType(vector3, zone, width, num13, out subService, out level);
                                break;
                              case ItemClass.Zone.Industrial:
                                ZoneBlock.GetIndustryType(vector3, out subService, out level);
                                break;
                              case ItemClass.Zone.Office:
                                ZoneBlock.GetOfficeType(vector3, zone, width, num13, out subService, out level);
                                break;
                            }
                            byte district2 = instance2.GetDistrict(vector3);
                            ushort style = instance2.m_districts.m_buffer[(int)district2].m_Style;
                            if (Singleton<BuildingManager>.instance.m_BuildingWrapper != null)
                                Singleton<BuildingManager>.instance.m_BuildingWrapper.OnCalculateSpawn(vector3, ref service, ref subService, ref level, ref style);
                            info = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, width, num13, zoningMode3, (int)style);
                            if (info == null)
                                break;
                            goto label_165;
                    }
                }
            label_165:
                if (info == null || (double) Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(vector3)) > (double) vector3.y || Singleton<DisasterManager>.instance.IsEvacuating(vector3))
                {
                    return;
                }
                float angle = block.m_angle + 1.570796f;
                if (zoningMode3 == BuildingInfo.ZoningMode.CornerLeft && info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
                {
                    angle -= 1.570796f;
                    num13 = width;
                }
                else if (zoningMode3 == BuildingInfo.ZoningMode.CornerRight && info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
                {
                    angle += 1.570796f;
                    num13 = width;
                }
                ushort building;
                if (Singleton<BuildingManager>.instance.CreateBuilding(out building, ref Singleton<SimulationManager>.instance.m_randomizer, info, vector3, angle, num13, Singleton<SimulationManager>.instance.m_currentBuildIndex))
                {
                    ++Singleton<SimulationManager>.instance.m_currentBuildIndex;
                    switch (service)
                    {
                        case ItemClass.Service.Residential:
                            instance1.m_actualResidentialDemand = Mathf.Max(0, instance1.m_actualResidentialDemand - 5);
                            break;
                        case ItemClass.Service.Commercial:
                            instance1.m_actualCommercialDemand = Mathf.Max(0, instance1.m_actualCommercialDemand - 5);
                            break;
                        case ItemClass.Service.Industrial:
                            instance1.m_actualWorkplaceDemand = Mathf.Max(0, instance1.m_actualWorkplaceDemand - 5);
                            break;
                        case ItemClass.Service.Office:
                            instance1.m_actualWorkplaceDemand = Mathf.Max(0, instance1.m_actualWorkplaceDemand - 5);
                            break;
                    }
                    switch (zone)
                    {
                        case ItemClass.Zone.ResidentialHigh:
                        case ItemClass.Zone.CommercialHigh:
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_flags |= Building.Flags.HighDensity;
                            break;
                    }
                }
                instance1.m_goodAreaFound[(int)zone] = (short)1024;
            }
        }
    }
}
