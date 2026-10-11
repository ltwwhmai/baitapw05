```mermaid 
classDiagram
    class DeviceStatus {
        <<enumeration>>
        Active
        UnderMaintenance
        Retired
    }
    
    class PrinterType {
        <<enumeration>>
        Laser
        Inkjet
    }

    class INetworkable {
        <<interface>>
        +String IpAddress
        +Boolean IsConnected
        +Connect(ipAddress: String)
        +Disconnect()
    }

    class Device {
        <<abstract>>
        +String Id
        +String Name
        +int YearPutIntoUse
        +decimal PurchasePrice
        +DeviceStatus Status
        +int YearsInUse
        +CalculateMaintenanceCost()* decimal
        +ToString() String
    }

    class Computer {
        +int RamCapacity
        +String ProcessorType
        +Boolean HasDedicatedGPU
        +CalculateMaintenanceCost() decimal
    }

    class Printer {
        +PrinterType PrinterType
        +int PagesPrinted
        +Boolean IsColor
        +CalculateMaintenanceCost() decimal
    }
    
    class NetworkPrinter {
    }

    class Projector {
        +int Brightness
        +int BulbHoursUsed
        +CalculateMaintenanceCost() decimal
    }

    class LabRoom {
        +String RoomId
        +String RoomName
        +int Capacity
        -List~Device~ _devices
        +AddDevice(device: Device)
        +RemoveDevice(deviceId: String) Boolean
        +FindDevice(deviceId: String) Device
        +CalculateAnnualMaintenanceCost() decimal
        +GetDevicesRequiringMaintenance() List~Device~
        +PrintDevices()
    }

    Device <|-- Computer
    Device <|-- Printer
    Device <|-- Projector
    Printer <|-- NetworkPrinter

    INetworkable <|.. Computer
    INetworkable <|.. NetworkPrinter

    LabRoom o-- Device
