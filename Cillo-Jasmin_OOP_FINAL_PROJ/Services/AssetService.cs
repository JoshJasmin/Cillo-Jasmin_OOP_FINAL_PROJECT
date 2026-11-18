using System;
using System.Collections.Generic;
using System.Linq;
using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;
using Cillo_Jasmin_OOP_FINAL_PROJ.Models;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Services
{
    public class AssetService
    {
        private readonly Dictionary<string, CampusAsset> _assets =
            new Dictionary<string, CampusAsset>(StringComparer.OrdinalIgnoreCase);

        private readonly AssetFileStorage? _storage;

        public AssetService() { }

        public AssetService(AssetFileStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            foreach (var asset in storage.Load())
            {
                if (!_assets.ContainsKey(asset.AssetTag))
                    _assets[asset.AssetTag] = asset;
            }
        }

        private void Persist()
        {
            _storage?.Save(_assets.Values);
        }

        public IReadOnlyCollection<CampusAsset> GetAll() => _assets.Values.ToList();

        public CampusAsset GetByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || !_assets.TryGetValue(tag, out var asset))
                throw new ArgumentException($"Asset with tag '{tag}' was not found.");
            return asset;
        }

        public void Add(CampusAsset asset)
        {
            if (_assets.ContainsKey(asset.AssetTag))
                throw new ArgumentException($"Asset tag '{asset.AssetTag}' already exists.");

            if (asset is INetworkable networkable)
            {
                if (string.IsNullOrWhiteSpace(networkable.IpAddress) ||
                    string.IsNullOrWhiteSpace(networkable.MacAddress))
                    throw new ArgumentException("Networkable assets require an IP and MAC address.");
            }

            _assets[asset.AssetTag] = asset;
            Persist();
        }

        public bool Remove(string tag)
        {
            var removed = _assets.Remove(tag);
            if (removed) Persist();
            return removed;
        }

        public void UpdateStatus(string tag, string newStatus)
        {
            var asset = GetByTag(tag);
            asset.UpdateStatus(newStatus);
            Persist();
        }

        public void CheckOut(string tag, string borrowerId)
        {
            var asset = GetByTag(tag);

            if (asset is not IBorrowable borrowable)
                throw new ArgumentException("This asset is not borrowable.");

            if (asset.Status == "In Repair")
                throw new ArgumentException("Cannot checkout an asset that is in repair.");

            if (asset.Status == "Unrepairable")
                throw new ArgumentException("Cannot checkout an asset that is unrepairable.");

            if (borrowable.IsCheckedOut)
                throw new ArgumentException($"Asset is already checked out to {borrowable.BorrowerId}.");

            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new ArgumentException("Borrower ID is required.");

            borrowable.CheckOut(borrowerId);
            Persist();
        }

        public void CheckIn(string tag)
        {
            var asset = GetByTag(tag);

            if (asset is not IBorrowable borrowable)
                throw new ArgumentException("This asset is not borrowable.");

            if (!borrowable.IsCheckedOut)
                throw new ArgumentException("This asset is not currently checked out.");

            borrowable.CheckIn();
            Persist();
        }

        public void AssignNetworkDetails(string tag, string ip, string mac)
        {
            var asset = GetByTag(tag);

            if (asset is not INetworkable networkable)
                throw new ArgumentException("This asset does not support network details.");

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(mac))
                throw new ArgumentException("Both IP and MAC are required.");

            networkable.AssignNetworkDetails(ip.Trim(), mac.Trim());
            Persist();
        }

        public void PersistChanges() => Persist();

        public List<CampusAsset> SearchByName(string keyword)
        {
            return _assets.Values
                .Where(a => a.ModelName != null &&
                            a.ModelName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<CampusAsset> FilterByStatus(string status)
        {
            return _assets.Values
                .Where(a => a.Status != null &&
                            a.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<CampusAsset> FilterByType<T>() where T : CampusAsset
        {
            return _assets.Values.OfType<T>().Cast<CampusAsset>().ToList();
        }

        public List<IBorrowable> GetCheckedOutBorrowables()
        {
            return _assets.Values
                .OfType<IBorrowable>()
                .Where(b => b.IsCheckedOut)
                .ToList();
        }

        public List<INetworkable> GetNetworkDevices()
        {
            return _assets.Values.OfType<INetworkable>().ToList();
        }

        public decimal GetTotalDepreciation()
        {
            return _assets.Values.Sum(a => a.CalculateDepreciation());
        }

        public Dictionary<string, int> GetStatusSummary()
        {
            return _assets.Values
                .GroupBy(a => a.Status ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}