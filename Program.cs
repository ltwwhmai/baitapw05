/****************/

// 202418940

// Lê Thanh Mai

/****************/  

using System;
using System.Collections.Generic;
using System.Linq;

// Định nghĩa tập hợp các trạng thái cố định của thiết bị trong vòng đời
public enum DeviceStatus
{
    Active,             // Đang hoạt động bình thường
    UnderMaintenance,   // Đang trong quá trình bảo trì, sửa chữa
    Retired             // Đã thanh lý, không còn sử dụng
}

// Phân loại máy in để hỗ trợ tính toán chi phí hoặc logic nghiệp vụ sau này
public enum PrinterType
{
    Laser,
    Inkjet
}

// Lớp cơ sở trừu tượng chứa các thuộc tính và logic dùng chung cho MỌI thiết bị
public abstract class Device
{
    public string Id { get; }
    public string Name { get; set; }
    
    // Chỉ cho phép gán giá trị khởi tạo một lần duy nhất qua constructor (chỉ có get)
    public int YearPutIntoUse { get; }
    public decimal PurchasePrice { get; }
    public DeviceStatus Status { get; set; }

    // Tự động tính số năm thiết bị đã hoạt động dựa trên năm hiện tại
    public int YearsInUse => DateTime.Now.Year - YearPutIntoUse;

    public Device(string id, string name, int yearPutIntoUse, decimal purchasePrice, DeviceStatus status)
    {
        // Ràng buộc tính toàn vẹn dữ liệu ngay khi đối tượng được tạo ra
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Mã thiết bị không được rỗng.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên thiết bị không được rỗng."); 
        if (purchasePrice <= 0) throw new ArgumentException("Giá mua phải lớn hơn 0.");
        if (yearPutIntoUse > DateTime.Now.Year) throw new ArgumentException("Năm sử dụng không lớn hơn năm hiện tại.");

        Id = id;
        Name = name;
        YearPutIntoUse = yearPutIntoUse;
        PurchasePrice = purchasePrice;
        Status = status;
    }

    // Phương thức trừu tượng bắt buộc các lớp con (Computer, Printer, Projector) phải tự định nghĩa cách tính phí riêng
    public abstract decimal CalculateMaintenanceCost();

    // Ghi đè phương thức ToString để khi in đối tượng ra console sẽ hiển thị chuỗi thông tin dễ đọc
    public override string ToString() => $"[{Id}] {Name} - {Status} (Year: {YearPutIntoUse}, Price: {PurchasePrice:C})";
}

// Interface định nghĩa "năng lực" kết nối mạng. Chỉ những máy nào thực thi interface này mới có các hàm Connect/Disconnect
public interface INetworkable
{
    string? IpAddress { get; } 
    void Connect(string ipAddress);
    void Disconnect();
    bool IsConnected { get; }
}

public class Computer : Device, INetworkable
{
    // Cấu hình các hằng số tỷ lệ phần trăm phí bảo trì (0.05m = 5%)
    private const decimal BASE_MAINTENANCE_RATE = 0.05m;
    private const decimal GPU_SURCHARGE_RATE = 0.02m;
    private const decimal AGE_SURCHARGE_RATE = 0.01m;

    public int RamCapacity { get; }
    public string ProcessorType { get; set; }
    public bool HasDedicatedGPU { get; set; }
    
    // Thuộc tính mạng chỉ có thể được thay đổi giá trị từ bên trong lớp này
    public string? IpAddress { get; private set; } 
    public bool IsConnected { get; private set; }

    public Computer(string id, string name, int year, decimal price, DeviceStatus status, int ram, string cpu, bool gpu)
        : base(id, name, year, price, status)
    {
        if (ram <= 0) throw new ArgumentException("RAM phải lớn hơn 0."); 
        RamCapacity = ram;
        ProcessorType = cpu;
        HasDedicatedGPU = gpu;
    }

    // Thực thi logic tính phí bảo trì đặc thù của máy tính
    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (HasDedicatedGPU) cost += PurchasePrice * GPU_SURCHARGE_RATE; // Phụ phí nếu có card đồ họa rời
        if (YearsInUse > 5) cost += PurchasePrice * AGE_SURCHARGE_RATE;  // Phụ phí nếu máy đã cũ
        return cost;
    }

    // Xử lý logic cấp phát IP và chuyển trạng thái kết nối
    public void Connect(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("IP không được rỗng.");
        if (IsConnected) throw new InvalidOperationException("Đang kết nối.");
        IpAddress = ipAddress;
        IsConnected = true;
    }

    // Xóa IP và ngắt trạng thái kết nối
    public void Disconnect()
    {
        IpAddress = null;
        IsConnected = false;
    }
}

