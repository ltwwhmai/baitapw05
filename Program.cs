using System;
using System.Collections.Generic;
using System.Linq;

// Enum cho trạng thái thiết bị
public enum DeviceStatus
{
    Active,
    UnderMaintenance,
    Retired
}

// Thêm enum cho loại máy in thay vì dùng string
public enum PrinterType
{
    Laser,
    Inkjet
}

// 1. Lớp trừu tượng Device
public abstract class Device
{
    public string Id { get; }
    public string Name { get; set; }
    public int YearPutIntoUse { get; set; }
    public decimal PurchasePrice { get; set; }
    public DeviceStatus Status { get; set; }

    // Chuyển thuộc tính YearsInUse lên lớp Device để GetDevicesRequiringMaintenance có thể truy cập
    public int YearsInUse => DateTime.Now.Year - YearPutIntoUse;

    public Device(string id, string name, int yearPutIntoUse, decimal purchasePrice, DeviceStatus status)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Mã thiết bị không được rỗng.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên thiết bị không được rỗng."); // Thêm validate Name
        if (purchasePrice <= 0) throw new ArgumentException("Giá mua phải lớn hơn 0.");
        if (yearPutIntoUse > DateTime.Now.Year) throw new ArgumentException("Năm sử dụng không lớn hơn năm hiện tại.");

        Id = id;
        Name = name;
        YearPutIntoUse = yearPutIntoUse;
        PurchasePrice = purchasePrice;
        Status = status;
    }

    public abstract decimal CalculateMaintenanceCost();

    public override string ToString() => $"[{Id}] {Name} - {Status} (Year: {YearPutIntoUse}, Price: {PurchasePrice:C})";
}

// 2. Interface INetworkable
public interface INetworkable
{
    string? IpAddress { get; } // Bổ sung nullable (string?)
    void Connect(string ipAddress);
    void Disconnect();
    bool IsConnected { get; }
}

// 3. Các lớp dẫn xuất
public class Computer : Device, INetworkable
{
    // Đặt hằng số (const) thay cho magic numbers
    private const decimal BASE_MAINTENANCE_RATE = 0.05m;
    private const decimal GPU_SURCHARGE_RATE = 0.02m;
    private const decimal AGE_SURCHARGE_RATE = 0.01m;

    public int RamCapacity { get; set; }
    public string ProcessorType { get; set; }
    public bool HasDedicatedGPU { get; set; }
    public string? IpAddress { get; private set; } // Bổ sung nullable
    public bool IsConnected { get; private set; }

    public Computer(string id, string name, int year, decimal price, DeviceStatus status, int ram, string cpu, bool gpu)
        : base(id, name, year, price, status)
    {
        if (ram <= 0) throw new ArgumentException("RAM phải lớn hơn 0."); // Thêm validate
        RamCapacity = ram;
        ProcessorType = cpu;
        HasDedicatedGPU = gpu;
    }

    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (HasDedicatedGPU) cost += PurchasePrice * GPU_SURCHARGE_RATE;
        if (YearsInUse > 5) cost += PurchasePrice * AGE_SURCHARGE_RATE; // Tái sử dụng YearsInUse
        return cost;
    }

    public void Connect(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("IP không được rỗng.");
        if (IsConnected) throw new InvalidOperationException("Đang kết nối.");
        IpAddress = ipAddress;
        IsConnected = true;
    }

    public void Disconnect()
    {
        // Ghi nhận hành vi khi ngắt kết nối nếu thiết bị chưa kết nối
        if (!IsConnected)
        {
            Console.WriteLine($"[{Id}] Thiết bị hiện chưa kết nối mạng.");
            return;
        }
        IpAddress = null;
        IsConnected = false;
    }
}

public class Printer : Device
{
    private const decimal BASE_MAINTENANCE_RATE = 0.04m;
    private const decimal HEAVY_USAGE_SURCHARGE = 500000m;
    private const decimal COLOR_SURCHARGE = 300000m;

    public PrinterType PrinterType { get; set; } // Thay string bằng enum
    public int PagesPrinted { get; set; }
    public bool IsColor { get; set; }

    public Printer(string id, string name, int year, decimal price, DeviceStatus status, PrinterType type, int pages, bool isColor)
        : base(id, name, year, price, status)
    {
        if (pages < 0) throw new ArgumentException("Số trang in không được âm."); // Thêm validate
        PrinterType = type;
        PagesPrinted = pages;
        IsColor = isColor;
    }

    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (PagesPrinted > 100000) cost += HEAVY_USAGE_SURCHARGE;
        if (IsColor) cost += COLOR_SURCHARGE;
        return cost;
    }
}

// NetworkPrinter kế thừa từ Printer và thực thi INetworkable
public class NetworkPrinter : Printer, INetworkable
{
    public string? IpAddress { get; private set; } // Bổ sung nullable
    public bool IsConnected { get; private set; }

    public NetworkPrinter(string id, string name, int year, decimal price, DeviceStatus status, PrinterType type, int pages, bool isColor)
        : base(id, name, year, price, status, type, pages, isColor) { }

    public void Connect(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("IP không được rỗng.");
        if (IsConnected) throw new InvalidOperationException("Đang kết nối.");
        IpAddress = ipAddress;
        IsConnected = true;
    }

    public void Disconnect()
    {
        if (!IsConnected)
        {
            Console.WriteLine($"[{Id}] Thiết bị hiện chưa kết nối mạng.");
            return;
        }
        IpAddress = null;
        IsConnected = false;
    }
}

