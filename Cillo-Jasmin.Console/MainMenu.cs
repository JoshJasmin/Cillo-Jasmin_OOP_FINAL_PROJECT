using System;
using System.Collections.Generic;
using System.Linq;
using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;
using Cillo_Jasmin_OOP_FINAL_PROJ.Models;
using Cillo_Jasmin_OOP_FINAL_PROJ.Services;

namespace Cillo_Jasmin_Console
{
    public static class MainMenu
    {
        private const string DataFile = @"C:\Users\angel\OneDrive\Documents\GITHUB\Cillo-Jasmin_OOP_FINAL_PROJECT\Cillo-Jasmin_OOP_FINAL_PROJ\JsonAssetStorage.json";

        private static readonly AssetFileStorage Storage = new AssetFileStorage(DataFile);
        private static readonly AssetService Service = new AssetService(Storage);

        public static void Run()
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════╗");
                Console.WriteLine("║   CAMPUS IT ASSET & LAB TRACKER               ║");
                Console.WriteLine("╠═══════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Add New Asset                              ║");
                Console.WriteLine("║ 2. View All Assets                            ║");
                Console.WriteLine("║ 3. Update Asset Status                        ║");
                Console.WriteLine("║ 4. View Depreciation                          ║");
                Console.WriteLine("║ 5. Borrow/Checkout Asset                      ║");
                Console.WriteLine("║ 6. Return/Check-in Asset                      ║");
                Console.WriteLine("║ 7. View Networked Devices (LINQ)              ║");
                Console.WriteLine("║ 8. View Borrowed Items (LINQ)                 ║");
                Console.WriteLine("║ 0. Exit                                       ║");
                Console.WriteLine("╚═══════════════════════════════════════════════╝");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddAsset(); break;
                    case "2": ViewAssets(); break;
                    case "3": UpdateAsset(); break;
                    case "4": ViewDepreciation(); break;
                    case "5": CheckoutAsset(); break;
                    case "6": CheckInAsset(); break;
                    case "7": ViewNetworkedAssets(); break;
                    case "8": ViewBorrowedAssets(); break;
                    case "0": exit = true; break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void AddAsset()
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════╗");
            Console.WriteLine("║       ADD NEW ASSET       ║");
            Console.WriteLine("╠═══════════════════════════╣");
            Console.WriteLine("║ 1. Workstation            ║");
            Console.WriteLine("║ 2. Networking Hardware    ║");
            Console.WriteLine("║ 3. Peripheral             ║");
            Console.WriteLine("║ 4. Software License       ║");
            Console.WriteLine("╚═══════════════════════════╝");
            Console.Write("Select asset type: ");
            string type = Console.ReadLine();

            if (type != "1" && type != "2" && type != "3" && type != "4")
            {
                Console.WriteLine("Invalid type.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Asset Tag: ");
            string tag = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(tag))
            {
                Console.WriteLine("Asset tag cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Purchase Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Invalid price format.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Model/Name: ");
            string model = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(model))
            {
                Console.WriteLine("Model name cannot be empty.");
                Console.ReadKey();
                return;
            }