public class Printer : Device
{
    private const decimal BASE_MAINTENANCE_RATE = 0.04m;
    private const decimal HEAVY_USAGE_SURCHARGE = 500000m;
    private const decimal COLOR_SURCHARGE = 300000m;

    public PrinterType PrinterType { get; set; } 
    public int PagesPrinted { get; }
    public bool IsColor { get; set; }

    public Printer(string id, string name, int year, decimal price, DeviceStatus status, PrinterType type, int pages, bool isColor)
        : base(id, name, year, price, status)
    {
        if (pages < 0) throw new ArgumentException("Số trang in không được âm.");
        PrinterType = type;
        PagesPrinted = pages;
        IsColor = isColor;
    }

    // Máy in có cách tính phí dựa trên số lượng bản in và màu sắc
    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (PagesPrinted > 100000) cost += HEAVY_USAGE_SURCHARGE; // Phụ phí hao mòn cao
        if (IsColor) cost += COLOR_SURCHARGE;                     // Phụ phí máy in màu
        return cost;
    }
}

// Lớp máy in có mạng (kế thừa lại toàn bộ đặc tính của máy in thường và bổ sung thêm khả năng mạng)
public class NetworkPrinter : Printer, INetworkable
{
    public string? IpAddress { get; private set; } 
    public bool IsConnected { get; private set; }

    // Gọi constructor của lớp cha (Printer) để khởi tạo các thuộc tính cơ bản
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
        IpAddress = null;
        IsConnected = false;
    }
}

public class Projector : Device
{
    private const decimal BASE_MAINTENANCE_RATE = 0.03m;
    private const decimal BULB_SURCHARGE = 1500000m;

    public int Brightness { get; }
    public int BulbHoursUsed { get; }

    public Projector(string id, string name, int year, decimal price, DeviceStatus status, int brightness, int bulbHours)
        : base(id, name, year, price, status)
    {
        if (brightness < 0) throw new ArgumentException("Độ sáng không được âm."); 
        if (bulbHours < 0) throw new ArgumentException("Số giờ dùng bóng đèn không được âm."); 
        Brightness = brightness;
        BulbHoursUsed = bulbHours;
    }

    // Phí bảo trì máy chiếu phụ thuộc chủ yếu vào tuổi thọ bóng đèn
    public override decimal CalculateMaintenanceCost()
    {
        decimal cost = PurchasePrice * BASE_MAINTENANCE_RATE;
        if (BulbHoursUsed > 3000) cost += BULB_SURCHARGE; // Thay bóng đèn mới nếu đã dùng quá số giờ quy định
        return cost;
    }
}

// Lớp quản lý phòng thực hành (Kết tập nhiều đối tượng Device)
public class LabRoom
{
    public string RoomId { get; }
    public string RoomName { get; set; }
    public int Capacity { get; set; }

    // Danh sách nội bộ chứa các thiết bị thực tế đang nằm trong phòng này
    private List<Device> _devices = new List<Device>();

    public LabRoom(string id, string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Mã phòng không được rỗng."); 
        if (capacity <= 0) throw new ArgumentException("Sức chứa phải lớn hơn 0."); 
        RoomId = id;
        RoomName = name;
        Capacity = capacity;
    }

    // Logic nghiệp vụ: Đưa một thiết bị vào phòng
    public void AddDevice(Device device)
    {
        if (device == null) throw new ArgumentNullException(nameof(device), "Thiết bị không được null.");
        
        // Kiểm tra xem mã thiết bị đã tồn tại trong phòng chưa để tránh trùng lặp dữ liệu
        if (_devices.Any(d => d.Id == device.Id)) throw new InvalidOperationException("Thiết bị trùng mã.");
        _devices.Add(device);
    }

    // Tính tổng phí bảo trì của TẤT CẢ thiết bị trong phòng nhờ cơ chế đa hình (Polymorphism)
    public decimal CalculateAnnualMaintenanceCost()
    {
        // Gọi CalculateMaintenanceCost() trên từng thiết bị. Hệ thống tự nhận diện loại máy và áp dụng công thức tương ứng.
        return _devices.Sum(d => d.CalculateMaintenanceCost());
    }