public class Projector : Device
{
    private const decimal BASE_MAINTENANCE_RATE = 0.03m;
    private const decimal BULB_SURCHARGE = 1500000m;

    public int Brightness { get; set; }
    public int BulbHoursUsed { get; set; }

    public Projector(string id, string name, int year, decimal price, DeviceStatus status, int brightness, int bulbHours)
        : base(id, name, year, price, status)
    {
        if (brightness < 0) throw new ArgumentException("Độ sáng không được âm."); // Thêm validate
        if (bulbHours < 0) throw new ArgumentException("Số giờ dùng bóng đèn không được âm."); // Thêm validate
        Brightness = brightness;
        BulbHoursUsed = bulbHours;
    }

    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (BulbHoursUsed > 3000) cost += BULB_SURCHARGE;
        return cost;
    }
}

// 4. Lớp kết tập LabRoom
public class LabRoom
{
    public string RoomId { get; }
    public string RoomName { get; set; }
    public int Capacity { get; set; }

    // Đổi tên biến thành _devices cho đúng chuẩn private field
    private List<Device> _devices = new List<Device>();

    public LabRoom(string id, string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Mã phòng không được rỗng."); // Thêm validate
        if (capacity <= 0) throw new ArgumentException("Sức chứa phải lớn hơn 0."); // Thêm validate
        RoomId = id;
        RoomName = name;
        Capacity = capacity;
    }

    public void AddDevice(Device device)
    {
        if (device == null) throw new ArgumentNullException(nameof(device), "Thiết bị không được null.");
        if (_devices.Any(d => d.Id == device.Id)) throw new InvalidOperationException("Thiết bị trùng mã.");
        if (_devices.Count >= Capacity) throw new InvalidOperationException("Phòng đã đạt tối đa sức chứa."); // Validate sức chứa
        _devices.Add(device);
    }

    public decimal CalculateAnnualMaintenanceCost()
    {
        return _devices.Sum(d => d.CalculateMaintenanceCost());
    }

    public void PrintDevices()
    {
        Console.WriteLine($"\n--- Phòng {RoomName} ---");
        // Kiểm tra và in ra thông báo nếu phòng trống
        if (_devices.Count == 0)
        {
            Console.WriteLine("Phòng hiện chưa có thiết bị nào.");
            return;
        }
        foreach (var d in _devices) Console.WriteLine(d);
    }

    public bool RemoveDevice(string deviceId)
    {
        var d = FindDevice(deviceId);
        return d != null && _devices.Remove(d);
    }

    public Device? FindDevice(string deviceId)
    {
        return _devices.FirstOrDefault(d => d.Id == deviceId);
    }

    public List<Device> GetDevicesRequiringMaintenance()
    {
        return _devices
            .Where(d => d.Status == DeviceStatus.UnderMaintenance || d.YearsInUse > 5)
            .ToList();
    }

    public IEnumerable<Device> GetDevices() => _devices;
}

class Program
{
    static void Main()
    {
        var comp1 = new Computer("C1", "PC-1", 2018, 15000000, DeviceStatus.Active, 16, "i5", false);
        var comp2 = new Computer("C2", "PC-2", 2023, 25000000, DeviceStatus.Active, 32, "i7", true);

        // Khởi tạo máy in bằng enum thay vì chuỗi
        var print1 = new Printer("P1", "Laser Print", 2020, 5000000, DeviceStatus.Active, PrinterType.Laser, 120000, false);
        var print2 = new NetworkPrinter("P2", "Color Print", 2022, 8000000, DeviceStatus.Active, PrinterType.Inkjet, 50000, true);

        var proj1 = new Projector("Pr1", "Sony Proj", 2021, 10000000, DeviceStatus.Active, 3000, 3500);

        var room1 = new LabRoom("L1", "Lab A", 30);
        room1.AddDevice(comp1);
        room1.AddDevice(comp2);
        room1.AddDevice(print1);
        room1.AddDevice(proj1);

        room1.PrintDevices();
        Console.WriteLine($"Tổng phí bảo trì Lab A: {room1.CalculateAnnualMaintenanceCost():C}");

        comp2.Connect("192.168.1.10");
        print2.Connect("192.168.1.20");
        Console.WriteLine($"IP Máy 2: {comp2.IpAddress}, IP Máy in mạng: {print2.IpAddress}");

        // Test cảnh báo ngắt kết nối
        comp2.Disconnect();
        comp2.Disconnect();

        var room2 = new LabRoom("L2", "Lab B", 25);
        room2.AddDevice(print2);

        // Thử thêm trùng mã
        try { room1.AddDevice(new Computer("C1", "PC-Trung", 2020, 1000000, DeviceStatus.Active, 8, "i3", false)); }
        catch (InvalidOperationException ex) { Console.WriteLine("Lỗi: " + ex.Message); }

        room2.PrintDevices();
        Console.WriteLine($"Tổng phí bảo trì Lab B: {room2.CalculateAnnualMaintenanceCost():C}");

        // Thiết bị cần bảo trì
        foreach (var d in room1.GetDevicesRequiringMaintenance()) Console.WriteLine("Cần bảo trì: " + d);

        // Duyệt thiết bị mạng qua INetworkable
        var tat_ca = new List<Device> { comp1, comp2, print1, print2, proj1 };
        foreach (var n in tat_ca.OfType<INetworkable>())
        {
            if (!n.IsConnected) n.Connect("192.168.1.100");
            Console.WriteLine($"Thiết bị mạng: {n.IpAddress}");
        }
    }
}