            try
            {
                CampusAsset newAsset;

                if (type == "1" || type == "2")
                {
                    Console.Write("Enter IP Address (e.g. 192.168.1.10): ");
                    string ip = Console.ReadLine();

                    Console.Write("Enter MAC Address (e.g. AA:BB:CC:DD:EE:FF): ");
                    string mac = Console.ReadLine();

                    if (!IsValidIp(ip))
                        throw new ArgumentException("Invalid IP address format.");

                    if (!IsValidMac(mac))
                        throw new ArgumentException("Invalid MAC address format.");

                    if (type == "1")
                    {
                        var ws = new Workstation(tag, price, model);
                        ws.AssignNetworkDetails(ip.Trim(), mac.Trim());
                        newAsset = ws;
                    }
                    else
                    {
                        var nh = new NetworkingHardware(tag, price, model);
                        nh.AssignNetworkDetails(ip.Trim(), mac.Trim());
                        newAsset = nh;
                    }
                }
                else if (type == "3")
                {
                    newAsset = new Peripheral(tag, price, model);
                }
                else
                {
                    Console.Write("Enter Months Remaining: ");
                    if (!int.TryParse(Console.ReadLine(), out int months) || months <= 0)
                        throw new ArgumentException("Invalid months. Must be a positive number.");

                    newAsset = new SoftwareLicense(tag, price, model, months);
                }

                Service.Add(newAsset);
                Console.WriteLine("Asset added and saved to disk successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadKey();
        }

        private static void ViewAssets()
        {
            Console.Clear();
            Console.WriteLine("--- ALL CAMPUS ASSETS ---");

            var assets = Service.GetAll();
            if (assets.Count == 0)
            {
                Console.WriteLine("No assets found.");
            }
            else
            {
                foreach (var asset in assets)
                {
                    Console.WriteLine(asset.ToString());

                    if (asset is INetworkable n)
                        Console.WriteLine($"   IP: {n.IpAddress ?? "N/A"} | MAC: {n.MacAddress ?? "N/A"}");

                    Console.WriteLine("-----------------------------------");
                }
            }
            Console.ReadKey();
        }

        private static void UpdateAsset()
        {
            Console.Clear();
            Console.WriteLine("--- UPDATE ASSET STATUS ---");
            Console.Write("Enter Asset Tag to update: ");
            string tag = Console.ReadLine();

            try
            {
                var asset = Service.GetByTag(tag);
                Console.WriteLine($"Current Status: {asset.Status}");
                Console.Write("Enter New Status (Available, In Repair, Unrepairable): ");
                string newStatus = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    Console.WriteLine("Status cannot be empty.");
                    Console.ReadKey();
                    return;
                }

                Service.UpdateStatus(tag, newStatus);
                Console.WriteLine("Status updated and saved to disk!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static void ViewDepreciation()
        {
            Console.Clear();
            Console.WriteLine("--- ASSET DEPRECIATION ---");

            var assets = Service.GetAll();
            if (assets.Count == 0)
            {
                Console.WriteLine("No assets found.");
            }
            else
            {
                foreach (var asset in assets)
                {
                    decimal depreciation = asset.CalculateDepreciation();
                    Console.WriteLine($"Tag: {asset.AssetTag} | Model: {asset.ModelName} | Depreciation Rate: {depreciation:C}");
                }
            }
            Console.ReadKey();
        }

        private static void CheckoutAsset()
        {
            Console.Clear();
            Console.WriteLine("--- CHECKOUT ASSET ---");
            Console.Write("Enter Asset Tag to checkout: ");
            string tag = Console.ReadLine();

            try
            {
                var asset = Service.GetByTag(tag);

                if (asset.Status == "Unrepairable")
                {
                    Console.WriteLine("This asset is unrepairable and cannot be checked out.");
                    Console.ReadKey();
                    return;
                }

                if (asset.Status == "In Repair")
                {
                    Console.WriteLine("This asset is currently in repair and cannot be checked out.");
                    Console.ReadKey();
                    return;
                }

                if (asset is IBorrowable borrowableAsset)
                {
                    if (borrowableAsset.IsCheckedOut)
                    {
                        Console.WriteLine($"This asset is already checked out to {borrowableAsset.BorrowerId}.");
                        Console.ReadKey();
                        return;
                    }

                    Console.Write("Enter Borrower ID: ");
                    string borrowerId = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(borrowerId))
                    {
                        Console.WriteLine("Borrower ID cannot be empty.");
                        Console.ReadKey();
                        return;
                    }

                    borrowableAsset.CheckOut(borrowerId);
                    Service.PersistChanges();
                    Console.WriteLine($"Successfully checked out to {borrowerId} and saved to disk.");
                }
                else
                {
                    Console.WriteLine("This asset type cannot be borrowed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Checkout failed: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static void CheckInAsset()
        {
            Console.Clear();
            Console.WriteLine("--- RETURN / CHECK-IN ASSET ---");
            Console.Write("Enter Asset Tag to return: ");
            string tag = Console.ReadLine();

            try
            {
                var asset = Service.GetByTag(tag);

                if (asset is not IBorrowable borrowableAsset)
                {
                    Console.WriteLine("This asset type cannot be checked in.");
                    Console.ReadKey();
                    return;
                }

                if (!borrowableAsset.IsCheckedOut)
                {
                    Console.WriteLine("This asset is not currently checked out.");
                    Console.ReadKey();
                    return;
                }

                if (asset.Status == "Unrepairable")
                {
                    Console.WriteLine("This asset is unrepairable and cannot be returned.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Currently checked out to: {borrowableAsset.BorrowerId}");
                Console.Write("Confirm return? (Y/N): ");
                string confirm = Console.ReadLine();

                if (!string.Equals(confirm, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Return cancelled.");
                    Console.ReadKey();
                    return;
                }

                borrowableAsset.CheckIn();
                Service.PersistChanges();
                Console.WriteLine("Asset returned successfully and saved to disk.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Check-in failed: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static bool IsValidIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;

            var parts = ip.Split('.');
            if (parts.Length != 4) return false;

            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out _)) return false;
            }

            return true;
        }

        private static bool IsValidMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return false;

            var parts = mac.Split(':');
            if (parts.Length != 6) return false;

            foreach (var part in parts)
            {
                if (part.Length != 2) return false;

                foreach (var ch in part)
                {
                    if (!Uri.IsHexDigit(ch)) return false;
                }
            }

            return true;
        }

        private static void ViewBorrowedAssets()
        {
            Console.Clear();
            Console.WriteLine("--- CURRENTLY BORROWED ASSETS ---");

            var borrowedItems = Service.GetCheckedOutBorrowables();

            if (borrowedItems.Count == 0)
            {
                Console.WriteLine("No items currently borrowed.");
            }
            else
            {
                foreach (var item in borrowedItems)
                {
                    var asset = (CampusAsset)item;
                    Console.WriteLine($"Tag: {asset.AssetTag} | Model: {asset.ModelName} | Borrower: {item.BorrowerId}");
                }
            }
            Console.ReadKey();
        }

        private static void ViewNetworkedAssets()
        {
            Console.Clear();
            Console.WriteLine("--- NETWORKED DEVICES ---");

            var networkDevices = Service.GetNetworkDevices();

            if (networkDevices.Count == 0)
            {
                Console.WriteLine("No networked devices found.");
            }
            else
            {
                foreach (var device in networkDevices)
                {
                    var asset = (CampusAsset)device;
                    Console.WriteLine($"Tag: {asset.AssetTag} | Model: {asset.ModelName} | IP: {device.IpAddress ?? "N/A"} | MAC: {device.MacAddress ?? "N/A"}");
                }
            }
            Console.ReadKey();
        }
    }
}