    public void PrintDevices()
    {
        Console.WriteLine($"\n--- Phòng {RoomName} ---");
        if (_devices.Count == 0)
        {
            Console.WriteLine("Phòng hiện chưa có thiết bị nào.");
            return;
        }
        // Gọi ToString() ngầm định của từng đối tượng
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

    // Trích xuất các thiết bị đạt điều kiện cần bảo dưỡng (đang ở trạng thái bảo trì hoặc đã quá 5 năm)
    public List<Device> GetDevicesRequiringMaintenance()
    {
        return _devices
            .Where(d => d.Status == DeviceStatus.UnderMaintenance || d.YearsInUse > 5)
            .ToList();
    }

    // Trả về danh sách thiết bị dạng read-only (IEnumerable) để bên ngoài đọc nhưng không thể can thiệp xóa/sửa trực tiếp
    public IEnumerable<Device> GetDevices() => _devices;
}

class Program
{
    static void Main()
    {
        // 1. Giai đoạn khởi tạo: Tạo ra các đối tượng thiết bị cụ thể trong bộ nhớ
        var comp1 = new Computer("C1", "PC-1", 2018, 15000000, DeviceStatus.Active, 16, "i5", false);
        var comp2 = new Computer("C2", "PC-2", 2023, 25000000, DeviceStatus.Active, 32, "i7", true);

        var print1 = new Printer("P1", "Laser Print", 2020, 5000000, DeviceStatus.Active, PrinterType.Laser, 120000, false);
        var print2 = new NetworkPrinter("P2", "Color Print", 2022, 8000000, DeviceStatus.Active, PrinterType.Inkjet, 50000, true);

        var proj1 = new Projector("Pr1", "Sony Proj", 2021, 10000000, DeviceStatus.Active, 3000, 3500);

        // 2. Phân bổ: Tạo phòng Lab A và đẩy máy móc vào phòng
        var room1 = new LabRoom("L1", "Lab A", 30);
        room1.AddDevice(comp1);
        room1.AddDevice(comp2);
        room1.AddDevice(print1);
        room1.AddDevice(proj1);

        // In danh sách và tổng phí (Test tính đa hình)
        room1.PrintDevices();
        Console.WriteLine($"Tổng phí bảo trì Lab A: {room1.CalculateAnnualMaintenanceCost():C}");

        // 3. Test thao tác mạng
        comp2.Connect("192.168.1.10");
        print2.Connect("192.168.1.20");
        Console.WriteLine($"IP Máy 2: {comp2.IpAddress}, IP Máy in mạng: {print2.IpAddress}");

        // 4. Test logic ngắt kết nối an toàn (gọi nhiều lần không gây crash)
        comp2.Disconnect();
        comp2.Disconnect();

        // 5. Test phòng Lab B
        var room2 = new LabRoom("L2", "Lab B", 25);
        room2.AddDevice(print2);

        // 6. Cố tình gây lỗi (Exception) để xem cơ chế chặn dữ liệu rác có hoạt động không
        try { room1.AddDevice(new Computer("C1", "PC-Trung", 2020, 1000000, DeviceStatus.Active, 8, "i3", false)); }
        catch (InvalidOperationException ex) { Console.WriteLine("Lỗi: " + ex.Message); } // Sẽ bắt được lỗi "Thiết bị trùng mã"

        room2.PrintDevices();
        Console.WriteLine($"Tổng phí bảo trì Lab B: {room2.CalculateAnnualMaintenanceCost():C}");

        // 7. Lọc danh sách thiết bị đến kỳ bảo trì
        foreach (var d in room1.GetDevicesRequiringMaintenance()) Console.WriteLine("Cần bảo trì: " + d);

        // 8. Test tính năng duyệt chung các máy có mạng (Sử dụng casting với OfType)
        var tat_ca = new List<Device> { comp1, comp2, print1, print2, proj1 };
        
        // Hệ thống sẽ tự lọc ra những máy nào implement INetworkable (Computer, NetworkPrinter)
        foreach (var n in tat_ca.OfType<INetworkable>())
        {
            if (!n.IsConnected) n.Connect("192.168.1.100");
            Console.WriteLine($"Thiết bị mạng: {n.IpAddress}");
        }
    }
}
