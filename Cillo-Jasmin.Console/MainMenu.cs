using System;
using System.Collections.Generic;
using System.Linq;
using Cillo_Jasmin_OOP_FINAL_PROJ.Models;
using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;

namespace Cillo_Jasmin_Console
{
    public static class MainMenu
    {
        private static List<CampusAsset> assets = new List<CampusAsset>();

        public static void Run()
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════╗");
                Console.WriteLine("║   CAMPUS IT ASSET & LAB TRACKER               ║");
                Console.WriteLine("║===============================================║");
                Console.WriteLine("║ 1. Add New Asset                              ║");
                Console.WriteLine("║ 2. View All Assets                            ║");
                Console.WriteLine("║ 3. Update Asset Status                        ║");
                Console.WriteLine("║ 4. View Depreciation                          ║");
                Console.WriteLine("║ 5. Borrow/Checkout Asset                      ║");
                Console.WriteLine("║ 6. View Networked Devices (LINQ)              ║");
                Console.WriteLine("║ 7. View Borrowed Items (LINQ)                 ║");
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
                    case "6": ViewNetworkedAssets(); break;
                    case "7": ViewBorrowedAssets(); break;
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
            Console.WriteLine("--- ADD NEW ASSET ---");
            Console.WriteLine("1. Workstation");
            Console.WriteLine("2. Networking Hardware");
            Console.WriteLine("3. Peripheral");
            Console.WriteLine("4. Software License");
            Console.Write("Select asset type: ");
            string type = Console.ReadLine();

            Console.Write("Enter Asset Tag: ");
            string tag = Console.ReadLine();

            Console.Write("Enter Purchase Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Invalid price format.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Model/Name: ");
            string model = Console.ReadLine();

            try
            {
                CampusAsset newAsset = null;

                switch (type)
                {
                    case "1": newAsset = new Workstation(tag, price, model); break;
                    case "2": newAsset = new NetworkingHardware(tag, price, model); break;
                    case "3": newAsset = new Peripheral(tag, price, model); break;
                    case "4":
                        Console.Write("Enter Months Remaining: ");
                        if (int.TryParse(Console.ReadLine(), out int months))
                            newAsset = new SoftwareLicense(tag, price, model, months); // <-- 4 arguments here
                        break;
                    default: Console.WriteLine("Invalid type."); return;
                }

                if (newAsset != null)
                {
                    assets.Add(newAsset);
                    Console.WriteLine("Asset added successfully!");
                }
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
            if (assets.Count == 0)
            {
                Console.WriteLine("No assets found.");
            }
            else
            {
                foreach (var asset in assets)
                {
                    Console.WriteLine(asset.ToString());
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

            var asset = assets.FirstOrDefault(a => a.AssetTag.Equals(tag, StringComparison.OrdinalIgnoreCase));

            if (asset != null)
            {
                Console.WriteLine($"Current Status: {asset.Status}");
                Console.Write("Enter New Status (Available, In Repair, Retired): ");
                string newStatus = Console.ReadLine();

                try
                {
                    asset.UpdateStatus(newStatus);
                    Console.WriteLine("Status updated successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update failed: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Asset not found.");
            }
            Console.ReadKey();
        }

        private static void ViewDepreciation()
        {
            Console.Clear();
            Console.WriteLine("--- ASSET DEPRECIATION ---");

            if (assets.Count == 0)
            {
                Console.WriteLine("No assets found.");
            }
            else
            {
                foreach (var asset in assets)
                {
                    // This demonstrates Polymorphism: calling the abstract method 
                    // which executes the specific logic for each derived class.
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

            var asset = assets.FirstOrDefault(a => a.AssetTag.Equals(tag, StringComparison.OrdinalIgnoreCase));

            if (asset != null)
            {
                if (asset is IBorrowable borrowableAsset)
                {
                    Console.Write("Enter Borrower ID: ");
                    string borrowerId = Console.ReadLine();

                    try
                    {
                        borrowableAsset.CheckOut(borrowerId);
                        Console.WriteLine($"Successfully checked out to {borrowerId}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Checkout failed: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("This asset type cannot be borrowed.");
                }
            }
            else
            {
                Console.WriteLine("Asset not found.");
            }
            Console.ReadKey();
        }

        private static void ViewBorrowedAssets()
        {
            Console.Clear();
            Console.WriteLine("--- CURRENTLY BORROWED ASSETS ---");

            // LINQ usage: Filtering assets that implement IBorrowable and are checked out
            var borrowedItems = assets.OfType<IBorrowable>().Where(b => b.IsCheckedOut).ToList();

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

            // LINQ usage: Filtering assets that implement INetworkable
            var networkDevices = assets.OfType<INetworkable>().ToList();

            if (networkDevices.Count == 0)
            {
                Console.WriteLine("No networked devices found.");
            }
            else
            {
                foreach (var device in networkDevices)
                {
                    var asset = (CampusAsset)device;
                    // Note: Assuming AssignNetworkDetails has been called at some point to set IP/MAC
                    Console.WriteLine($"Tag: {asset.AssetTag} | Model: {asset.ModelName} | IP: {device.IpAddress ?? "N/A"} | MAC: {device.MacAddress ?? "N/A"}");
                }
            }
            Console.ReadKey();
        }
    }
}