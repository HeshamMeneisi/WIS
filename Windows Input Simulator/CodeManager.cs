using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simulator;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Simulator
{
    class CodeManager
    {
        public static string GenerateCode(EventSet set)
        {
            string code = "EnsureResolution(" + set.Resolution.Width + "," + set.Resolution.Height + ");\n";
            if (!set.IgnoreKeyStats)
                code += "SpecialKeys(" + set.NumLockStat + "," + set.CapsLockStat + "," + set.ScrollLockStat + ");\n";
            if (set.ShowDesktop)
                code += "ShowDesktop();\n";
            if (set.Multiplier > 1)
                code += "SetMultiplier(" + set.Multiplier + ");\n";
            foreach (Event eve in set.GetEvents())
            {
                code += "@(" + eve.EventTime + ")do{" + Enum.GetName(typeof(EventType), eve.EventType) + "}using<" + ((eve.EventType == EventType.Wheel) ? ((WheelInfo)eve.Info).Point.ToString().Replace("X=", null).Replace("Y=", null).Trim('{', '}') + "|" + ((WheelInfo)eve.Info).Value: eve.Info.ToString().Replace("X=", null).Replace("Y=", null).Trim('{', '}')) + ">end;\n";
            }
            return code;
        }
        public static EventSet Interpret(string code)
        {
            EventSet set = new EventSet();
            // Resolution
            try
            {
                MatchCollection mc = Regex.Matches(code, "EnsureResolution(.*?);");
                if(mc.Count > 0)
                {
                    if (mc.Count == 1)
                    {
                        string st = mc[0].Value.Substring(17).Trim(';').Trim(')');
                        Size res = (Size)new SizeConverter().ConvertFromString(st);
                        set.Resolution = res;
                    }
                    else
                        throw new Exception("Multiple resolution signatures are NOT acceptable, there can be only one Resolution(w,h) signature in a single script which is called at the initiation of the script regardless of its position.");
                }
                else
                    throw new Exception("The code doesn't contain a resolution signature --EnsureResolution(w,h) missing.");
            }
            catch(Exception ex)
            {
                throw new Exception("Interpretation Error --" + ex.Message);
            }
            // RestoreMouse
            try
            {
                MatchCollection mc = Regex.Matches(code, "RestoreMouse(.*?);");
                if (mc.Count > 0)
                {
                    if (mc.Count == 1)
                        set.RestoreMouse = true;
                    else
                        throw new Exception("Multiple RestoreMouse() signatures are NOT acceptable.");
                }
                else
                    set.IgnoreKeyStats = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Combiling Error --" + ex.Message);
            }
            // Num, Caps, Scroll
            try
            {
                MatchCollection mc = Regex.Matches(code, "SpecialKeys(.*?);");
                if (mc.Count > 0)
                {
                    if (mc.Count == 1)
                    {
                        set.IgnoreKeyStats = false;
                        string[] vals = mc[0].Value.Substring(12).Trim(';').Trim(')').Split(',');
                        if (vals.Length == 3)
                        {
                            try
                            {
                                set.NumLockStat = bool.Parse(vals[0]);
                                set.CapsLockStat = bool.Parse(vals[1]);
                                set.ScrollLockStat = bool.Parse(vals[2]);
                            }
                            catch
                            {
                                throw new Exception("Invalid boolean values in SpecialKeys().");
                            }
                        }
                        else
                            throw new Exception("SpecialKeys(n,c,s) Accepts 3 true/false statements for NumLock, CapsLock and ScrollLock in that order.");
                    }
                    else
                        throw new Exception("Multiple SpecialKeys(n,c,s) signatures are NOT acceptable, there can be only one SpecialKeys(n,c,s) signature in a single script which is called at the initiation of the script regardless of its position, later modifications can be achieved by calling a KeyDown followed by a KeyUp event to the desired key.");
                }
                else
                    set.IgnoreKeyStats = true;
            }
            catch(Exception ex)
            {
                throw new Exception("Combiling Error --" + ex.Message);
            }
            // ShowDesktop
            try
            {
                MatchCollection mc = Regex.Matches(code, "ShowDesktop(.*?);");
                if (mc.Count > 0)
                {
                    if (mc.Count == 1)
                    {
                        set.ShowDesktop = true;
                    }
                    else
                        throw new Exception("Multiple ShowDesktop() signatures are NOT acceptable, there can be only one ShowDesktop() signature in a single script which is called at the initiation of the script regardless of its position.");
                }
                else
                    set.ShowDesktop = false;
            }
            catch (Exception ex)
            {
                throw new Exception("Combiling Error --" + ex.Message);
            }
            // Multiplier
            try
            {
                MatchCollection mc = Regex.Matches(code, "SetMultiplier(.*?);");
                if (mc.Count > 0)
                {
                    if (mc.Count == 1)
                    {
                        string st = mc[0].Value.Substring(14).Trim(';').Trim(')');
                        int m = 0;
                        if (!Int32.TryParse(st, out m))
                            throw new Exception("Invalid integral value in SetMultiplier()");
                        set.Multiplier = m;
                    }
                    else
                        throw new Exception("Multiple multiplier signatures are NOT acceptable, there can be only one SetMultiplier(n) signature in a single script which is called at the initiation of the script regardless of its position.");
                }
                else
                    set.Multiplier = 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Combiling Error --" + ex.Message);
            }
            MatchCollection col1 = Regex.Matches(code, "@(.*?)do{.*?}using<.*?>end;");
            foreach (Match match in col1)
            {
                try
                {
                    int time = Convert.ToInt32(Regex.Match(match.Value, "@(.*?)do").Value.Substring(2).Replace(")do", null));
                    EventType type = (EventType)Enum.Parse(typeof(EventType),(Regex.Match(match.Value, "do{.*?}using").Value.Substring(3).Replace("}using", null)),true);
                    object info;
                    if (type == EventType.KeyDown || type == EventType.KeyUp || type == EventType.ExecuteFile)
                        info = Regex.Match(match.Value, "using<.*?>end;").Value.Substring(6).Replace(">end;", null).Trim('"');
                    else if (type == EventType.Wheel)
                    {
                        string[] sa = Regex.Match(match.Value, "using<.*?>end;").Value.Substring(6).Replace(">end;", null).Split('|');
                        if (sa.Length == 2)
                            info = new WheelInfo((Point)new PointConverter().ConvertFromString(sa[0]), Convert.ToInt32(sa[1]));
                        else
                            throw new Exception("Invalid Wheel event call. -- " + string.Join("|",sa));
                    }
                    else
                        info = new PointConverter().ConvertFromString(Regex.Match(match.Value, "using<.*?>end;").Value.Substring(6).Replace(">end;", null));
                    set.AddEvent(time, type, info);
                }
                catch (Exception ex)
                {
                    throw new Exception("Combiling Error in (" + match.Value + ")\n" + ex.Message);
                }
            }
            return set;
        }
    }
}
