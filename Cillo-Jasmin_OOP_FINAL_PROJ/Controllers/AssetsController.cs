using System;
using System.Linq;
using System.Net;
using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;
using Cillo_Jasmin_OOP_FINAL_PROJ.Models;
using Cillo_Jasmin_OOP_FINAL_PROJ.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AssetService _service;
        private readonly AssetFileStorage _storage;

        public AssetsController(AssetService service, AssetFileStorage storage)
        {
            _service = service;
            _storage = storage;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{tag}")]
        public IActionResult Get(string tag)
        {
            try { return Ok(_service.GetByTag(tag)); }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddAssetRequest req)
        {
            if (req == null) return BadRequest("Request body is required.");

            try
            {
                CampusAsset asset = req.Type?.ToLowerInvariant() switch
                {
                    "workstation" => new Workstation(req.AssetTag, req.Price, req.ModelName),
                    "networking" => new NetworkingHardware(req.AssetTag, req.Price, req.ModelName),
                    "peripheral" => new Peripheral(req.AssetTag, req.Price, req.ModelName),
                    "software" => new SoftwareLicense(req.AssetTag, req.Price, req.ModelName,
                                                        req.MonthsRemaining ?? 12),
                    _ => throw new ArgumentException("Invalid type.")
                };

                if (asset is INetworkable networkable)
                {
                    if (!IsValidIp(req.IpAddress))
                        throw new ArgumentException("Networkable assets require a valid IP address.");

                    if (!IsValidMac(req.MacAddress))
                        throw new ArgumentException("Networkable assets require a valid MAC address.");

                    networkable.AssignNetworkDetails(req.IpAddress.Trim(), req.MacAddress.Trim());
                }

                _service.Add(asset);
                return CreatedAtAction(nameof(Get), new { tag = asset.AssetTag }, asset);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{tag}")]
        public IActionResult Delete(string tag)
        {
            return _service.Remove(tag)
                ? Ok(new { message = "Deleted", tag })
                : NotFound($"Asset '{tag}' not found.");
        }

        [HttpPut("{tag}/status")]
        public IActionResult UpdateStatus(string tag, [FromBody] StatusRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.NewStatus))
                return BadRequest("NewStatus is required.");

            try
            {
                _service.UpdateStatus(tag, req.NewStatus);
                return Ok(new { message = "Status updated", tag, status = req.NewStatus });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("{tag}/checkout")]
        public IActionResult CheckOut(string tag, [FromBody] BorrowerRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.BorrowerId))
                return BadRequest("BorrowerId is required.");

            try
            {
                _service.CheckOut(tag, req.BorrowerId);
                return Ok(new { message = "Checked out", tag, borrower = req.BorrowerId });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("{tag}/checkin")]
        public IActionResult CheckIn(string tag)
        {
            try
            {
                _service.CheckIn(tag);
                return Ok(new { message = "Checked in", tag });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{tag}/network")]
        public IActionResult AssignNetwork(string tag, [FromBody] NetworkRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Ip) || string.IsNullOrWhiteSpace(req.Mac))
                return BadRequest("Ip and Mac are required.");

            if (!IsValidIp(req.Ip))
                return BadRequest("Invalid IP address format.");

            if (!IsValidMac(req.Mac))
                return BadRequest("Invalid MAC address format.");

            try
            {
                _service.AssignNetworkDetails(tag, req.Ip, req.Mac);
                return Ok(new { message = "Network details assigned", tag, req.Ip, req.Mac });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("checkedout")]
        public IActionResult GetCheckedOut()
        {
            var list = _service.GetCheckedOutBorrowables();
            return Ok(list.Select(b =>
            {
                var asset = b as CampusAsset;
                return new { Tag = asset?.AssetTag, Model = asset?.ModelName, b.BorrowerId, b.IsCheckedOut };
            }));
        }

        [HttpGet("network-devices")]
        public IActionResult GetNetworkDevices()
        {
            var list = _service.GetNetworkDevices();
            return Ok(list.Select(n =>
            {
                var asset = n as CampusAsset;
                return new { Tag = asset?.AssetTag, Model = asset?.ModelName, n.IpAddress, n.MacAddress };
            }));
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required.");
            return Ok(_service.SearchByName(q));
        }

        [HttpGet("filter/{status}")]
        public IActionResult FilterByStatus(string status) => Ok(_service.FilterByStatus(status));

        [HttpGet("type/{type}")]
        public IActionResult FilterByType(string type)
        {
            return type?.ToLowerInvariant() switch
            {
                "workstation" => Ok(_service.FilterByType<Workstation>()),
                "networking" => Ok(_service.FilterByType<NetworkingHardware>()),
                "peripheral" => Ok(_service.FilterByType<Peripheral>()),
                "software" => Ok(_service.FilterByType<SoftwareLicense>()),
                _ => BadRequest("Invalid type.")
            };
        }

        [HttpGet("depreciation")]
        public IActionResult TotalDepreciation() => Ok(new { totalDepreciation = _service.GetTotalDepreciation() });

        [HttpGet("status-summary")]
        public IActionResult StatusSummary() => Ok(_service.GetStatusSummary());

        [HttpGet("persist-path")]
        public IActionResult PersistPath() => Ok(new
        {
            path = _storage.FilePath,
            exists = System.IO.File.Exists(_storage.FilePath)
        });

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
    }

    public record AddAssetRequest(
        string AssetTag,
        string ModelName,
        decimal Price,
        string Type,
        int? MonthsRemaining,
        string? IpAddress,
        string? MacAddress);

    public record BorrowerRequest(string BorrowerId);
    public record StatusRequest(string NewStatus);
    public record NetworkRequest(string Ip, string Mac);
}