using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Cillo_Jasmin_OOP_FINAL_PROJ.Models;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Services
{
    public class AssetFileStorage
    {
        public string FilePath { get; }

        public AssetFileStorage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            FilePath = filePath;
        }

        public void Save(IEnumerable<CampusAsset> assets)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("[");

                var list = new List<CampusAsset>(assets);
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    sb.Append("  {");
                    sb.Append($"\"type\":\"{GetTypeName(a)}\",");
                    sb.Append($"\"assetTag\":\"{Escape(a.AssetTag)}\",");
                    sb.Append($"\"purchasePrice\":{a.PurchasePrice.ToString(CultureInfo.InvariantCulture)},");
                    sb.Append($"\"modelName\":\"{Escape(a.ModelName)}\",");
                    sb.Append($"\"status\":\"{Escape(a.Status)}\"");

                    if (a is Workstation ws)
                    {
                        sb.Append($",\"ipAddress\":\"{Escape(ws.IpAddress)}\"");
                        sb.Append($",\"macAddress\":\"{Escape(ws.MacAddress)}\"");
                        sb.Append($",\"isCheckedOut\":{(ws.IsCheckedOut ? "true" : "false")}");
                        sb.Append($",\"borrowerId\":\"{Escape(ws.BorrowerId)}\"");
                    }
                    else if (a is NetworkingHardware nh)
                    {
                        sb.Append($",\"ipAddress\":\"{Escape(nh.IpAddress)}\"");
                        sb.Append($",\"macAddress\":\"{Escape(nh.MacAddress)}\"");
                    }
                    else if (a is Peripheral p)
                    {
                        sb.Append($",\"isCheckedOut\":{(p.IsCheckedOut ? "true" : "false")}");
                        sb.Append($",\"borrowerId\":\"{Escape(p.BorrowerId)}\"");
                    }
                    else if (a is SoftwareLicense sl)
                    {
                        sb.Append($",\"monthsRemaining\":{sl.MonthsRemaining}");
                    }

                    sb.Append("}");
                    if (i < list.Count - 1) sb.Append(",");
                    sb.AppendLine();
                }

                sb.AppendLine("]");

                var fullPath = Path.GetFullPath(FilePath);
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(FilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssetFileStorage.Save] FAILED: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public List<CampusAsset> Load()
        {
            var result = new List<CampusAsset>();

            try
            {
                if (!File.Exists(FilePath))
                    return result;

                var text = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(text))
                    return result;

                var objects = ExtractObjects(text);

                foreach (var obj in objects)
                {
                    try
                    {
                        var fields = ParseFields(obj);
                        if (!fields.TryGetValue("type", out var type)) continue;

                        var assetTag = fields.GetValueOrDefault("assetTag", "");
                        var modelName = fields.GetValueOrDefault("modelName", "");
                        var priceStr = fields.GetValueOrDefault("purchasePrice", "0");
                        var status = fields.GetValueOrDefault("status", "Available");
                        decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

                        CampusAsset asset;
                        switch (type.ToLowerInvariant())
                        {
                            case "workstation":
                                var ws = new Workstation(assetTag, price, modelName);
                                ws.AssignNetworkDetails(
                                    fields.GetValueOrDefault("ipAddress", null),
                                    fields.GetValueOrDefault("macAddress", null));
                                var wsCheckedOut = fields.GetValueOrDefault("isCheckedOut", "false") == "true";
                                ws.RestoreCheckoutState(wsCheckedOut, fields.GetValueOrDefault("borrowerId", null));
                                asset = ws;
                                break;

                            case "networking":
                                var nh = new NetworkingHardware(assetTag, price, modelName);
                                nh.RestoreNetworkState(
                                    fields.GetValueOrDefault("ipAddress", null),
                                    fields.GetValueOrDefault("macAddress", null));
                                asset = nh;
                                break;

                            case "peripheral":
                                var p = new Peripheral(assetTag, price, modelName);
                                var pCheckedOut = fields.GetValueOrDefault("isCheckedOut", "false") == "true";
                                p.RestoreCheckoutState(pCheckedOut, fields.GetValueOrDefault("borrowerId", null));
                                asset = p;
                                break;

                            case "software":
                                int.TryParse(fields.GetValueOrDefault("monthsRemaining", "12"), out var months);
                                asset = new SoftwareLicense(assetTag, price, modelName, months);
                                break;

                            default:
                                continue;
                        }

                        if (!string.IsNullOrWhiteSpace(status))
                            asset.UpdateStatus(status);

                        result.Add(asset);
                    }
                    catch
                    {
                        // Skip corrupted entry silently
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssetFileStorage.Load] FAILED: {ex.GetType().Name}: {ex.Message}");
            }

            return result;
        }

        private static string GetTypeName(CampusAsset a) => a switch
        {
            Workstation => "workstation",
            NetworkingHardware => "networking",
            Peripheral => "peripheral",
            SoftwareLicense => "software",
            _ => "unknown"
        };

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static List<string> ExtractObjects(string text)
        {
            var objects = new List<string>();
            int depth = 0;
            var current = new StringBuilder();

            foreach (char c in text)
            {
                if (c == '{')
                {
                    if (depth == 0) current.Clear();
                    depth++;
                    current.Append(c);
                }
                else if (c == '}')
                {
                    depth--;
                    current.Append(c);
                    if (depth == 0)
                        objects.Add(current.ToString());
                }
                else if (depth > 0)
                {
                    current.Append(c);
                }
            }
            return objects;
        }

        private static Dictionary<string, string> ParseFields(string objectText)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var inner = objectText.Trim().TrimStart('{').TrimEnd('}');

            int i = 0;
            while (i < inner.Length)
            {
                while (i < inner.Length && (char.IsWhiteSpace(inner[i]) || inner[i] == ',')) i++;
                if (i >= inner.Length) break;

                if (inner[i] != '"') break;
                int keyStart = ++i;
                while (i < inner.Length && inner[i] != '"') i++;
                var key = inner.Substring(keyStart, i - keyStart);
                i++;
                while (i < inner.Length && inner[i] != ':') i++;
                i++;

                string value;
                if (i < inner.Length && inner[i] == '"')
                {
                    i++;
                    var vsb = new StringBuilder();
                    while (i < inner.Length && inner[i] != '"')
                    {
                        if (inner[i] == '\\' && i + 1 < inner.Length)
                        {
                            vsb.Append(inner[i + 1]);
                            i += 2;
                        }
                        else
                        {
                            vsb.Append(inner[i]);
                            i++;
                        }
                    }
                    value = vsb.ToString();
                    i++;
                }
                else
                {
                    int valStart = i;
                    while (i < inner.Length && inner[i] != ',' && inner[i] != '}') i++;
                    value = inner.Substring(valStart, i - valStart).Trim();
                }

                dict[key] = value;
            }

            return dict;
        }
    }
}